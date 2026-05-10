# Part 3 — Infrastructure Layer

## Objective
Implement Cosmos DB SDK integration and resilience handling.

## Scope
- CosmosConnectionFactory
- CosmosRepository
- Retry policies
- Cosmos settings
- Infrastructure DI
- Integration tests

## Implementation Steps

### Create Infrastructure Folders
- CosmosDb
- Resilience
- Extensions
- Configuration

### Implement CosmosDbSettings
Properties:
- DefaultConnectionString
- MaxRetryAttempts
- RequestTimeoutSeconds
- ConnectionMode

### Implement CosmosConnectionFactory
Requirements:
- Cache CosmosClient instances
- Use ConcurrentDictionary
- Configure retry options
- Configure serializer options

### Implement RetryPolicyHelper
Use Polly.

Handle:
- HTTP 429
- HTTP 408
- HTTP 503

### Implement CosmosRepository
Methods:
- GetAsync
- InsertAsync
- UpsertAsync
- PatchAsync

### Implement Patch Mapping
Map domain PatchOperation to Cosmos SDK PatchOperation.

### Implement Exception Translation
Map Cosmos exceptions into domain exceptions.

### Add Infrastructure DI
Register:
- ICosmosConnectionFactory
- ICosmosRepository

## Validation Rules
- Never expose Cosmos SDK types externally
- Never recreate CosmosClient per request

## Testing
Use Cosmos Emulator.

Test:
- CRUD operations
- Retry behavior
- Client reuse
- Exception handling

## Definition of Done
- Emulator tests pass
- Retry handling works
- Repository operations succeed
