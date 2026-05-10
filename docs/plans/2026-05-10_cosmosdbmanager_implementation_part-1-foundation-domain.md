# Part 1 — Foundation and Domain Layer

## Objective
Create solution structure, project setup, and Domain layer contracts/models.

## Scope
- Solution scaffolding
- Project references
- NuGet packages
- Domain entities
- Repository interfaces
- Exception hierarchy

## Implementation Steps

### Create Projects
- CosmosDbManager.Domain
- CosmosDbManager.Application
- CosmosDbManager.Infrastructure
- CosmosDbManager.Web
- Application.Tests
- Infrastructure.Tests

### Configure References
Application → Domain
Infrastructure → Application + Domain
Web → Application + Infrastructure

### Create Domain Folders
- Entities
- Interfaces
- Exceptions
- Enums
- ValueObjects

### Implement Models
- CosmosConfiguration
- CosmosDocument
- PatchOperation
- PatchOperationType

### Implement Interfaces
- ICosmosRepository
- ICosmosConnectionFactory

### Implement Exceptions
- CosmosDbException
- DocumentNotFoundException
- DocumentConflictException
- InvalidConfigurationException
- CosmosRateLimitException
- CosmosServiceUnavailableException

## Validation Rules
- Domain layer must not reference ASP.NET or Cosmos SDK
- All models immutable where applicable

## Testing
- Exception constructor tests
- Entity initialization tests
- Enum parsing tests

## Definition of Done
- Solution builds
- Domain contracts compile
- Tests compile successfully
