# 🚗 CarRental API

A **car rental marketplace API** where users can list their cars for rent and book other users' cars — built with **.NET 8** and deployed on **Microsoft Azure**.

---

## 🔐 Test Credentials
Email:    test@user.com <br>
Password: Testing@user

---

## 🛠️ Tech Stack

- **.NET 8 Web API** — REST API framework
- **Entity Framework Core** — ORM with Code First migrations
- **SQL Server (Azure)** — Primary database
- **Redis (Azure Cache)** — Distributed caching
- **JWT Bearer Tokens** — Authentication
- **AutoMapper** — Object mapping
- **BCrypt** — Password hashing
- **Swagger** — API documentation

---

## 🏗️ Architecture

N-Tier Layered Architecture with strict one-directional dependencies:

```
API → Business → DataAccess → Domain
```

- **API** — Controllers, Middleware, JWT setup
- **Business** — Services, DTOs, AutoMapper profiles
- **DataAccess** — Repositories, EF Core, Redis cache
- **Domain** — Entities, Enums

---

## ✨ Features

- 🔐 **JWT Authentication** — Register and login returns a token immediately
- 👥 **Role Based Access Control** — User and Admin roles with protected endpoints
- 🚘 **Car Listings** — Any user can list, update, and delete their own cars
- 📅 **Booking System** — Book any car for a date range with auto price calculation
- ⚡ **Redis Caching** — Car listings cached with Cache-Aside pattern
- 🛡️ **Fraud Prevention** — Car details locked while a booking is active
- 🗑️ **Soft Delete** — Cars and users are never permanently removed
- 🔒 **Ownership Checks** — Users can only modify their own resources
- 👮 **Admin Panel** — Manage users, cars, and admin accounts
- 🌐 **Global Exception Handling** — Clean error responses across all endpoints

---

## 🔑 Authentication & Authorization

- Passwords hashed with **BCrypt** — never stored in plain text
- **JWT tokens** contain user ID, email, and role as claims
- Protected endpoints use `[Authorize]` attribute
- Admin endpoints use `[Authorize(Roles = "Admin")]`
- Deactivated users are blocked from logging in

---

## 📡 Key Endpoints

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| GET | `/api/cars` | Public |
| POST | `/api/cars` | Authenticated |
| PUT | `/api/cars/{id}` | Owner only |
| POST | `/api/bookings` | Authenticated |
| PUT | `/api/bookings/{id}/cancel` | Renter or Owner |
| GET | `/api/admin/users` | Admin only |

---

## 🚀 Getting Started

```bash
git clone https://github.com/Faris-5683/CarRental.git
cd CarRental

# Apply migrations
dotnet ef database update -p CarRental.DataAccess -s CarRental.API

# Run
dotnet run --project CarRental.API
```

Open Swagger at `https://localhost:{port}/swagger`

---

## ☁️ Deployment

Hosted on **Microsoft Azure**:
- **Azure App Service** — Linux, .NET 8
- **Azure SQL Server** — Managed database
- **Azure Cache for Redis** — Distributed cache

---

## 👤 Author

**Muhammad Faris Khan** — [@Faris-5683](https://github.com/Faris-5683)
