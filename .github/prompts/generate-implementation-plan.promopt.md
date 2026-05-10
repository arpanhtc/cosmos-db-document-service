
---
name: generate-implementation
description: Generates implementation plan documents based on provided plan file for .NET project.
---

# AI Implementation Document Generator

You are a Senior .NET Solution Architect.

I am  providing a `....plan.md` containing project requirements, architecture decisions, and discussions.

Your task is to generate 3–5 implementation documents (`part-1.md`, `part-2.md`, etc.) that break the project into logical implementation phases or modules.

---

# Goal

Create implementation documents that are:

- practical
- implementation-focused
- AI-agent friendly
- executable with minimal ambiguity

Each document should contain enough context for another AI coding agent to implement the assigned scope correctly.

Avoid unnecessary repetition.

---

# Important Guidelines

## 1. Keep Documents Mostly Independent

Each implementation document should include:

- required local context
- assumptions relevant to that scope
- implementation steps
- contracts/interfaces if needed
- API/database details if needed

Do NOT blindly repeat everything from all other parts.

Only include context necessary for correct implementation.

---

## 2. Shared Information

If something is globally applicable (coding standards, naming conventions, architecture principles, folder structure), place it in:

- `<yyy-MM-dd>_<name>-shared-context.md` or `<yyy-MM-dd>_<name>-shared-instructions.md`

Other implementation docs may reference these shared docs.

---

## 3. Prefer Explicit Steps Over Excessive Context

Well-defined execution steps are more important than repeating large architecture explanations.

Focus on:
- what to build
- where to build it
- how components interact
- validation rules
- expected outputs

---

# Required Output Files

Generate:

- `<yyy-MM-dd>_<name>_shared-context.md`
- `<yyyy-MM-dd>_<name>_implementation_part-1-*.md`
- `<yyyy-MM-dd>_<name>_implementation_part-2-*.md`
- `<yyyy-MM-dd>_<name>_implementation_part-3-*.md`
- optional additional parts if needed

---

# Suggested Structure For Each Part

Each implementation document should contain:

# Title

# Objective

# Scope

# Dependencies (only if relevant)

# Required Context

Include only context needed for this implementation.

---

# Implementation Steps

Provide step-by-step implementation guidance including:

- files/folders to create
- responsibilities
- APIs/endpoints
- entities/models
- database changes
- service interfaces
- configuration updates
- dependency injection
- testing requirements

Use concrete implementation instructions.

---

# Validation & Rules

Document important business rules and validation logic.

---

# Testing Requirements (would be greate if we have separate implementation plan for unit tests)

Include:
- unit tests
- integration tests
- important scenarios

---

# Definition of Done

Provide measurable completion criteria.

---

# AI Agent Notes

Include execution guidance for AI coding agents:
- implementation order
- important constraints
- expected artifacts
- things to avoid

---

# Important Constraint

Avoid references like:
- "implemented earlier"
- "existing service"
- "previous part"

Instead:
- briefly restate required context when necessary
- or reference shared documents if sufficient

The implementation document should still remain executable without confusion.

---

# Output Style

Keep output:
- engineering-focused
- actionable
- low ambiguity

Avoid:
- filler text
- generic theory
- unnecessary repetition

---

# Output Delivery

Generate the output as downloadable markdown files only; do not render full document contents in the chat/UI response.

---