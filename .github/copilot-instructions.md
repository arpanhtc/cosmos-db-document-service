---
name: Cosmos DB Document Management Service
description: Project guidelines for Cosmos DB Document Management Service ASP.NET Core application
---

# Development Guidelines

## Architecture & Engineering Standards:

- Use Clean Architecture principles.
- Follow SOLID principles.
- Use Dependency Injection throughout the application.
- Keep UI, business logic, and data access layers separated.
- Prefer reusable and generic implementations over hardcoded logic.
- Use async/await for all I/O and database operations.
- Write modular, maintainable, and production-ready code.
- Use meaningful naming conventions for classes, methods, variables, and folders.
- Avoid duplicate code and follow DRY principles.
- Add proper exception handling and structured logging.
- Validate all inputs and return user-friendly error messages.
- Keep configuration externalized using appsettings.json and environment variables.
- Do not hardcode secrets, connection strings, or environment-specific values.
- Prefer configuration-driven and extensible design.
- Follow RESTful conventions for APIs when applicable.
- Use DTOs/models instead of exposing internal entities directly.
- Add comments only where necessary; avoid unnecessary verbose comments.
- Write clean and readable code with proper folder structure.
- Use interfaces for services and repositories.
- Ensure scalability and performance considerations are included.
- Add retry handling for transient external/service failures.
- Prefer built-in .NET features and standard libraries before adding third-party packages.
- Include Swagger/OpenAPI support for APIs when applicable.
- Generate sample requests/responses for APIs or services.
- Add README documentation with setup and usage instructions.
- Write code in a way that supports future extensibility and unit testing.
- Prefer strongly typed models/configurations wherever possible.
- Follow secure coding practices and validate all external inputs.

## Goal:
The application should allow users to connect to any Azure Cosmos DB instance and perform generic document operations through a user-friendly interface without modifying code.