# TC Agro Identity Service

Authentication, authorization, and user management for agricultural monitoring platform.

## 🎯 Purpose

Provides JWT-based authentication and user management for the TC Agro Solutions ecosystem.

**Core Features:**

- User registration and authentication
- JWT token generation and validation
- Role-based authorization (Admin, Farmer, Viewer)
- Password management with BCrypt
- User profile management

## 🛠️ Technology Stack

- **.NET 10.0** - Target framework
- **FastEndpoints** - Web API framework
- **Entity Framework Core 10.0** - ORM
- **PostgreSQL** - Database
- **BCrypt.Net** - Password hashing
- **FluentValidation** - Request validation
- **Wolverine** - Message bus (RabbitMQ local / Azure Service Bus production)
- **OpenTelemetry** - Observability (metrics, traces)
- **Serilog** - Structured logging
- **Ardalis.Result** - Result pattern

## 🏗️ Architecture

```
src/
├── Adapters/
│   ├── Inbound/
│   │   └── TC.Agro.Identity.Service/    # API layer (FastEndpoints)
│   └── Outbound/                        # Infrastructure adapters
└── Core/                                # Domain & Application logic
```

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK
- Docker (for PostgreSQL, Redis, RabbitMQ)
- k3d (optional, for local Kubernetes)

### Local Development

```bash
# Start infrastructure
docker compose up -d postgres redis rabbitmq

# Apply migrations
dotnet ef database update --project src/Adapters/Inbound/TC.Agro.Identity.Service

# Run service
dotnet run --project src/Adapters/Inbound/TC.Agro.Identity.Service
```

### Docker Build

```bash
# Build image (from repository root)
docker build -t identity-service:latest -f src/Adapters/Inbound/TC.Agro.Identity.Service/Dockerfile .

# Run container
docker run -p 8080:8080 identity-service:latest
```

## 🔐 Security

- **Password Hashing:** BCrypt with workFactor 12
- **JWT Tokens:** HS256 algorithm, 8-hour expiration
- **Authorization:** Role-based access control
- **Validation:** FluentValidation on all endpoints

## 📊 Key Endpoints

| Endpoint         | Method | Auth | Description         |
| ---------------- | ------ | ---- | ------------------- |
| `/auth/register` | POST   | No   | User registration   |
| `/auth/login`    | POST   | No   | User authentication |
| `/auth/refresh`  | POST   | Yes  | Refresh JWT token   |
| `/users/{id}`    | GET    | Yes  | Get user profile    |
| `/users/{id}`    | PUT    | Yes  | Update user profile |

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 📚 Documentation

- **Copilot Instructions:** [.github/copilot-instructions.md](.github/copilot-instructions.md)
- **Repository:** https://github.com/rdpresser/tc-agro-identity-service
- **Parent Project:** TC Agro Solutions (Hackathon Phase 5 - FIAP 8NETT)

## 🏷️ License

MIT License

---

> Part of TC Agro Solutions - Agricultural monitoring platform with IoT, sensor data processing, and dashboards.
