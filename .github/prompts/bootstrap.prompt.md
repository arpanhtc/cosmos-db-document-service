---
name: bootstrap
description: Planning document for the launch of a new project for a generic Azure Cosmos DB document management application with a web-based UI using .NET 10 and Bootstrap.
agent: Plan
---

<!-- Tip: Use /create-prompt in chat to generate content with agent assistance -->

Project: Generic Azure Cosmos DB Document Management Application

Objective:
Build a reusable and configurable .NET 10 application for managing JSON documents in Azure Cosmos DB using a web-based UI.

Core Features:
- Insert JSON documents
- Upsert JSON documents
- View/Get Existing JSON documents
- Patch/update specific fields of existing documents
- Work with dynamic/schema-independent JSON payloads

Configuration:
Allow users to configure:
- Cosmos DB Connection String
- Database Name
- Container Name
- Partition Key

Technology Stack:
- .NET 10
- ASP.NET Core MVC with Razor Views
- Azure Cosmos DB SDK
- Bootstrap for UI styling

Application Requirements:
- Provide a clean UI for:
  - Entering Cosmos DB configuration
  - Selecting database/container
  - Adding JSON documents
  - Updating existing documents
  - Viewing(Getting) existing documents
  - Applying patch operations
  - Viewing operation results/errors
- Include controllers/services for backend logic.
- Separate UI, business logic, and data access layers.

Patch Requirements:
- Support partial updates without replacing the full document.
- Use Cosmos DB Patch Operations.

Validation & Error Handling:
- Validate:
  - id
  - partition key
  - valid JSON payload
- Show user-friendly validation and error messages.

Additional Features:
- Environment-based configuration support.
- README with setup and usage instructions.

Goal:
The application should allow users to connect to any Azure Cosmos DB instance and perform generic document operations through a user-friendly interface without modifying code.