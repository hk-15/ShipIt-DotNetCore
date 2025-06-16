# ShipIt Inventory Management

## Setup Instructions
Open the project in VSCode.
VSCode should automatically set up and install everything you'll need apart from the database connection!

### Setting up the Database.


Install PostgreSQL and pgAdmin downloaded from https://www.postgresql.org/download/windows/

This installer includes:

- The PostgreSQL server
- pgAdmin, a graphical tool for managing and developing your databases
- StackBuilder, a package manager for downloading and installing additional PostgreSQL tools and drivers. Stackbuilder includes management, integration, migration, replication, geospatial, connectors and other tools.

Create 2 new postgres databases in pgAdmin - one for the main program and one for our test database. Set the owner of the main database to `postgres`.
Ask a team member for a dump of the production databases to create and populate your tables.

Import the Database backup by right-clicking the main database in pgAdmin and selecting `Restore`. Change the format to `Plain` and select the dump file. Then, change the owner of the main database to an owner with a password (the program won't run otherwise).

Then for each of the projects, add a `.env` file at the root of the project.
That file should contain a property named `POSTGRES_CONNECTION_STRING`.
It should look something like this:
```
POSTGRES_CONNECTION_STRING="Server=localhost;Port=5432;Database=your_database_name;User Id=your_database_user; Password=your_database_password;"

```

## Running The API
The project uses .NET Core 3.1. To run the program use the command `dotnet run --roll-forward Major` in the ShipIt directory to run the program using the latest version of .NET installed on your dev machine.

## Running The Tests
To run the tests you should be able to run `dotnet test` in the ShipItTests directory.

## Deploying to Production
TODO
