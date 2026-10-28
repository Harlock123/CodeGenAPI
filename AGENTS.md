# AGENTS.md

Notes for anyone, human or AI agent, changing this code. `README.md` shows the Swagger UI in use and how connection
strings are passed; this file covers how the code is laid out and what to watch when changing it.

## What this is

An ASP.NET Core Web API (net9.0) that reads a SQL Server database's schema and generates source code from it: C#
model classes, Web API getter and POST methods, WinForms, HTML and XAML form definitions, create scripts, and a SQL
pretty-printer. Every endpoint returns generated text or schema lists; nothing is written to disk. Swagger UI is the
intended client.

## Commands

```bash
dotnet build CodeGenAPI.sln                  # global.json asks for SDK 7.0.0 or later (rolls forward to any major)
dotnet run --project CodeGenAPI              # https://localhost:7276 and http://localhost:5125
```

- Swagger UI is at `/swagger`, and only in the Development environment (the launch profiles set it).
- Endpoints take a `CN` query parameter: either a full connection string (starting with `Data Source`, `Server` or
  `DSN`) or the name of one under `Settings:Databases` in `appsettings.json`.
- There are no tests yet, and no CI workflow.

## Layout

- `CodeGenAPI/Program.cs`: controllers, Swagger with XML comments (`GenerateDocumentationFile` is on) and annotations.
- `CodeGenAPI/Controllers/SchemaDiscoveryController.cs`: almost all of the code, under `api/SchemaDiscovery/...`.
  - Schema endpoints: `GetAll`, `GetTables`, `GetTableSchema`, `GetListofTablePKs`, `GetIdentityFieldForTable` and
    others.
  - Generators: `GetTableModel`, `GetInterfaceClass...`, `GetGetterWebMethod...`, `GetPOSTMethodForTable`,
    `GetWINFORMsDefinitionForTable`, `GetHTMLFormDefinitionForTable`, `GetXAMLFormDefinition...`, `GetCreateScript`.
  - `FetchActualConnectionString` turns `CN` into a real connection string.
  - Generated code is built by string concatenation in each method.
- `CodeGenAPI/Models/Field.cs` (and the older `Controllers/Field1.cs`): one column's name, type, size and flags.
- `CodeGenAPI/Attributes/`, `Filters/`, `Services/`, `SecurityImplementations/`: an API-key filter (header `ApiKey`)
  and encryption helpers. The `[ApiKeyAuthorize]` attribute is commented out on the controller, so the API is open.
- `CodeGenAPI/SupportDLLs/SQL_Formatter.dll`: a .NET Framework 4.7.2 assembly used by `MakeSQLUglyPretty` and
  `MakeSQLUglyFromPostPretty`.
- `DatabaseScripts/`: a SQL Server script that creates a sample database (it drops it first).
- `CodeGenAPI/WeatherForecast*`: leftovers from the project template.

## Rules that keep it working

- **SQL Server only.** Schema reads use `System.Data.SqlClient` and SMO. The MySQL package and connection string
  are present but unused.
- **Integrated security needs Windows.** The SSPI connection strings in `appsettings.json` only work on Windows;
  elsewhere pass a SQL login connection string.
- **Keep XML doc comments.** Swagger reads `CodeGenAPI.xml`, and `[SwaggerOperation]` summaries describe each
  endpoint in the UI. New endpoints should have both.
- **A file under `bin/` is tracked.** `CodeGenAPI/bin/Debug/net6.0/Swagger-Custom.css` is committed (the project
  refers to it as `swagger-custom.css`); don't delete `bin/` wholesale.
- **Settings come from `appsettings.json` only.** The controller builds its own `ConfigurationBuilder` over that
  one file, so user secrets and environment variables are not seen. Keep real passwords out of it; pass a full
  connection string in `CN` instead.

## Making a change

- Add an endpoint as an `[HttpGet]`/`[HttpPost]` method with a `[Route("...")]` on `SchemaDiscoveryController`,
  taking `CN` with a named default like the others.
- Check the generated code by calling the endpoint through Swagger against a real database.
