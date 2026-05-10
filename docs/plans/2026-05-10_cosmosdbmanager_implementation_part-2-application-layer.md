# Part 2 — Application Layer

## Objective
Implement DTOs, validators, mappers, and orchestration services.

## Scope
- DTOs
- Validators
- DocumentService
- Mapping logic
- DI registration
- Unit tests

## Implementation Steps

### Create DTOs
Request DTOs:
- InsertDocumentRequest
- UpsertDocumentRequest
- GetDocumentRequest
- PatchDocumentRequest
- CosmosConfigurationDto

Response DTOs:
- DocumentResponse
- OperationResult<T>

### Implement Validators
Use FluentValidation.

Validate:
- Required fields
- JSON validity
- Patch operation paths
- Numeric increment values

### Implement DocumentMapper
Methods:
- ToCosmosConfiguration
- ToDomainDocument
- ToPatchOperations
- ToDocumentResponse

### Implement IDocumentService
Methods:
- InsertAsync
- UpsertAsync
- GetAsync
- PatchAsync

### Implement DocumentService
Responsibilities:
- Validation orchestration
- DTO mapping
- Repository invocation
- Error mapping
- Logging

### Add DI Registration
Create:
- ApplicationServiceExtensions

## Validation Rules
- No Cosmos SDK references
- No controller logic
- Preserve raw JSON structure

## Testing
- Validator tests
- Mapper tests
- DocumentService tests using Moq

## Definition of Done
- Validators pass
- Service compiles
- Mapping logic validated
- DI registration works
