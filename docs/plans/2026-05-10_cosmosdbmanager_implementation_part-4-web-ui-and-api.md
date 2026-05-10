# Part 4 — Web Layer and API

## Objective
Implement MVC UI, API endpoints, middleware, session management, and Swagger.

## Scope
- Program.cs
- Controllers
- Razor Views
- ViewModels
- Middleware
- Swagger
- REST API
- JavaScript behaviors

## Implementation Steps

### Configure Program.cs
Add:
- MVC
- Session
- Swagger
- FluentValidation
- HTTPS redirection

### Implement Session Helpers
Create:
- SessionExtensions

Methods:
- SetObject
- GetObject

### Create ViewModels
- InsertDocumentViewModel
- UpsertDocumentViewModel
- GetDocumentViewModel
- PatchDocumentViewModel
- ConfigurationViewModel
- OperationResultViewModel

### Implement Controllers
- HomeController
- ConfigurationController
- DocumentController
- DocumentApiController

### Create Shared Views
- _Layout.cshtml
- _ConfigurationPanel.cshtml
- _ValidationSummary.cshtml
- _OperationResult.cshtml

### Create Document Views
- Insert.cshtml
- Upsert.cshtml
- Get.cshtml
- Patch.cshtml

### Implement Middleware
- GlobalExceptionMiddleware
- Security headers middleware
- ValidateModelFilter

### Configure Swagger
Enable in Development only.

### Add JavaScript
- json-formatter.js
- dynamic patch operation builder

## Validation Rules
- Controllers remain thin
- No direct repository access
- No SDK usage in UI layer

## Testing
- Controller tests
- Middleware tests
- API endpoint tests
- Session persistence tests

## Definition of Done
- MVC UI works
- Swagger loads
- API endpoints functional
- Session persistence works
- Middleware handles exceptions correctly
