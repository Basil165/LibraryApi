# Library Management API (.NET 8)

This project is a RESTful Library Management System built using ASP.NET Core Web API.
It allows authenticated users to create, view, update, and delete books.

All book-related endpoints are protected using JWT authentication.

---

## Tech Stack
- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core
- SQL Server (Express / LocalDB)
- JWT Authentication
- Swagger (OpenAPI)
- Serilog (Logging)

---

## Requirements
- .NET 8 SDK
- SQL Server Express or LocalDB
- SQL Server Management Studio (optional)

---

## How to Run the Application



### Steps

1. Clone the repository
```bash
git clone https://github.com/Basil165/LibraryApi.git
cd LibraryApi

2. Configure the database connection

"DefaultConnection": "Server=.\\SQLEXPRESS;Database=LibraryDb;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet ef database update
Run the application

3. Create the database and tables

dotnet ef database update


