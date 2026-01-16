# GitHub Copilot Instructions - TC Agro Identity Service

## 📋 Project Context

**Name:** TC Agro Identity Service  
**Repository:** https://github.com/rdpresser/tc-agro-identity-service  
**Purpose:** Authentication, authorization, and user management for agricultural monitoring platform  
**Parent Project:** TC Agro Solutions - Phase 5 (Hackathon 8NETT FIAP)  
**Deadline:** February 27, 2026  
**Architecture:** Microservices on Kubernetes (k3d local, AKS production)

**Core Responsibilities:**

- User registration and authentication
- JWT token generation and validation
- Role-based authorization
- Password management (BCrypt)
- User profile management

---

## 🛠️ Technology Stack

### Backend

- **Language:** C# / .NET 10.0
- **Web Framework:** FastEndpoints (not MVC Controllers)
- **ORM:** Entity Framework Core 10.0
- **Database:** PostgreSQL (Azure PostgreSQL Flexible Server in production)
- **Cache:** Redis (Azure Redis Cache in production)
- **Messaging:** RabbitMQ local / Azure Service Bus production
- **Message Bus:** Wolverine + RabbitMQ
- **Password Hashing:** BCrypt.Net-Next
- **Validation:** FluentValidation

### Infrastructure

- **Local Orchestration:** k3d (Kubernetes)
- **Cloud Orchestration:** Azure Kubernetes Service (AKS)
- **GitOps:** ArgoCD
- **CI/CD:** GitHub Actions
- **Container Registry:** Docker Hub / Azure Container Registry

### Observability

- **Telemetry:** OpenTelemetry (OTLP, AspNetCore, Http, Runtime)
- **Metrics:** Prometheus
- **Logging:** Serilog (Console, Grafana Loki)
- **Tracing:** Distributed tracing with OpenTelemetry
- **APM:** Application Insights (production)

### Code Quality

- **Analyzers:** SonarAnalyzer.CSharp
- **Package Management:** Central Package Management (CPM)
- **Target Framework:** .NET 10.0 (enforced via Directory.Build.targets)
- **Nullable:** Enabled
- **Warnings as Errors:** Enabled

---

## 📝 C# Coding Conventions

### Project Structure

```
src/
├── Adapters/
│   ├── Inbound/
│   │   └── TC.Agro.Identity.Service/
│   │       ├── Endpoints/          # FastEndpoints
│   │       ├── Features/           # Handlers, Commands, Queries
│   │       ├── Program.cs
│   │       └── appsettings.json
│   └── Outbound/
│       └── (Infrastructure adapters)
└── Core/
    └── (Domain, Application layer)
```

### Naming Conventions

- **Namespaces:** `TC.Agro.Identity.{Layer}`
- **Classes:** PascalCase
- **Methods:** PascalCase
- **Local variables:** camelCase
- **Constants:** UPPER_CASE or PascalCase
- **Interfaces:** Prefix `I` (e.g., `IUserRepository`)

### FastEndpoints - Endpoint Structure

```csharp
using FastEndpoints;

public class RegisterUserEndpoint : Endpoint<RegisterUserRequest, RegisterUserResponse>
{
    private readonly IUserService _userService;

    public RegisterUserEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
        Description(b => b
            .Accepts<RegisterUserRequest>("application/json")
            .Produces<RegisterUserResponse>(201)
            .ProducesProblemDetails(400));
    }

    public override async Task HandleAsync(RegisterUserRequest req, CancellationToken ct)
    {
        var result = await _userService.RegisterAsync(req, ct);

        if (result.IsSuccess)
        {
            await SendCreatedAtAsync<GetUserEndpoint>(
                new { id = result.Value.Id },
                result.Value,
                cancellation: ct);
        }
        else
        {
            ThrowError(result.Errors.First());
        }
    }
}
```

### Entity Framework Core - User Entity

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}
```

### DbContext Configuration

```csharp
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
```

---

## 🔐 JWT Authentication

### Token Generation

```csharp
public class JwtTokenService
{
    private readonly IConfiguration _config;

    public string GenerateToken(User user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!)
        );
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Password Hashing with BCrypt

```csharp
public class PasswordService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
```

### JWT Configuration in Program.cs

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
            )
        };
    });

builder.Services.AddAuthorization();
```

---

## ✅ Validation with FluentValidation

```csharp
public class RegisterUserRequestValidator : Validator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("Password must contain at least one digit")
            .Matches(@"[@$!%*?&#]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100);
    }
}
```

---

## 🔍 Observability with Serilog & OpenTelemetry

### Structured Logging

```csharp
public class UserService
{
    private readonly ILogger<UserService> _logger;

    public async Task<Result<UserDto>> RegisterAsync(RegisterUserRequest request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Registering new user with email {Email}",
            request.Email
        );

        try
        {
            // ... registration logic ...

            _logger.LogInformation(
                "Successfully registered user {UserId} with email {Email}",
                user.Id,
                user.Email
            );

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to register user with email {Email}",
                request.Email
            );

            return Result<UserDto>.Error("Registration failed");
        }
    }
}
```

### OpenTelemetry Configuration

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddSource("Wolverine")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"]!);
            });
    });
```

---

## 🧪 Testing Patterns

### Unit Test - Service

```csharp
public class UserServiceTests
{
    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);
        var passwordService = new PasswordService();
        var service = new UserService(context, passwordService, Mock.Of<ILogger<UserService>>());

        var request = new RegisterUserRequest
        {
            Email = "test@example.com",
            Password = "Test@1234",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = await service.RegisterAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        user.Should().NotBeNull();
        user!.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsError()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);
        await context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash"
        });
        await context.SaveChangesAsync();

        var passwordService = new PasswordService();
        var service = new UserService(context, passwordService, Mock.Of<ILogger<UserService>>());

        var request = new RegisterUserRequest
        {
            Email = "test@example.com",
            Password = "Test@1234",
            FirstName = "Jane",
            LastName = "Doe"
        };

        // Act
        var result = await service.RegisterAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("already exists"));
    }
}
```

---

## 🎯 Important Rules

### ✅ ALWAYS Do:

- Use **FastEndpoints** for APIs (not MVC Controllers)
- Use **async/await** in all I/O operations
- Implement **structured logging** with Serilog
- Add **validation** with FluentValidation
- Use **DTOs** for requests/responses (do not expose entities)
- Hash passwords with **BCrypt** (workFactor: 12)
- Use **CancellationToken** in async methods
- Generate **secure JWT tokens** with proper claims
- Implement **OpenTelemetry** instrumentation
- Write **unit tests** for business logic
- Use **Ardalis.Result** pattern for success/error handling
- Follow **Central Package Management** (no versions in .csproj)

### ❌ NEVER Do:

- Use MVC Controllers (use FastEndpoints)
- Expose domain entities directly in APIs
- Store passwords in plain text (always hash with BCrypt)
- Hardcode JWT secrets (use configuration)
- Perform blocking synchronous operations
- Ignore error handling or validation
- Return sensitive data (passwords, hashes) in responses
- Log sensitive information (passwords, tokens)
- Add package versions in .csproj (use Directory.Packages.props)
- Use TargetFramework other than net10.0

### 🔐 Security:

- All endpoints except `/auth/login` and `/auth/register` require JWT
- Validate input on all endpoints with FluentValidation
- Use BCrypt with workFactor 12 for password hashing
- Never log passwords, tokens, or sensitive data
- Implement rate limiting for login endpoints
- Use HTTPS in production
- Validate JWT signature and claims

### 📈 Performance:

- Use Redis cache for user sessions (optional)
- Implement connection pooling for PostgreSQL
- Use indexes on email, status fields
- Enable async operations throughout
- Lazy loading disabled in EF Core (use explicit Include)

---

## 🚀 Useful Commands

### Local Development (k3d)

```bash
# Build Docker image
docker build -t localhost:5000/identity-service:latest -f src/Adapters/Inbound/TC.Agro.Identity.Service/Dockerfile .

# Push to local registry
docker push localhost:5000/identity-service:latest

# Apply Kubernetes manifests
kubectl apply -f k8s/

# Check pods
kubectl get pods -n agro

# View logs
kubectl logs -f <pod-name> -n agro

# Port forward for local testing
kubectl port-forward svc/identity-service 8080:80 -n agro
```

### Entity Framework Migrations

```bash
# Add new migration
dotnet ef migrations add <MigrationName> --project src/Adapters/Inbound/TC.Agro.Identity.Service

# Apply migrations
dotnet ef database update --project src/Adapters/Inbound/TC.Agro.Identity.Service

# Generate SQL script
dotnet ef migrations script --project src/Adapters/Inbound/TC.Agro.Identity.Service --output migration.sql
```

### Docker Commands

```bash
# Run locally (development)
docker run -p 8080:8080 --env-file .env identity-service:latest

# View container logs
docker logs -f <container-id>

# Stop container
docker stop <container-id>
```

### Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test
dotnet test --filter "FullyQualifiedName~UserServiceTests.RegisterAsync_ValidRequest_CreatesUser"
```

---

## 📝 Documentation and Language Standards

### Chat Responses

- **Match user's language:** Respond in the same language the user initiated the chat
  - If user starts in **Portuguese** → respond in Portuguese
  - If user starts in **English** → respond in English
- **Consistent language:** Maintain the chat language throughout the conversation

### Code and Documentation Files

- **No automatic .md file creation:** Do not create markdown documentation files unless explicitly requested
- **Suggest before creating:** If a .md file seems valuable, ask first or mention at the end
- **Visual summaries in chat:** Provide clear, visual summaries with emoji and formatting
- **Use English for all content:** All files, code, comments, filenames must use English
  - Exception: Chat responses follow user's language (see above)
  - This applies to: C# code/comments, variable names, file/folder names, documentation

---

## 📚 References

### Technology Documentation

- **FastEndpoints:** https://fast-endpoints.com/
- **EF Core 10:** https://learn.microsoft.com/ef/core/
- **Wolverine:** https://wolverine.netlify.app/
- **OpenTelemetry:** https://opentelemetry.io/docs/languages/net/
- **BCrypt.Net:** https://github.com/BcryptNet/bcrypt.net

### Project Resources

- **Repository:** https://github.com/rdpresser/tc-agro-identity-service
- **Parent Project:** TC Agro Solutions (Hackathon Phase 5)

---

> **Last update:** January 2026  
> **Version:** 1.0  
> Use these instructions to guide code generation in the TC Agro Identity Service project.
