# CRM Backend

This is an ASP.NET Core Web API project for the CRM backend.

## Prerequisites

- .NET 10.0 SDK

## Getting Started

1. Restore dependencies:
   ```
   dotnet restore
   ```

2. Build the project:
   ```
   dotnet build
   ```

3. Run the application:
   ```
   dotnet run
   ```

The API will be available at `https://localhost:5001` (HTTPS) and `http://localhost:5000` (HTTP).

## GraphQL

The project includes GraphQL support using Hot Chocolate.

- GraphQL endpoint: `/graphql`
- Banana Cake Pop UI: `/graphql` (in development mode)

Example query:
```graphql
{
  hello
}
```

## Development

- Use Visual Studio Code with the C# Dev Kit extension for the best development experience.
- The project includes Swagger UI for API testing at `/swagger` when running in development mode.

## Project Structure

- `Controllers/`: API controllers
- `Models/`: Data models
- `Services/`: Business logic services
- `appsettings.json`: Configuration files

## HTTPS Certificate

The project is configured with HTTPS. To trust the development certificate, run:
```
dotnet dev-certs https --trust
```