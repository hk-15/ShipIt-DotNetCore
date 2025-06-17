# ShipIt Inventory Management

## Setup Instructions
Open the project in VSCode.

### Setting up the Database.

Install PostgreSQL and pgAdmin from https://www.postgresql.org/download/windows/

This installer includes:

- The PostgreSQL server
- pgAdmin, a graphical tool for managing and developing your databases
- StackBuilder, a package manager for downloading and installing additional PostgreSQL tools and drivers. Stackbuilder includes management, integration, migration, replication, geospatial, connectors and other tools.

Create a new Login/Group Role and set a password, then go to the Privileges tab and make sure `Can login?` and `Create databases?` are both enabled.

Then, create 2 new postgres databases in pgAdmin - one for the main program and one for our test database. Set the owner as the new role you have just added.

Ask a team member for a dump of the production databases to create and populate your tables. Import the dump by right-clicking each database in pgAdmin and selecting `Restore`. Change the format to `Plain` and select the dump file. Then, for each database, you will need to right-click each table and go to Properties and set the owner to the new role you created.

Then for each of the projects, add a `.env` file at the root of the project.
That file should contain a property named `POSTGRES_CONNECTION_STRING`.
It should look something like this:
```
POSTGRES_CONNECTION_STRING="Server=localhost;Port=5432;Database=your_database_name;User Id=your_database_user; Password=your_database_password;"

```

## Running The API
The project uses .NET Core 3.1 so you'll need to update the target framework in the .csproj file to the latest version of .NET installed on your dev machine.
To run the program use the command `dotnet run` 

## Running The Tests
Update the target framework as above, and run the tests with `dotnet test` in the ShipItTests directory.

## Deploying to Production
TODO
