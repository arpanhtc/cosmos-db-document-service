# Shared Context — Cosmos DB Manager

## Objective
Provide globally applicable implementation guidance, architecture rules, conventions, dependencies, and standards shared across all implementation parts.

## Architecture
- Clean Architecture
- SOLID principles
- Dependency Injection
- Async/Await everywhere

## Project References
Web → Application → Domain
Infrastructure → Application → Domain
Web → Infrastructure (DI only)

## Shared Rules
- No infrastructure dependencies in Domain
- No business logic in Controllers
- All IO async with CancellationToken
- Use structured logging
- Use System.Text.Json
- Never log secrets or connection strings

## Shared Validation
- Id required, max 255 chars
- PartitionKey required
- JSON payload must be valid JSON
- PartitionKey path must start with '/'

## Shared Packages
- Microsoft.Azure.Cosmos
- FluentValidation.AspNetCore
- Polly
- Swashbuckle.AspNetCore
- xUnit
- Moq
- FluentAssertions

## Shared Testing Rules
- Unit tests use mocks only
- Integration tests use Cosmos Emulator
- All services interface-driven

## AI Agent Guidance
Implementation order:
1. Foundation
2. Domain
3. Application
4. Infrastructure
5. Web
6. API
7. Tests
8. Documentation
