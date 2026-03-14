# Northwind Charts - Complete MVC Operations App

This solution provides a full-featured Northwind operations app with:

- Admin login (cookie authentication)
- Full CRUD for Products, Categories, Customers, Orders (+ lines), Suppliers, and Shippers
- Business rules for order validation and shipping (stock checks + stock deduction)
- Stored-procedure-backed sales dashboard APIs and charts

## Prerequisites

- .NET 8 SDK
- SQL Server with the `Northwind` database

## Database Setup (Stored Procedures)

Run these scripts against your `Northwind` database in order:

1. `database/sql/001_usp_DashboardTopProducts.sql`
2. `database/sql/002_usp_DashboardMonthlySales.sql`
3. `database/sql/003_usp_DashboardSalesByCategory.sql`
4. `database/sql/004_usp_DashboardOrdersByCountry.sql`

## Configure Connection + Admin Credentials

`NorthwindCharts.Mvc/appsettings.Development.json` includes local defaults.
Override credentials for local development via User Secrets:

```powershell
dotnet user-secrets init --project NorthwindCharts.Mvc/NorthwindCharts.Mvc.csproj
dotnet user-secrets set "AdminAuth:Username" "admin" --project NorthwindCharts.Mvc/NorthwindCharts.Mvc.csproj
dotnet user-secrets set "AdminAuth:Password" "your-strong-password" --project NorthwindCharts.Mvc/NorthwindCharts.Mvc.csproj
dotnet user-secrets set "ConnectionStrings:DbConnection" "Server=.;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=True;" --project NorthwindCharts.Mvc/NorthwindCharts.Mvc.csproj
```

## Build / Run / Test

```powershell
dotnet restore NorthwindCharts.Mvc/NorthwindCharts.Mvc.csproj
dotnet build NorthwindCharts.Mvc/NorthwindCharts.Mvc.csproj
dotnet test NorthwindCharts.Tests/NorthwindCharts.Tests.csproj
dotnet run --project NorthwindCharts.Mvc/NorthwindCharts.Mvc.csproj
```

Open `/Auth/Login` and sign in with configured admin credentials.
