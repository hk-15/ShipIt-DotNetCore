﻿﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Npgsql;
using ShipIt.Exceptions;
using ShipIt.Models.DataModels;

namespace ShipIt.Repositories
{
    public interface IProductRepository
    {
        int GetCount();
        ProductDataModel GetProductByGtin(string gtin);
        IEnumerable<ProductDataModel> GetProductsByGtin(List<string> gtins);
        IEnumerable<ProductDataModel> GetProductsById(IEnumerable<int> ids);
        ProductDataModel GetProductById(int id);
        void AddProducts(IEnumerable<ProductDataModel> products);
        void DiscontinueProductByGtin(string gtin);
        IEnumerable<ProductStockCompanyModel> GetProductsDetails(int id);
    }

    public class ProductRepository : RepositoryBase, IProductRepository
    {
        public int GetCount()
        {
            string EmployeeCountSQL = "SELECT COUNT(*) FROM gcp";
            return (int)QueryForLong(EmployeeCountSQL);
        }

        public ProductDataModel GetProductByGtin(string gtin)
        {
            string sql =
                "SELECT p_id, gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt FROM gtin WHERE gtin_cd = @gtin_cd";
            var parameter = new NpgsqlParameter("@gtin_cd", gtin);
            return base.RunSingleGetQuery(
                sql,
                reader => new ProductDataModel(reader),
                string.Format("No products found with gtin of value {0}", gtin),
                parameter
            );
        }

        public IEnumerable<ProductDataModel> GetProductsByGtin(List<string> gtins)
        {
            string sql = String.Format(
                "SELECT p_id, gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt FROM gtin WHERE gtin_cd IN ('{0}')",
                String.Join("','", gtins)
            );
            return base.RunGetQuery(
                sql,
                reader => new ProductDataModel(reader),
                "No products found with given gtin ids",
                null
            );
        }

        public IEnumerable<ProductDataModel> GetProductsById(IEnumerable<int> ids)
        {
            string sql = String.Format(
                "SELECT p_id, gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt FROM gtin WHERE p_id IN ('{0}')",
                String.Join("','", ids)
            );
            //var parameter = new NpgsqlParameter("@p_id", ids);
            string noProductWithIdErrorMessage = string.Format(
                "No products found with id of value {0}",
                ids.ToString()
            );
            return base.RunGetQuery(
                sql,
                reader => new ProductDataModel(reader),
                noProductWithIdErrorMessage,
                //parameter
                null
            );
        }

        public ProductDataModel GetProductById(int id)
        {
            string sql =
                "SELECT p_id, gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt FROM gtin WHERE p_id = @p_id";
            var parameter = new NpgsqlParameter("@p_id", id);
            string noProductWithIdErrorMessage = string.Format(
                "No products found with id of value {0}",
                id.ToString()
            );
            return RunSingleGetQuery(sql, reader => new ProductDataModel(reader), noProductWithIdErrorMessage, parameter);
        }

        public void DiscontinueProductByGtin(string gtin)
        {
            string sql = "UPDATE gtin SET ds = 1 WHERE gtin_cd = @gtin_cd";
            var parameter = new NpgsqlParameter("@gtin_cd", gtin);
            string noProductWithGtinErrorMessage = string.Format(
                "No products found with gtin of value {0}",
                gtin.ToString()
            );

            RunSingleQuery(sql, noProductWithGtinErrorMessage, parameter);
        }

        public void AddProducts(IEnumerable<ProductDataModel> products)
        {
            string sql =
                "INSERT INTO gtin (gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt) VALUES (@gtin_cd, @gcp_cd, @gtin_nm, @m_g, @l_th, @ds, @min_qt)";

            var parametersList = new List<NpgsqlParameter[]>();
            var gtins = new List<string>();

            foreach (var product in products)
            {
                if (gtins.Contains(product.Gtin))
                {
                    throw new MalformedRequestException(
                        string.Format("Cannot add products with duplicate gtins: {0}", product.Gtin)
                    );
                }
                gtins.Add(product.Gtin);
                parametersList.Add(product.GetNpgsqlParameters().ToArray());
            }

            var conflicts = TryGetProductsByGtin(gtins);
            if (conflicts.Any())
            {
                throw new MalformedRequestException(
                    string.Format(
                        "Cannot add products with existing gtins: {0}",
                        string.Join(", ", conflicts.Select(c => c.Gtin))
                    )
                );
            }

            RunTransaction(sql, parametersList);
        }

        private IEnumerable<ProductDataModel> TryGetProductsByGtin(List<string> gtins)
        {
            try
            {
                var products = GetProductsByGtin(gtins).ToList();
                return products;
            }
            catch (NoSuchEntityException)
            {
                return new List<ProductDataModel>();
            }
        }

        public IEnumerable<ProductStockCompanyModel> GetProductsDetails(int id)
        {
            string sql =
                "SELECT stock.p_id, hld, w_id, gtin_cd, gtin.gcp_cd, gtin_nm, m_g, l_th, ds, min_qt, gln_nm, gln_addr_02, gln_addr_03, gln_addr_04, gln_addr_postalcode, gln_addr_city, contact_tel, contact_mail FROM stock JOIN gtin ON stock.p_id = gtin.p_id JOIN gcp ON gtin.gcp_cd = gcp.gcp_cd WHERE stock.w_id = @w_id";
            var parameter = new NpgsqlParameter("@w_id", id);
            string noProductsWithIdErrorMessage = string.Format(
                "No products found in warehouse with id of value {0}",
                id.ToString()
            );
            return RunGetQuery(sql, reader => new ProductStockCompanyModel(reader), noProductsWithIdErrorMessage, parameter);
        }
    }
}
