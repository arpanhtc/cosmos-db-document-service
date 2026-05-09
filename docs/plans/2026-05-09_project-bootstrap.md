# Plan: Cosmos DB Document Management Application Implementation

## TL;DR

Build a layered ASP.NET Core 10 application for managing JSON documents in Azure Cosmos DB. The project is currently a skeleton template with zero Cosmos DB integration. Implementation will follow Clean Architecture principles: create foundational layers (Interfaces, DTOs, Services, Repositories) → integrate Azure Cosmos DB SDK → build API endpoints and UI → add validation and error handling. Estimated complexity: 8 architectural phases with 40+ discrete steps. Total scope: ~2-3 weeks for a single developer working full-time.

---

## Steps

### **Phase 1: Project Setup & Foundation** 
*Sets up dependencies and creates the architectural skeleton.*

1. Add NuGet packages:
   - `Microsoft.Azure.Cosmos` (SDK for Cosmos DB)
   - `Microsoft.Extensions.Configuration` extensions (for configuration management)
   - Optional: `Serilog` (structured logging, though built-in logging can work initially)

2. Create folder structure for layers:
   - `Interfaces/` — service and repository contracts
   - `Services/` — business logic (CosmosDbService, DocumentService, ConfigurationService, etc.)
   - `Repositories/` — data access layer with Cosmos DB operations
   - `DTOs/` — data transfer objects (request/response models)
   - `Models/` — domain models (already exists, will expand)
   - `Constants/` — enums, constants for Cosmos DB operations
   - `Utilities/` or `Helpers/` — validation helpers, JSON utilities

3. Update `appsettings.json` and `appsettings.Development.json`:
   - Add `CosmosDb` section with: `ConnectionString`, `DatabaseName`, `ContainerName`, `PartitionKeyPath`
   - Add `Logging` settings for structured logging
   - Add feature flags for UI options

4. Update `Program.cs` dependency injection:
   - Register `IConfigurationService`, `ICosmosDbService`, `IDocumentService`, repositories
   - Add logging configuration
   - Add CORS if needed for API

**Dependencies:** None (first phase)  
**Verification:**
- [ ] Project compiles without errors
- [ ] New folders created and organized
- [ ] appsettings includes Cosmos DB configuration sections
- [ ] Program.cs registers all services without errors

---

### **Phase 2: Core Models & DTOs**
*Defines data structures for documents, operations, and API communication.*

1. Create domain models:
   - `Document.cs` — represents a Cosmos DB document (id, partition key, content/payload)
   - `PartitionKeySchema.cs` — metadata about partition key (name, type, default path '/pk')

2. Create DTOs for API requests/responses:
   - `CreateDocumentRequest.cs` — inputs for inserting document (id, **optional** partitionKeyValue, jsonPayload)
   - `UpsertDocumentRequest.cs` — inputs for upserting document (id, **optional** partitionKeyValue, jsonPayload)
   - `PatchDocumentRequest.cs` — patch operation details (id, **optional** partitionKeyValue, path, value, operation type)
   - `GetDocumentRequest.cs` — query parameters for retrieval (id, **optional** partitionKeyValue)
   - `DocumentResponse.cs` — standard API response wrapper (success, data, errors, diagnostics)
   - `OperationResult.cs` — result of any Cosmos DB operation (success, error message, affected data, **cosmosDbDiagnostics**)
   - `CosmosDbDiagnostics.cs` — **NEW** - includes RequestCharge (RU), ActivityId, StatusCode, RetryCount, RetryAfter
   - `CosmosDbConnectionInfo.cs` — configuration DTO for connection details

3. Create enums for operations:
   - `PatchOperationType.cs` — Add, Replace, Remove, Set, Increment, etc. (Cosmos DB Patch operations)
   - `DocumentOperationType.cs` — Insert, Upsert, Patch, Get, Delete

4. Create validation models:
   - `ValidationResult.cs` — contains validation errors and status
   - Validation attributes or helper classes for id, partition key, JSON payload

**Dependencies:** Depends on Phase 1 (folder structure)  
**Verification:**
- [ ] All models compile and use nullable reference types
- [ ] DTOs follow naming conventions (Request/Response suffix)
- [ ] Enums cover all Cosmos DB patch operation types
- [ ] Models are serializable to/from JSON

---

### **Phase 3: Interfaces & Contracts**
*Defines service and repository contracts for dependency injection.*

1. Create service interfaces in `Interfaces/`:
   - `ICosmosDbService.cs` — connection, database/container initialization, connection validation
   - `IDocumentService.cs` — business logic for document operations (InsertAsync, UpsertAsync, PatchAsync, GetAsync, DeleteAsync)
   - `IConfigurationService.cs` — manages runtime configuration changes (connection string, database name, container name, partition key path)
   - `IPatchOperationService.cs` — constructs and validates patch operations
   - `IPartitionKeyExtractor.cs` — **NEW** - extracts partition key from JSON payload using configured path (e.g., '/pk')

2. Create repository interfaces:
   - `IDocumentRepository.cs` — low-level Cosmos DB document operations

3. Define service method signatures with proper async patterns:
   - All I/O operations use `async Task<T>` or `async Task`
   - Include cancellation token support where applicable
   - `OperationResult<T>` includes Cosmos DB diagnostics for error cases

**Dependencies:** Depends on Phase 2 (DTOs and models)  
**Verification:**
- [ ] All interfaces use clear naming conventions
- [ ] Method signatures are consistent (async, return types, parameters)
- [ ] Interfaces support testability (no tight coupling)
- [ ] Documentation comments on key methods

---

### **Phase 4: Configuration Service**
*Manages runtime Cosmos DB configuration without code changes.*

1. Implement `ConfigurationService`:
   - Read Cosmos DB settings from `appsettings.json` (IConfiguration)
   - Support runtime updates to connection string, database, container, partition key path
   - Default partition key path: `/pk` (configurable in appsettings)
   - Validate configuration on initialization
   - Expose properties: `ConnectionString`, `DatabaseName`, `ContainerName`, `PartitionKeyPath`

2. Implement `IConfigurationService` interface with:
   - `GetConfigurationAsync()` — retrieves current settings
   - `UpdateConfigurationAsync(connectionInfo)` — updates settings at runtime
   - `ValidateConfigurationAsync()` — checks if connection is valid
   - `GetPartitionKeyPath()` — returns configured partition key path (default '/pk')

3. Add configuration validation:
   - Connection string format validation
   - Ensure database name and container name are non-empty
   - Validate partition key path format (should be valid JSON path like '/pk' or '/customer/id')

**Dependencies:** Depends on Phase 3 (interfaces)  
**Verification:**
- [ ] Service correctly reads from appsettings
- [ ] Runtime updates work without restart
- [ ] Configuration errors are clear and actionable
- [ ] Can be injected into controllers/services

---

### **Phase 5: Cosmos DB Service & Repository**
*Implements low-level Cosmos DB SDK integration.*

1. Implement `CosmosDbService`:
   - Initialize `CosmosClient` from connection string
   - Lazy-load database and container references
   - Health check: test connection to Cosmos DB
   - Dispose pattern for `CosmosClient`
   - Handle transient failures with appropriate error messages

2. Implement `DocumentRepository`:
   - `InsertDocumentAsync(document, partitionKey)` — create new document
   - `UpsertDocumentAsync(document, partitionKey)` — insert or replace
   - `GetDocumentAsync(id, partitionKey)` — retrieve single document
   - `QueryDocumentsAsync(query, parameters)` — flexible querying
   - `PatchDocumentAsync(id, partitionKey, patchOperations)` — apply patch operations
   - `DeleteDocumentAsync(id, partitionKey)` — delete document
   - All methods are async

3. **Cosmos DB Diagnostics Capture:**
   - Extract from `ResponseMessage` and `ItemResponse<>`:
     - `RequestCharge` (RU consumption)
     - `ActivityId` (for support troubleshooting)
     - `StatusCode` (HTTP status)
     - `Diagnostics` (retry info, latency, etc.)
   - Map to `CosmosDbDiagnostics` DTO in `OperationResult`
   - Include in all error responses returned to UI

4. Error handling strategy:
   - Map Cosmos DB exceptions (404, 429, 503, etc.) to meaningful error messages
   - Include Cosmos DB diagnostics in error results (StatusCode, RequestCharge, ActivityId, RetryAfter)
   - Preserve full error context in logs without exposing internal details to UI

**Dependencies:** Depends on Phase 4 (ConfigurationService)  
**Verification:**
- [ ] CosmosClient initialized correctly with connection string
- [ ] Can connect to valid Cosmos DB instance (or fails gracefully)
- [ ] All CRUD operations map to Cosmos DB SDK calls
- [ ] Error handling preserves context for debugging
- [ ] Async/await patterns followed throughout

---

### **Phase 6: Document Service (Business Logic)**
*Implements high-level document operations with validation.*

1. Implement `DocumentService`:
   - `InsertDocumentAsync(request)` — validates JSON, id, partition key → calls repository
   - `UpsertDocumentAsync(request)` — same validation as insert
   - `GetDocumentAsync(request)` — retrieves and formats response
   - `PatchDocumentAsync(id, partitionKey, patches)` — validates operations → applies patches
   - `DeleteDocumentAsync(id, partitionKey)` — deletes document with validation
   - All methods return `OperationResult<T>` with Cosmos DB diagnostics on success or failure

2. **Implement partition key extraction:**
   - Create `PartitionKeyExtractor` service (implements `IPartitionKeyExtractor`)
   - If user provides partition key value in request → use it directly
   - If NOT provided → extract from JSON payload using configured partition key path (default '/pk')
   - Parse JSON path using JSON pointer RFC 6901 or similar approach
   - Validation: if partition key not found AND not provided by user → clear error message
   - Support nested paths: '/customer/id', '/data/0/pk', etc.

3. Implement patch operation service:
   - `IPatchOperationService` — builds and validates patch operations from DTOs
   - Support all Cosmos DB patch operation types: Add, Replace, Remove, Set, Increment, etc.
   - Validate patch paths are valid JSON paths
   - Validate values match expected types

4. Implement validation logic:
   - JSON payload validation (is valid JSON?)
   - Partition key presence and type check (after extraction or user input)
   - Document ID validation (non-empty, reasonable length)
   - Duplicate detection (optional: check if document exists before insert)

5. **Cosmos DB Diagnostics in Results:**
   - All `OperationResult` responses include `CosmosDbDiagnostics` with:
     - RequestCharge (RU consumed)
     - ActivityId (for support reference)
     - StatusCode (HTTP status from Cosmos DB)
     - RetryCount and RetryAfter (for throttling scenarios)
   - On success: diagnostics shown in UI for informational purposes
   - On failure: diagnostics help with troubleshooting

**Dependencies:** Depends on Phase 5 (Repository)  
**Verification:**
- [ ] All operations include input validation
- [ ] Invalid JSON payloads are rejected with clear errors
- [ ] Partition key and ID validation works
- [ ] Patch operations validated before execution
- [ ] Error messages are user-friendly (no stack traces)

---

### **Phase 7: API Controllers & UI Flow**
*Creates REST-like endpoints and connects UI to services.*

1. Create API controllers (or extend HomeController):
   - `DocumentsController.cs`:
     - `POST /documents/insert` — create new document (id, **optional** partitionKeyValue, jsonPayload)
     - `POST /documents/upsert` — insert or replace (id, **optional** partitionKeyValue, jsonPayload)
     - `GET /documents/{id}` — retrieve document (id, **optional** partitionKeyValue)
     - `POST /documents/{id}/patch` — apply patch operations (id, **optional** partitionKeyValue, operations)
     - `DELETE /documents/{id}` — delete document (id, **optional** partitionKeyValue)
     - `POST /documents/configure` — update Cosmos DB settings
   - All endpoints return JSON with standardized response format
   - Responses include Cosmos DB diagnostics (RequestCharge, ActivityId, StatusCode, etc.)

2. Update `HomeController`:
   - `Index()` — show configuration UI
   - `ConfigureCosmos()` — test connection with provided settings

3. Create views for UI:
   - `Index.cshtml` — configuration panel (connection string, database, container, partition key path)
   - `Insert.cshtml` — form to insert JSON document (id, **optional** partition key field, JSON payload)
     - Help text: "If partition key not provided, will extract from JSON using path '/pk'"
   - `Upsert.cshtml` — form to upsert document (same as Insert)
   - `Get.cshtml` — search/retrieve form with results display (id, **optional** partition key)
   - `Patch.cshtml` — patch operation builder UI (id, **optional** partition key, operation builder)
   - Results display component — shows operation success/errors **with Cosmos DB diagnostics**
     - Display RequestCharge (RU), ActivityId (for support), StatusCode
     - Collapsible "Details" section for diagnostic info
   - Include client-side validation (HTML5 + JavaScript)
   - Form field labels clarify partition key is optional

4. Error handling in controllers:
   - Catch domain exceptions and return `BadRequest()` or `InternalServerError()`
   - Include user-friendly error message + Cosmos DB diagnostics in response
   - Log all errors server-side with full context

**Dependencies:** Depends on Phase 6 (DocumentService, PatchOperationService)  
**Verification:**
- [ ] All endpoints are callable (test with Postman or browser)
- [ ] Endpoints return correct HTTP status codes (200, 400, 404, 500)
- [ ] Error responses are consistent and informative
- [ ] UI forms render without errors
- [ ] Form submissions work end-to-end

---

### **Phase 8: UI Polish & Refinement**
*Finishes UI, improves UX, and adds user-facing features.*

1. Enhance views with Bootstrap styling:
   - Configuration panel: styled input fields, test connection button
   - Document operations: tabbed or multi-step forms
   - Results: formatted JSON display, copy-to-clipboard buttons
   - Error messages: styled alert boxes with context
   - **Cosmos DB Diagnostics Display:** Collapsible section showing RequestCharge (RU), ActivityId, StatusCode, RetryInfo

2. Add JavaScript enhancements:
   - Real-time JSON validation (show errors before submit)
   - Syntax highlighting for JSON display (e.g., `highlight.js` or similar)
   - Dynamic form fields for patch operations (add/remove operation rows)
   - Loading spinners during async operations
   - Copy-to-clipboard for ActivityId (for support reference)

3. Implement session state:
   - Remember last Cosmos DB configuration in session
   - Pre-populate forms with common defaults
   - Remember last used partition key path

4. Add README and documentation:
   - Setup instructions (how to get Cosmos DB connection string)
   - Usage guide (how to use each feature)
   - Configuration reference (appsettings format)
   - Partition key extraction explanation (default '/pk', can be customized)
   - Understanding Cosmos DB diagnostics (RequestCharge, ActivityId, etc.)
   - Troubleshooting section

**Dependencies:** Depends on Phase 7 (API Controllers, Views)  
**Verification:**
- [ ] All views render without errors
- [ ] Forms are responsive (mobile-friendly)
- [ ] Error messages display clearly with diagnostic info
- [ ] Operations complete and show results with diagnostics
- [ ] README is complete and clear
- [ ] Partition key extraction works (both explicit input and payload extraction)

---

## Relevant Files

### **To Create/Modify:**

**Configuration:**
- `appsettings.json` — add CosmosDb section with ConnectionString, DatabaseName, ContainerName, **PartitionKeyPath** (default: '/pk')

**Program.cs:**
- Register all services (ConfigurationService, CosmosDbService, DocumentService, PatchOperationService, PartitionKeyExtractor, repositories)
- Configure logging
- Configure DI container

**New Folders & Files:**
- `Interfaces/` — ICosmosDbService.cs, IDocumentService.cs, IConfigurationService.cs, IPatchOperationService.cs, **IPartitionKeyExtractor.cs**, IDocumentRepository.cs
- `Services/` — ConfigurationService.cs, CosmosDbService.cs, DocumentService.cs, PatchOperationService.cs, **PartitionKeyExtractor.cs**
- `Repositories/` — DocumentRepository.cs
- `DTOs/` — CreateDocumentRequest.cs, UpsertDocumentRequest.cs, PatchDocumentRequest.cs, GetDocumentRequest.cs, DocumentResponse.cs, OperationResult.cs, **CosmosDbDiagnostics.cs**
- `Models/` — Document.cs, PatchOperation.cs
- `Constants/` — PatchOperationType.cs, DocumentOperationType.cs
- `Controllers/` — DocumentsController.cs (or extend HomeController)
- `Views/` — Configuration.cshtml, Insert.cshtml, Upsert.cshtml, Get.cshtml, Patch.cshtml, Results.cshtml (with diagnostic display)
- `README.md` — setup and usage instructions

**Existing Files to Modify:**
- Cosmos.Db.Document.Service.csproj — add NuGet packages
- Program.cs — DI registration, logging config
- HomeController.cs — add configuration actions
- _Layout.cshtml — update navbar with links to operations

---

## Verification Strategy

### **Unit Testing (Post-Implementation)**
- [ ] ConfigurationService: test reading/updating settings
- [ ] DocumentService: test validation logic, error handling
- [ ] PatchOperationService: test operation building and validation
- [ ] Repository: mock CosmosClient and test CRUD operations

### **Integration Testing (Pre-Release)**
- [ ] Connect to real Cosmos DB test instance
- [ ] Test full end-to-end: configure → insert → get → patch → delete
- [ ] Test error scenarios: invalid connection, malformed JSON, missing partition key, duplicate IDs
- [ ] Test patch operations with various types (Add, Replace, Remove, Increment)

### **Manual Testing Checklist**
- [ ] Configuration: enter connection string, click "Test Connection", verify success/failure
- [ ] Insert: fill form, click Insert, verify document appears in Cosmos DB
- [ ] Get: enter document ID, verify retrieval and display
- [ ] Patch: select field, apply operation, verify only that field changed
- [ ] Delete: verify document removed from Cosmos DB
- [ ] Error scenarios: malformed JSON, invalid partition key, non-existent document

### **Code Review Points**
- [ ] All layers follow separation of concerns (UI ≠ Business Logic ≠ Data Access)
- [ ] Async/await used consistently throughout
- [ ] DTOs don't expose internal entities
- [ ] Exceptions are caught and logged, not leaked to UI
- [ ] Configuration is not hardcoded
- [ ] Dependency injection is used throughout (testable design)

---

## Decisions & Scope

### **Included**
- Full CRUD operations (Insert, Get/Retrieve, Upsert, Patch, Delete)
- Dynamic JSON document support (schema-independent)
- Runtime configuration (change connection without code)
- Cosmos DB Patch operations (all types supported by SDK)
- User-friendly error handling and validation
- Bootstrap-based responsive UI
- Environment-based configuration (appsettings.Development vs Production)

### **Excluded (Intentionally)**
- Authentication/Authorization (can be added later; framework already prepared)
- Bulk operations (batch import of multiple documents) — can be phased in
- SQL query builder UI — can be phased in as advanced feature
- Document versioning/history — out of scope
- Backup/restore functionality — out of scope
- Monitoring/analytics dashboard — out of scope
- User account management — out of scope
- Audit logging — can be added to Services layer later

### **Assumptions**
- Single Cosmos DB account; multi-tenancy not required for MVP
- Documents fit in memory (no streaming for very large docs)
- Partition key is always present in documents (user provides it or extracted)
- No concurrency conflicts (last-write-wins for updates)
- Internet connectivity to Cosmos DB available during operation

---

## Approved Decisions

### **Decision 1: UI Technology ✅ APPROVED**
**Choice:** ASP.NET Core MVC with Razor views + Bootstrap + vanilla JavaScript

Matches current template, simpler for admin tool use case.

---

### **Decision 2: Error Handling & Cosmos DB Diagnostics ✅ APPROVED**
**Choice:** Include Cosmos DB diagnostic information in error responses

**Implementation Details:**
- Error responses will include:
  - User-friendly message (what went wrong, how to fix it)
  - Cosmos DB HTTP status code (e.g., 404, 429, 503)
  - Request charge (RU consumption)
  - Activity ID (for support troubleshooting)
  - Retry count (if applicable)
  - Suggested retry delay (for 429 throttling)
- Technical details logged server-side with full stack trace
- UI displays diagnostic info in a collapsible "Details" section (for power users)

---

### **Decision 3: Partition Key Flexibility ✅ APPROVED**
**Choice:** Flexible partition key with fallback to payload extraction

**Implementation Details:**
- Default partition key path: `/pk`
- User provides partition key value in form (optional input field)
- If NOT provided by user: application attempts to extract from JSON payload using configured partition key path
- Configuration: partition key path in `appsettings.json` under `CosmosDb.PartitionKeyPath`
- Validation:
  - If partition key path doesn't exist in payload and no user input: validation error "Partition key not found and not provided"
  - If partition key extraction fails: clear error message with the expected path
  - User can override the extracted value via form input
