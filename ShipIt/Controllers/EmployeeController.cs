﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;

namespace ShipIt.Controllers
{
    [Route("employees")]
    public class EmployeeController : ControllerBase
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType
        );

        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        [HttpGet("")]
        public EmployeeResponse Get([FromQuery] string name)
        {
            Log.Info($"Looking up employee by name: {name}");
            var employees = _employeeRepository.GetEmployeesByName(name);
            var employeesList = new List<Employee>();
            foreach (var employee in employees)
            {
                var employeeModel = new Employee(employee);
                Log.Info("Found employee: " + employeeModel);
                employeesList.Add(employeeModel);
            }
            return new EmployeeResponse(employeesList);
        }

        [HttpGet("{warehouseId}")]
        public EmployeeResponse Get([FromRoute] int warehouseId)
        {
            Log.Info(String.Format("Looking up employee by id: {0}", warehouseId));

            var employees = _employeeRepository
                .GetEmployeesByWarehouseId(warehouseId)
                .Select(e => new Employee(e));

            Log.Info(String.Format("Found employees: {0}", employees));

            return new EmployeeResponse(employees);
        }

        [HttpPost("")]
        public Response Post([FromBody] AddEmployeesRequest requestModel)
        {
            List<Employee> employeesToAdd = requestModel.Employees;
            //List<Employee> duplicateEmployees = new List<Employee>();
            //List<Employee> employeesToAdd = new List<Employee>();

            if (employeesToAdd.Count == 0)
            {
                throw new MalformedRequestException("Expected at least one <employee> tag");
            }
            try
            {

                foreach (var employee in employeesToAdd)
                {
                    var existingEmployee = _employeeRepository.GetEmployeeByName(employee.Name);
                    if (existingEmployee != null)
                    {
                        string existingEmployeeRole = existingEmployee.Role.ToString().ToUpper();
                        string employeeRole = employee.role.ToString().Replace('_', ' ').ToUpper();

                        Console.WriteLine(existingEmployeeRole);
                        Console.WriteLine(employeeRole);

                        if ((existingEmployee.WarehouseId == employee.WarehouseId) && (existingEmployeeRole == employeeRole) && (existingEmployee.Ext == employee.ext))
                        {
                            Console.WriteLine("Duplicate found");
                            throw new MalformedRequestException("Duplicate employee found");
                        }

                    }
                }
                Log.Info("Adding employees: " + employeesToAdd);
                _employeeRepository.AddEmployees(employeesToAdd);
                Log.Debug("Employees added successfully");
                return new Response { Success = true };
            }
            catch (NoSuchEntityException)
            {
                Log.Info("Adding employees: " + employeesToAdd);
                _employeeRepository.AddEmployees(employeesToAdd);
                Log.Debug("Employees added successfully");
                return new Response { Success = true };
            }
               

        }

        [HttpDelete("")]
        public ObjectResult Delete([FromBody] RemoveEmployeeRequest requestModel)
        {
            string name = requestModel.Name;
            if (name == null)
            {
                throw new MalformedRequestException("Unable to parse name from request parameters");
            }

            var employees = _employeeRepository.GetEmployeesByName(name).ToList();
            if (employees.Count() > 1)
            {
                string jsonEmployeeResponse = JsonConvert.SerializeObject(employees);
                string errorMessageAndResponse =
                    "Two or more employees exist with this name, please use attached information to delete by ID"
                    + jsonEmployeeResponse;
                return BadRequest(errorMessageAndResponse);
            }

            try
            {
                _employeeRepository.RemoveEmployee(name);
                return Accepted();
            }
            catch (NoSuchEntityException)
            {
                throw new NoSuchEntityException("No employee exists with name: " + name);
            }
        }

        [HttpDelete("{id}")]
        public ObjectResult Delete([FromRoute] int id)
        {

            try
            {
                _employeeRepository.RemoveEmployeeById(id);
                return Accepted();
            }
            catch (NoSuchEntityException)
            {
                throw new NoSuchEntityException("No employee exists with Id: " + id);
            }
        }
    }
}
