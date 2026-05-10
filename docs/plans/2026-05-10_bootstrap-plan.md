# 📋 Project Implementation Plan
## Generic Azure Cosmos DB Document Management Application

> **Stack:** .NET 10 · ASP.NET Core MVC · Razor Views · Azure Cosmos DB SDK · Bootstrap 5  
> **Architecture:** Clean Architecture · SOLID · Dependency Injection · Async/Await  
> **Version:** 1.0.0 | **Status:** Planning

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Solution Structure](#2-solution-structure)
3. [Layer Definitions](#3-layer-definitions)
4. [Domain Layer](#4-domain-layer)
5. [Application Layer](#5-application-layer)
6. [Infrastructure Layer](#6-infrastructure-layer)
7. [Web (Presentation) Layer](#7-web-presentation-layer)
8. [Configuration & Secrets Management](#8-configuration--secrets-management)
9. [UI Design & Pages](#9-ui-design--pages)
10. [Validation & Error Handling](#10-validation--error-handling)
11. [Patch Operations](#11-patch-operations)
12. [Logging & Observability](#12-logging--observability)
13. [Swagger / OpenAPI](#13-swagger--openapi)
14. [Retry & Resilience](#14-retry--resilience)
15. [Security Considerations](#15-security-considerations)
16. [Testing Strategy](#16-testing-strategy)
17. [README Requirements](#17-readme-requirements)
18. [Implementation Phases](#18-implementation-phases)
19. [File & Folder Reference](#19-file--folder-reference)
20. [Sample Requests & Responses](#20-sample-requests--responses)

---

## 1. Project Overview

### Goal
Build a reusable, configurable, and production-ready .NET 10 web application that allows any user to connect to **any Azure Cosmos DB instance** and perform generic document operations — without writing or modifying any code.

### Core Operations
| Operation | Description |
|-----------|-------------|
| **Insert** | Add a new JSON document to a container |
| **Upsert** | Insert or replace a document by ID |
| **Get / View** | Retrieve an existing document by ID + partition key |
| **Patch** | Apply partial field updates using Cosmos DB Patch API |

### Key Principles
- Schema-independent: works with any JSON document structure
- Configuration-driven: no hardcoded values
- Clean separation of UI, business logic, and data access
- Extensible and testable design

---

## 2. Solution Structure

```
CosmosDbManager/
├── CosmosDbManager.sln
├── src/
│   ├── CosmosDbManager.Domain/           # Entities, interfaces, enums, value objects
│   ├── CosmosDbManager.Application/      # Use cases, services, DTOs, validators
│   ├── CosmosDbManager.Infrastructure/   # Cosmos DB SDK implementation, repositories
│   └── CosmosDbManager.Web/              # ASP.NET Core MVC, Razor Views, Controllers
├── tests/
│   ├── CosmosDbManager.Application.Tests/
│   └── CosmosDbManager.Infrastructure.Tests/
├── .gitignore
├── README.md
└── appsettings.json (root reference)
```

### Project References
```
Web → Application → Domain
Infrastructure → Application → Domain
Web → Infrastructure (for DI registration only)
```

---

## 3. Layer Definitions

### Domain Layer — `CosmosDbManager.Domain`
- Pure C# class library (no framework dependencies)
- Contains: Entities, Interfaces, Enums, Custom Exceptions, Value Objects

### Application Layer — `CosmosDbManager.Application`
- References: Domain only
- Contains: Service interfaces, DTOs, Use case implementations, Validators, Mappings

### Infrastructure Layer — `CosmosDbManager.Infrastructure`
- References: Application + Domain
- Contains: Cosmos DB SDK integration, Repository implementations, Retry policies

### Web (Presentation) Layer — `CosmosDbManager.Web`
- References: Application + Infrastructure (for DI only)
- Contains: Controllers, Razor Views, ViewModels, Middleware, Filters, Program.cs

---

## 4. Domain Layer

### 4.1 Folder Structure
```
CosmosDbManager.Domain/
├── Entities/
│   └── CosmosDocument.cs
├── Interfaces/
│   ├── ICosmosRepository.cs
│   └── ICosmosConnectionFactory.cs
├── Enums/
│   └── PatchOperationType.cs
├── Exceptions/
│   ├── CosmosDbException.cs
│   ├── DocumentNotFoundException.cs
│   └── InvalidConfigurationException.cs
└── ValueObjects/
    ├── CosmosConfiguration.cs
    └── PatchOperation.cs
```

### 4.2 Key Models

#### `CosmosConfiguration.cs` (Value Object)
```csharp
public sealed class CosmosConfiguration
{
    public string ConnectionString { get; init; }
    public string DatabaseName { get; init; }
    public string ContainerName { get; init; }
    public string PartitionKey { get; init; }
}
```

#### `CosmosDocument.cs` (Entity)
```csharp
public sealed class CosmosDocument
{
    public string Id { get; init; }
    public string PartitionKeyValue { get; init; }
    public JsonDocument Payload { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
```

#### `PatchOperation.cs` (Value Object)
```csharp
public sealed class PatchOperation
{
    public PatchOperationType OperationType { get; init; }  // Add, Set, Replace, Remove, Increment
    public string Path { get; init; }                       // e.g., "/status"
    public object? Value { get; init; }
}
```

#### `PatchOperationType.cs` (Enum)
```csharp
public enum PatchOperationType
{
    Add,
    Set,
    Replace,
    Remove,
    Increment
}
```

### 4.3 Repository Interface

#### `ICosmosRepository.cs`
```csharp
public interface ICosmosRepository
{
    Task<CosmosDocument> GetAsync(string id, string partitionKeyValue, CancellationToken ct = default);
    Task<CosmosDocument> InsertAsync(CosmosDocument document, CancellationToken ct = default);
    Task<CosmosDocument> UpsertAsync(CosmosDocument document, CancellationToken ct = default);
    Task<CosmosDocument> PatchAsync(string id, string partitionKeyValue, IEnumerable<PatchOperation> operations, CancellationToken ct = default);
}
```

#### `ICosmosConnectionFactory.cs`
```csharp
public interface ICosmosConnectionFactory
{
    CosmosClient CreateClient(CosmosConfiguration config);
}
```

---

## 5. Application Layer

### 5.1 Folder Structure
```
CosmosDbManager.Application/
├── DTOs/
│   ├── Request/
│   │   ├── InsertDocumentRequest.cs
│   │   ├── UpsertDocumentRequest.cs
│   │   ├── GetDocumentRequest.cs
│   │   ├── PatchDocumentRequest.cs
│   │   └── CosmosConfigurationDto.cs
│   └── Response/
│       ├── DocumentResponse.cs
│       └── OperationResult.cs
├── Interfaces/
│   └── IDocumentService.cs
├── Services/
│   └── DocumentService.cs
├── Validators/
│   ├── InsertDocumentValidator.cs
│   ├── UpsertDocumentValidator.cs
│   ├── GetDocumentValidator.cs
│   ├── PatchDocumentValidator.cs
│   └── CosmosConfigurationValidator.cs
├── Mappings/
│   └── DocumentMapper.cs
└── Extensions/
    └── ApplicationServiceExtensions.cs
```

### 5.2 DTOs

#### `CosmosConfigurationDto.cs`
```csharp
public sealed class CosmosConfigurationDto
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string ContainerName { get; set; }
    public string PartitionKey { get; set; }
}
```

#### `InsertDocumentRequest.cs`
```csharp
public sealed class InsertDocumentRequest
{
    public CosmosConfigurationDto Configuration { get; set; }
    public string Id { get; set; }
    public string PartitionKeyValue { get; set; }
    public string JsonPayload { get; set; }   // Raw JSON string
}
```

#### `UpsertDocumentRequest.cs`
```csharp
public sealed class UpsertDocumentRequest
{
    public CosmosConfigurationDto Configuration { get; set; }
    public string Id { get; set; }
    public string PartitionKeyValue { get; set; }
    public string JsonPayload { get; set; }
}
```

#### `GetDocumentRequest.cs`
```csharp
public sealed class GetDocumentRequest
{
    public CosmosConfigurationDto Configuration { get; set; }
    public string Id { get; set; }
    public string PartitionKeyValue { get; set; }
}
```

#### `PatchDocumentRequest.cs`
```csharp
public sealed class PatchDocumentRequest
{
    public CosmosConfigurationDto Configuration { get; set; }
    public string Id { get; set; }
    public string PartitionKeyValue { get; set; }
    public List<PatchOperationDto> Operations { get; set; }
}

public sealed class PatchOperationDto
{
    public string OperationType { get; set; }   // "Add" | "Set" | "Replace" | "Remove" | "Increment"
    public string Path { get; set; }             // e.g., "/status"
    public object? Value { get; set; }
}
```

#### `DocumentResponse.cs`
```csharp
public sealed class DocumentResponse
{
    public string Id { get; set; }
    public string PartitionKeyValue { get; set; }
    public string JsonPayload { get; set; }   // Pretty-printed JSON
    public DateTimeOffset? Timestamp { get; set; }
}
```

#### `OperationResult<T>.cs`
```csharp
public sealed class OperationResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }

    public static OperationResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static OperationResult<T> Failure(string error, string? code = null) =>
        new() { IsSuccess = false, ErrorMessage = error, ErrorCode = code };
}
```

### 5.3 Service Interface

#### `IDocumentService.cs`
```csharp
public interface IDocumentService
{
    Task<OperationResult<DocumentResponse>> InsertAsync(InsertDocumentRequest request, CancellationToken ct = default);
    Task<OperationResult<DocumentResponse>> UpsertAsync(UpsertDocumentRequest request, CancellationToken ct = default);
    Task<OperationResult<DocumentResponse>> GetAsync(GetDocumentRequest request, CancellationToken ct = default);
    Task<OperationResult<DocumentResponse>> PatchAsync(PatchDocumentRequest request, CancellationToken ct = default);
}
```

### 5.4 Validators

Each validator checks:
- `Id` — required, non-empty string
- `PartitionKeyValue` — required, non-empty string
- `JsonPayload` — valid, parseable JSON
- `CosmosConfigurationDto` — all fields required, `ConnectionString` must be non-empty
- `PatchOperations` — at least one operation, valid `OperationType`, non-empty `Path`

> Use `FluentValidation` or built-in `DataAnnotations` — prefer FluentValidation for testability.

---

## 6. Infrastructure Layer

### 6.1 Folder Structure
```
CosmosDbManager.Infrastructure/
├── CosmosDb/
│   ├── CosmosConnectionFactory.cs
│   ├── CosmosRepository.cs
│   └── CosmosClientOptions.cs
├── Resilience/
│   └── RetryPolicyHelper.cs
└── Extensions/
    └── InfrastructureServiceExtensions.cs
```

### 6.2 `CosmosConnectionFactory.cs`
- Implements `ICosmosConnectionFactory`
- Creates `CosmosClient` from `CosmosConfiguration`
- Uses `CosmosClientOptions` with:
  - `SerializerOptions` for camelCase JSON
  - Connection mode: `Gateway` (default) or `Direct` (configurable)
  - Request timeout configurable via `appsettings.json`
- Cache `CosmosClient` instances keyed by connection string (avoid recreating clients)

### 6.3 `CosmosRepository.cs`
- Implements `ICosmosRepository`
- Accepts `CosmosConfiguration` per-request (dynamic container targeting)
- All methods use `async/await`
- Maps Cosmos SDK types to/from Domain entities
- Handles `CosmosException` and wraps into domain exceptions

#### Method Implementations

| Method | SDK Call | Notes |
|--------|----------|-------|
| `GetAsync` | `container.ReadItemAsync<dynamic>()` | Throw `DocumentNotFoundException` if 404 |
| `InsertAsync` | `container.CreateItemAsync<dynamic>()` | Throw on conflict (409) |
| `UpsertAsync` | `container.UpsertItemAsync<dynamic>()` | Creates or replaces |
| `PatchAsync` | `container.PatchItemAsync<dynamic>()` | Uses `PatchOperation` from Cosmos SDK |

### 6.4 Patch Operation Mapping

Map `PatchOperationDto.OperationType` to Cosmos SDK `PatchOperation`:
```
"Add"       → PatchOperation.Add(path, value)
"Set"       → PatchOperation.Set(path, value)
"Replace"   → PatchOperation.Replace(path, value)
"Remove"    → PatchOperation.Remove(path)
"Increment" → PatchOperation.Increment(path, value)
```

### 6.5 `RetryPolicyHelper.cs`
- Handle transient Cosmos DB errors (HTTP 429, 503, 408)
- Implement exponential backoff with jitter
- Max retry count configurable via `appsettings.json`
- Use `Polly` library or `CosmosClientOptions.MaxRetryAttemptsOnRateLimitedRequests`

---

## 7. Web (Presentation) Layer

### 7.1 Folder Structure
```
CosmosDbManager.Web/
├── Controllers/
│   ├── HomeController.cs
│   ├── DocumentController.cs
│   └── ConfigurationController.cs
├── ViewModels/
│   ├── ConfigurationViewModel.cs
│   ├── InsertDocumentViewModel.cs
│   ├── UpsertDocumentViewModel.cs
│   ├── GetDocumentViewModel.cs
│   ├── PatchDocumentViewModel.cs
│   └── OperationResultViewModel.cs
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _ConfigurationPanel.cshtml  (partial)
│   │   ├── _ValidationSummary.cshtml   (partial)
│   │   └── _OperationResult.cshtml     (partial)
│   ├── Home/
│   │   └── Index.cshtml
│   ├── Document/
│   │   ├── Insert.cshtml
│   │   ├── Upsert.cshtml
│   │   ├── Get.cshtml
│   │   └── Patch.cshtml
│   └── Configuration/
│       └── Index.cshtml
├── Middleware/
│   └── GlobalExceptionMiddleware.cs
├── Filters/
│   └── ValidateModelFilter.cs
├── wwwroot/
│   ├── css/
│   │   └── site.css
│   ├── js/
│   │   ├── site.js
│   │   └── json-formatter.js
│   └── lib/
│       └── bootstrap/
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

### 7.2 Controllers

#### `HomeController.cs`
- `GET /` → Dashboard/landing page with navigation to all features

#### `ConfigurationController.cs`
- `GET /configuration` → Display configuration form
- `POST /configuration/test` → Test Cosmos DB connection (optional but recommended)

#### `DocumentController.cs`
| Action | Route | Method | Description |
|--------|-------|--------|-------------|
| `Insert` | `/document/insert` | GET | Show Insert form |
| `Insert` | `/document/insert` | POST | Execute Insert |
| `Upsert` | `/document/upsert` | GET | Show Upsert form |
| `Upsert` | `/document/upsert` | POST | Execute Upsert |
| `Get` | `/document/get` | GET | Show Get form |
| `Get` | `/document/get` | POST | Execute Get |
| `Patch` | `/document/patch` | GET | Show Patch form |
| `Patch` | `/document/patch` | POST | Execute Patch |

### 7.3 ViewModels
- Mirror DTOs but include display-specific fields (e.g., `FormattedJson`, `OperationSuccess`, `ErrorMessage`)
- Include `[Required]`, `[Display]` annotations for Razor form binding
- Store Cosmos configuration in Session or hidden form fields (configurable)

### 7.4 Session Management
- Store `CosmosConfigurationDto` in HTTP Session (serialized as JSON)
- Pre-populate configuration panel across pages
- Session timeout configurable via `appsettings.json`

### 7.5 Program.cs — Service Registration
```csharp
// Application Layer
builder.Services.AddScoped<IDocumentService, DocumentService>();

// Infrastructure Layer
builder.Services.AddSingleton<ICosmosConnectionFactory, CosmosConnectionFactory>();
builder.Services.AddScoped<ICosmosRepository, CosmosRepository>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<InsertDocumentValidator>();

// Session
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
```

---

## 8. Configuration & Secrets Management

### 8.1 `appsettings.json` Structure
```json
{
  "CosmosDb": {
    "DefaultConnectionString": "",
    "DefaultDatabaseName": "",
    "DefaultContainerName": "",
    "DefaultPartitionKey": "",
    "MaxRetryAttempts": 3,
    "RequestTimeoutSeconds": 30,
    "ConnectionMode": "Gateway"
  },
  "Session": {
    "IdleTimeoutMinutes": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 8.2 Rules
- Never hardcode connection strings or secrets
- Use `appsettings.Development.json` for local dev overrides (gitignored)
- Support environment variable overrides: `CosmosDb__DefaultConnectionString`
- Use `IOptions<CosmosDbSettings>` strongly typed configuration binding
- Production: use Azure Key Vault, environment variables, or secrets manager

### 8.3 Strongly Typed Settings

#### `CosmosDbSettings.cs`
```csharp
public sealed class CosmosDbSettings
{
    public const string SectionName = "CosmosDb";
    public string DefaultConnectionString { get; set; } = string.Empty;
    public string DefaultDatabaseName { get; set; } = string.Empty;
    public string DefaultContainerName { get; set; } = string.Empty;
    public string DefaultPartitionKey { get; set; } = string.Empty;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RequestTimeoutSeconds { get; set; } = 30;
    public string ConnectionMode { get; set; } = "Gateway";
}
```

---

## 9. UI Design & Pages

### 9.1 Layout (`_Layout.cshtml`)
- Bootstrap 5 navbar with links: Home | Insert | Upsert | Get | Patch
- Sticky Cosmos DB Configuration panel (collapsible sidebar or top section)
- Footer with app name and version
- Responsive design (mobile-friendly)
- Toast/alert component for success and error messages

### 9.2 Configuration Panel (`_ConfigurationPanel.cshtml`)
Reusable partial rendered on every document page:
- `Connection String` — password input (masked)
- `Database Name` — text input
- `Container Name` — text input
- `Partition Key` — text input (e.g., `/tenantId`)
- `[Save to Session]` button — persists config across pages
- `[Test Connection]` button — validates connectivity (async AJAX call)

### 9.3 Insert Page (`Document/Insert.cshtml`)
- Configuration Panel (partial)
- `Document ID` — text input (required)
- `Partition Key Value` — text input (required)
- `JSON Payload` — `<textarea>` with syntax highlighting (CodeMirror or highlight.js)
- `[Insert Document]` submit button
- Result panel — displays returned document or error

### 9.4 Upsert Page (`Document/Upsert.cshtml`)
- Same as Insert page
- Label changes to `Upsert Document`
- Tooltip explaining upsert behavior

### 9.5 Get Page (`Document/Get.cshtml`)
- Configuration Panel (partial)
- `Document ID` — text input (required)
- `Partition Key Value` — text input (required)
- `[Get Document]` submit button
- Result panel — pretty-printed JSON viewer

### 9.6 Patch Page (`Document/Patch.cshtml`)
- Configuration Panel (partial)
- `Document ID` — text input (required)
- `Partition Key Value` — text input (required)
- Dynamic Patch Operations builder:
  - `[+ Add Operation]` button adds a row:
    - `Operation Type` — dropdown (Add / Set / Replace / Remove / Increment)
    - `Path` — text input (e.g., `/status`)
    - `Value` — text input (hidden for Remove)
  - `[– Remove]` button per row
- `[Apply Patch]` submit button
- Result panel — shows patched document or error

### 9.7 Result Panel (`_OperationResult.cshtml`)
Reusable partial for all pages:
- ✅ Success: green alert + JSON viewer with copy button
- ❌ Error: red alert + error code + user-friendly message
- Collapsible raw response section

---

## 10. Validation & Error Handling

### 10.1 Input Validation Rules
| Field | Rule |
|-------|------|
| `Id` | Required, non-empty, max 255 chars |
| `PartitionKeyValue` | Required, non-empty |
| `JsonPayload` | Required, must be valid JSON (parseable) |
| `ConnectionString` | Required, non-empty |
| `DatabaseName` | Required, non-empty, alphanumeric + hyphens |
| `ContainerName` | Required, non-empty, alphanumeric + hyphens |
| `PartitionKey` | Required, must start with `/` |
| `PatchPath` | Required, must start with `/` |
| `PatchOperationType` | Must be a valid enum value |

### 10.2 Error Handling Strategy

#### `GlobalExceptionMiddleware.cs`
- Catch all unhandled exceptions
- Log with structured logging (include correlation ID)
- Return user-friendly error page (not stack trace)
- Log full exception details server-side only

#### `ValidateModelFilter.cs`
- Action filter applied globally
- Return validation errors as model state errors
- Render validation summary on form pages

#### Domain Exception Hierarchy
```
CosmosDbException (base)
├── DocumentNotFoundException       → HTTP 404 → "Document not found."
├── DocumentConflictException       → HTTP 409 → "A document with this ID already exists."
├── InvalidConfigurationException   → HTTP 400 → "Invalid Cosmos DB configuration."
├── CosmosRateLimitException        → HTTP 429 → "Too many requests. Please try again."
└── CosmosServiceUnavailableException → HTTP 503 → "Cosmos DB service is unavailable."
```

### 10.3 JSON Validation
```csharp
public static bool IsValidJson(string json)
{
    try
    {
        JsonDocument.Parse(json);
        return true;
    }
    catch (JsonException)
    {
        return false;
    }
}
```

---

## 11. Patch Operations

### 11.1 Supported Operations
| Operation | Description | Value Required |
|-----------|-------------|----------------|
| `Add` | Adds a new field or array element | ✅ Yes |
| `Set` | Sets field value (creates if missing) | ✅ Yes |
| `Replace` | Replaces existing field value | ✅ Yes |
| `Remove` | Removes a field | ❌ No |
| `Increment` | Increments a numeric field | ✅ Yes (number) |

### 11.2 Cosmos SDK Mapping
```csharp
private static Microsoft.Azure.Cosmos.PatchOperation MapToCosmosPatch(PatchOperation op)
{
    return op.OperationType switch
    {
        PatchOperationType.Add       => Microsoft.Azure.Cosmos.PatchOperation.Add(op.Path, op.Value),
        PatchOperationType.Set       => Microsoft.Azure.Cosmos.PatchOperation.Set(op.Path, op.Value),
        PatchOperationType.Replace   => Microsoft.Azure.Cosmos.PatchOperation.Replace(op.Path, op.Value),
        PatchOperationType.Remove    => Microsoft.Azure.Cosmos.PatchOperation.Remove(op.Path),
        PatchOperationType.Increment => Microsoft.Azure.Cosmos.PatchOperation.Increment(op.Path, Convert.ToDouble(op.Value)),
        _ => throw new ArgumentOutOfRangeException(nameof(op.OperationType))
    };
}
```

### 11.3 UI Behavior
- Dynamically add/remove patch operation rows via JavaScript
- Validate that at least one operation exists before submit
- Hide `Value` field when `Remove` is selected
- Validate numeric `Value` when `Increment` is selected

---

## 12. Logging & Observability

### 12.1 Structured Logging
- Use `ILogger<T>` throughout all services and controllers
- Log format: structured JSON (configurable via `appsettings.json`)
- Log levels:
  - `Information` — operation start/complete, configuration loaded
  - `Warning` — document not found, retry triggered
  - `Error` — Cosmos SDK exception, validation failure
  - `Critical` — unhandled exception in middleware

### 12.2 Log Events
| Event | Level | Message |
|-------|-------|---------|
| Insert success | Info | `"Document {Id} inserted into {Container}"` |
| Document not found | Warning | `"Document {Id} not found in {Container}"` |
| Cosmos rate limit | Warning | `"Rate limited on {Container}. Retry {Attempt}/{Max}"` |
| Cosmos exception | Error | `"CosmosException on {Operation}: {StatusCode} - {Message}"` |
| Unhandled exception | Critical | `"Unhandled exception: {Message}"` |

### 12.3 Correlation ID
- Generate a unique `X-Correlation-Id` per request
- Include in all log entries
- Return in response headers for traceability

---

## 13. Swagger / OpenAPI

### 13.1 Configuration
- Enable Swagger UI at `/swagger` in Development only
- Use `Swashbuckle.AspNetCore`
- Add XML comments to controllers and DTOs (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`)

### 13.2 Document API Endpoints (REST)
Expose optional REST API endpoints alongside MVC for programmatic use:

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/documents` | POST | Insert document |
| `/api/documents/upsert` | PUT | Upsert document |
| `/api/documents/{id}` | GET | Get document |
| `/api/documents/{id}/patch` | PATCH | Patch document |

> **Note:** These API endpoints are secondary to the MVC UI. All require `CosmosConfigurationDto` in the request body or headers.

---

## 14. Retry & Resilience

### 14.1 Strategy
- Use Cosmos SDK's built-in retry: `MaxRetryAttemptsOnRateLimitedRequests` and `MaxRetryWaitTimeOnRateLimitedRequests`
- Supplement with Polly for additional transient errors (503, 408)
- Retry max attempts configurable via `appsettings.json`

### 14.2 Polly Policy (Infrastructure)
```csharp
Policy
    .Handle<CosmosException>(ex => ex.StatusCode is
        HttpStatusCode.ServiceUnavailable or
        HttpStatusCode.RequestTimeout or
        HttpStatusCode.TooManyRequests)
    .WaitAndRetryAsync(
        retryCount: settings.MaxRetryAttempts,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
            + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),  // jitter
        onRetry: (exception, timeSpan, attempt, context) =>
            logger.LogWarning("Retry {Attempt} after {Delay}ms: {Message}", attempt, timeSpan.TotalMilliseconds, exception.Message)
    );
```

---

## 15. Security Considerations

### 15.1 Input Security
- Sanitize all user inputs before rendering in Razor views (use `@Html.Encode` / Razor auto-encoding)
- Validate JSON payload server-side regardless of client-side validation
- Do not expose raw Cosmos DB error messages to end users

### 15.2 Connection String Safety
- Mask connection string in UI (password input type)
- Never log connection strings
- Do not store connection strings in cookies (use server-side session only)

### 15.3 HTTP Security Headers
Add via middleware:
```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
```

### 15.4 HTTPS
- Enforce HTTPS redirection in production
- Use HSTS in production environments

---

## 16. Testing Strategy

### 16.1 Unit Tests — `CosmosDbManager.Application.Tests`
- Test all validators (valid + invalid inputs)
- Test `DocumentService` with mocked `ICosmosRepository`
- Test `DocumentMapper` mapping logic
- Test `OperationResult` factory methods
- Test `PatchOperation` mapping logic

### 16.2 Integration Tests — `CosmosDbManager.Infrastructure.Tests`
- Test `CosmosRepository` against Cosmos DB Emulator
- Test connection factory client creation and caching
- Test retry policy behavior with simulated transient errors

### 16.3 Testing Tools
| Tool | Purpose |
|------|---------|
| `xUnit` | Test framework |
| `Moq` | Mocking interfaces |
| `FluentAssertions` | Readable assertions |
| `Microsoft.Azure.Cosmos.Emulator` | Local Cosmos DB emulator |

### 16.4 Design for Testability
- All services depend on interfaces (never concrete classes)
- No static dependencies
- Cosmos configuration passed per-request (no static state)
- `ICosmosConnectionFactory` allows mocking of `CosmosClient`

---

## 17. README Requirements

The `README.md` at the solution root must include:

### Sections
1. **Project Overview** — what the app does
2. **Prerequisites** — .NET 10 SDK, Azure Cosmos DB account or emulator
3. **Getting Started** — clone, restore, run instructions
4. **Configuration** — `appsettings.json` fields explained
5. **Environment Variables** — override table
6. **Features** — Insert, Upsert, Get, Patch with screenshots
7. **Patch Operations Guide** — supported ops, example payloads
8. **API Reference** — Swagger link and sample request/response
9. **Architecture Overview** — solution structure diagram
10. **Running Tests** — `dotnet test` command
11. **Troubleshooting** — common errors and fixes
12. **Contributing** — coding standards reference

---

## 18. Implementation Phases

### Phase 1 — Solution Scaffolding
- [ ] Create solution and 4 projects
- [ ] Set up project references
- [ ] Add NuGet packages
- [ ] Configure `appsettings.json`
- [ ] Set up `Program.cs` with DI

### Phase 2 — Domain Layer
- [ ] Define `CosmosConfiguration` value object
- [ ] Define `CosmosDocument` entity
- [ ] Define `PatchOperation` value object
- [ ] Define `PatchOperationType` enum
- [ ] Define `ICosmosRepository` interface
- [ ] Define `ICosmosConnectionFactory` interface
- [ ] Define custom exception hierarchy

### Phase 3 — Application Layer
- [ ] Implement all DTOs (Request + Response)
- [ ] Implement `OperationResult<T>`
- [ ] Implement `IDocumentService` interface
- [ ] Implement `DocumentService`
- [ ] Implement all validators
- [ ] Implement `DocumentMapper`
- [ ] Register services in `ApplicationServiceExtensions`

### Phase 4 — Infrastructure Layer
- [ ] Implement `CosmosConnectionFactory` with client caching
- [ ] Implement `CosmosRepository` (Get, Insert, Upsert, Patch)
- [ ] Implement patch operation mapping
- [ ] Implement retry policy with Polly
- [ ] Register infrastructure in `InfrastructureServiceExtensions`

### Phase 5 — Web Layer
- [ ] Create `_Layout.cshtml` with Bootstrap 5 navbar
- [ ] Create `_ConfigurationPanel.cshtml` partial
- [ ] Implement `ConfigurationController` with session save
- [ ] Implement `DocumentController` (all 4 actions)
- [ ] Create all ViewModels
- [ ] Create Insert, Upsert, Get, Patch Razor views
- [ ] Create `_OperationResult.cshtml` partial
- [ ] Add JSON textarea with syntax highlighting
- [ ] Add dynamic Patch Operation builder (JavaScript)
- [ ] Implement `GlobalExceptionMiddleware`
- [ ] Implement `ValidateModelFilter`
- [ ] Add security headers middleware
- [ ] Configure Swagger

### Phase 6 — API Endpoints (Optional REST Layer)
- [ ] Create `DocumentApiController.cs`
- [ ] Add XML documentation comments
- [ ] Configure Swagger with examples

### Phase 7 — Testing
- [ ] Unit tests for validators
- [ ] Unit tests for `DocumentService`
- [ ] Unit tests for `DocumentMapper`
- [ ] Integration tests for `CosmosRepository`

### Phase 8 — Documentation & Finalization
- [ ] Write `README.md`
- [ ] Add `.gitignore`
- [ ] Review and clean up code
- [ ] Final testing end-to-end

---

## 19. File & Folder Reference

### NuGet Packages

| Package | Project | Purpose |
|---------|---------|---------|
| `Microsoft.Azure.Cosmos` | Infrastructure | Cosmos DB SDK |
| `Polly` | Infrastructure | Retry/resilience |
| `FluentValidation.AspNetCore` | Application + Web | Input validation |
| `Swashbuckle.AspNetCore` | Web | Swagger/OpenAPI |
| `Microsoft.AspNetCore.Session` | Web | Session support |
| `xUnit` | Tests | Test framework |
| `Moq` | Tests | Mocking |
| `FluentAssertions` | Tests | Assertions |

### Key Files Summary
| File | Layer | Purpose |
|------|-------|---------|
| `CosmosConfiguration.cs` | Domain | Value object for DB config |
| `ICosmosRepository.cs` | Domain | Repository contract |
| `DocumentService.cs` | Application | Core business logic |
| `OperationResult.cs` | Application | Unified result wrapper |
| `CosmosRepository.cs` | Infrastructure | Cosmos SDK calls |
| `RetryPolicyHelper.cs` | Infrastructure | Polly retry setup |
| `DocumentController.cs` | Web | HTTP request handling |
| `_ConfigurationPanel.cshtml` | Web | Reusable config partial |
| `GlobalExceptionMiddleware.cs` | Web | Central error handling |
| `Program.cs` | Web | App startup + DI |

---

## 20. Sample Requests & Responses

### Insert Document

**Request (POST `/api/documents`)**
```json
{
  "configuration": {
    "connectionString": "AccountEndpoint=https://myaccount.documents.azure.com:443/;AccountKey=...",
    "databaseName": "MyDatabase",
    "containerName": "Users",
    "partitionKey": "/tenantId"
  },
  "id": "user-001",
  "partitionKeyValue": "tenant-abc",
  "jsonPayload": "{\"id\": \"user-001\", \"tenantId\": \"tenant-abc\", \"name\": \"John Doe\", \"email\": \"john@example.com\"}"
}
```

**Response (200 OK)**
```json
{
  "isSuccess": true,
  "data": {
    "id": "user-001",
    "partitionKeyValue": "tenant-abc",
    "jsonPayload": "{\n  \"id\": \"user-001\",\n  \"tenantId\": \"tenant-abc\",\n  \"name\": \"John Doe\",\n  \"email\": \"john@example.com\",\n  \"_ts\": 1720000000\n}",
    "timestamp": "2025-01-15T10:30:00Z"
  }
}
```

### Get Document

**Request (POST `/document/get`)**
```
id: user-001
partitionKeyValue: tenant-abc
+ Configuration Panel fields
```

**Error Response (404)**
```json
{
  "isSuccess": false,
  "errorMessage": "Document with ID 'user-001' was not found in container 'Users'.",
  "errorCode": "DOCUMENT_NOT_FOUND"
}
```

### Patch Document

**Request (POST `/api/documents/user-001/patch`)**
```json
{
  "configuration": {
    "connectionString": "...",
    "databaseName": "MyDatabase",
    "containerName": "Users",
    "partitionKey": "/tenantId"
  },
  "id": "user-001",
  "partitionKeyValue": "tenant-abc",
  "operations": [
    { "operationType": "Set", "path": "/status", "value": "active" },
    { "operationType": "Add", "path": "/lastLoginAt", "value": "2025-01-15T10:30:00Z" },
    { "operationType": "Increment", "path": "/loginCount", "value": 1 },
    { "operationType": "Remove", "path": "/temporaryToken" }
  ]
}
```

**Response (200 OK)**
```json
{
  "isSuccess": true,
  "data": {
    "id": "user-001",
    "partitionKeyValue": "tenant-abc",
    "jsonPayload": "{\n  \"id\": \"user-001\",\n  \"tenantId\": \"tenant-abc\",\n  \"name\": \"John Doe\",\n  \"status\": \"active\",\n  \"lastLoginAt\": \"2025-01-15T10:30:00Z\",\n  \"loginCount\": 1\n}",
    "timestamp": "2025-01-15T10:30:05Z"
  }
}
```

### Validation Error Response
```json
{
  "isSuccess": false,
  "errorMessage": "Validation failed: 'Id' is required. 'JsonPayload' is not valid JSON.",
  "errorCode": "VALIDATION_ERROR"
}
```

---

*This plan serves as the single source of truth for implementing the Generic Azure Cosmos DB Document Management Application. All implementation decisions should align with the principles, structure, and conventions defined in this document.*
