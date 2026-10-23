# User Management API

A complete, beginner-friendly ASP.NET Core 8 Web API project built for the Coursera assignment: **User Management API with CRUD Operations, Validation, Copilot Debugging, and Custom Middleware**.

This project implements a robust RESTful API with full CRUD capabilities, data annotations for input validation, thread-safe in-memory storage, custom structured request logging middleware, interactive Swagger/OpenAPI documentation, and a ready-to-run automated test suite.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Key Features](#key-features)
3. [Technologies Used](#technologies-used)
4. [Prerequisites](#prerequisites)
5. [Project Structure](#project-structure)
6. [Installation and Run Instructions](#installation-and-run-instructions)
7. [API Endpoints Reference](#api-endpoints-reference)
8. [Request & Response Models](#request--response-models)
9. [Validation Rules](#validation-rules)
10. [Example curl Commands](#example-curl-commands)
11. [Invalid Requests and Error Responses](#invalid-requests-and-error-responses)
12. [Custom Middleware Explanation](#custom-middleware-explanation)
13. [How to Test the API](#how-to-test-the-api)
14. [GitHub Copilot Debugging Guide](#github-copilot-debugging-guide)
15. [Known Limitations](#known-limitations)
16. [Publishing to GitHub](#publishing-to-github)

---

## Project Overview

The **User Management API** is designed to provide complete lifecycle management for user accounts. It demonstrates core ASP.NET Core competencies required for modern backend development:

- **Controllers-based Architecture**: Clean separation of concerns with `UsersController`.
- **Standard REST Semantics**: Rigorous adherence to HTTP status codes (`200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `404 Not Found`, `409 Conflict`).
- **Data Validation**: Automatic request model validation via `[ApiController]` and System.ComponentModel.DataAnnotations.
- **Thread Safety**: Concurrent collections, atomic ID generation via `Interlocked`, and atomic locks to prevent race conditions during email uniqueness checks.
- **Custom Middleware**: High-performance request timing and structured logging via `RequestLoggingMiddleware`.
- **Developer Experience**: Interactive Swagger UI served at the application root (`/`).

---

## Key Features

- **Full CRUD Support**: Create, Read (all or single), Update, and Delete user entities.
- **Seeded Data**: Pre-loaded with sample users for immediate testing out of the box.
- **Server-Generated Identifiers**: Prevents client-side ID spoofing or collision.
- **Case-Insensitive Email Uniqueness**: Enforced across creates and updates.
- **Automatic String Trimming**: Names and email addresses are automatically trimmed of extraneous whitespace upon deserialization.
- **Structured Request Logging**: Measures elapsed execution time using `Stopwatch` and records HTTP method, path, and response status code.
- **Exception Safety**: The logging middleware captures and rethrows unexpected exceptions without swallowing errors.

---

## Technologies Used

- **Language**: C# 12
- **Framework**: ASP.NET Core 8.0 (LTS)
- **API Architecture**: Controllers-based Web API
- **Documentation**: Swagger / OpenAPI via Swashbuckle (`Swashbuckle.AspNetCore 6.6.2`)
- **Runtime & SDK**: .NET SDK 8.0
- **Version Control**: Git & GitHub

---

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher.
- `curl` or any API client (Postman, Bruno, or a web browser for Swagger).
- (Optional) Git for version control.

To verify your installed .NET SDK version, run:
```bash
dotnet --version
```

---

## Project Structure

```
UserManagementAPI/
├── Controllers/
│   └── UsersController.cs            # Full CRUD endpoints with validation and error handling
├── Models/
│   ├── User.cs                       # User domain entity
│   ├── UserCreateRequest.cs          # DTO for POST /api/users with data annotations
│   └── UserUpdateRequest.cs          # DTO for PUT /api/users/{id} with data annotations
├── Middleware/
│   └── RequestLoggingMiddleware.cs    # Custom request timing & structured logging middleware
├── Properties/
│   └── launchSettings.json           # Local launch profiles (port 5123)
├── Program.cs                        # Application bootstrap, DI container, and middleware pipeline
├── appsettings.json                  # Logging configuration and app settings
├── UserManagementAPI.csproj          # Project file and NuGet dependencies
├── test_api.sh                       # Automated test script testing all 14 scenarios
├── .gitignore                        # Git exclusion rules for .NET
└── README.md                         # Comprehensive documentation
```

---

## Installation and Run Instructions

### 1. Clone or Navigate to the Project Directory

```bash
cd UserManagementAPI
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Project

```bash
dotnet build
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 4. Run the API Locally

```bash
dotnet run
```

By default, the API will start listening on:
- **Base URL**: `http://localhost:5123`
- **Swagger UI**: `http://localhost:5123/` (served at the root path for convenience)
- **API Endpoint**: `http://localhost:5123/api/users`

Press `Ctrl + C` in the terminal to stop the server.

---

## API Endpoints Reference

| HTTP Method | Route | Description | Success Status | Error Statuses |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/users` | Retrieve all registered users | `200 OK` | `500 Internal Server Error` |
| `GET` | `/api/users/{id}` | Retrieve a single user by ID | `200 OK` | `404 Not Found` |
| `POST` | `/api/users` | Create a new user (server generates ID) | `201 Created` | `400 Bad Request`, `409 Conflict` |
| `PUT` | `/api/users/{id}` | Update existing user details | `200 OK` | `400 Bad Request`, `404 Not Found`, `409 Conflict` |
| `DELETE` | `/api/users/{id}` | Remove a user by ID | `204 No Content` | `404 Not Found` |

---

## Request & Response Models

### 1. User Entity (`Models/User.cs`)
Returned by `GET`, `POST`, and `PUT` endpoints:
```json
{
  "id": 1,
  "name": "Alice Johnson",
  "email": "alice.johnson@example.com",
  "age": 28,
  "createdAtUtc": "2026-10-23T15:55:25.595539Z"
}
```

### 2. UserCreateRequest (`Models/UserCreateRequest.cs`)
Payload for `POST /api/users`. Note that `id` is omitted because it is assigned safely by the server:
```json
{
  "name": "Diana Prince",
  "email": "diana.prince@example.com",
  "age": 30
}
```

### 3. UserUpdateRequest (`Models/UserUpdateRequest.cs`)
Payload for `PUT /api/users/{id}`:
```json
{
  "name": "Diana Wayne",
  "email": "diana.wayne@example.com",
  "age": 31
}
```

---

## Validation Rules

Input validation is enforced using `System.ComponentModel.DataAnnotations` attributes and `[ApiController]`, which automatically formats validation errors into standard RFC 7807/9110 Problem Details responses:

1. **Name**:
   - `[Required(ErrorMessage = "Name is required.")]`
   - `[StringLength(80, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 80 characters.")]`
   - Automatically trimmed of leading and trailing whitespace.
2. **Email**:
   - `[Required(ErrorMessage = "Email is required.")]`
   - `[EmailAddress(ErrorMessage = "Invalid email address format.")]`
   - `[StringLength(120, ErrorMessage = "Email must not exceed 120 characters.")]`
   - Automatically trimmed and verified to be unique across all users (case-insensitive).
3. **Age**:
   - `[Required(ErrorMessage = "Age is required.")]`
   - `[Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]`
4. **ID Handling**:
   - IDs are not accepted in `UserCreateRequest`.
   - The server assigns incrementing IDs via `Interlocked.Increment(ref _nextId)` to guarantee thread safety.

---

## Example curl Commands

Ensure the application is running (`dotnet run`) before executing these commands:

### 1. Get All Users (`GET /api/users`)
```bash
curl -i http://localhost:5123/api/users
```
**Expected Response**: `HTTP/1.1 200 OK` with JSON array of users.

### 2. Get User By ID (`GET /api/users/1`)
```bash
curl -i http://localhost:5123/api/users/1
```
**Expected Response**: `HTTP/1.1 200 OK` with user details.

### 3. Create a User (`POST /api/users`)
```bash
curl -i -X POST http://localhost:5123/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Diana Prince",
    "email": "diana.prince@example.com",
    "age": 30
  }'
```
**Expected Response**: `HTTP/1.1 201 Created` with a `Location: http://localhost:5123/api/Users/4` response header and the created user JSON in the body.

### 4. Update an Existing User (`PUT /api/users/4`)
```bash
curl -i -X PUT http://localhost:5123/api/users/4 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Diana Wayne",
    "email": "diana.prince@example.com",
    "age": 31
  }'
```
**Expected Response**: `HTTP/1.1 200 OK` with updated user object.

### 5. Delete a User (`DELETE /api/users/4`)
```bash
curl -i -X DELETE http://localhost:5123/api/users/4
```
**Expected Response**: `HTTP/1.1 204 No Content`.

---

## Invalid Requests and Error Responses

### 1. Nonexistent User ID (`GET /api/users/999`)
```bash
curl -i http://localhost:5123/api/users/999
```
**Response**: `HTTP/1.1 404 Not Found`
```json
{
  "message": "User with ID 999 was not found."
}
```

### 2. Invalid Email Format (`POST /api/users`)
```bash
curl -i -X POST http://localhost:5123/api/users \
  -H "Content-Type: application/json" \
  -d '{"name": "John Doe", "email": "not-an-email", "age": 25}'
```
**Response**: `HTTP/1.1 400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["Invalid email address format."]
  }
}
```

### 3. Out-of-Range Age (`POST /api/users`)
```bash
curl -i -X POST http://localhost:5123/api/users \
  -H "Content-Type: application/json" \
  -d '{"name": "John Doe", "email": "john@example.com", "age": 150}'
```
**Response**: `HTTP/1.1 400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Age": ["Age must be between 1 and 120."]
  }
}
```

### 4. Duplicate Email on Creation (`POST /api/users`)
```bash
curl -i -X POST http://localhost:5123/api/users \
  -H "Content-Type: application/json" \
  -d '{"name": "Copycat", "email": "ALICE.JOHNSON@example.com", "age": 28}'
```
**Response**: `HTTP/1.1 409 Conflict`
```json
{
  "message": "A user with email 'ALICE.JOHNSON@example.com' already exists."
}
```

### 5. Duplicate Email on Update (`PUT /api/users/2`)
Attempting to change user 2's email to user 1's email:
```bash
curl -i -X PUT http://localhost:5123/api/users/2 \
  -H "Content-Type: application/json" \
  -d '{"name": "Bob Smith", "email": "alice.johnson@example.com", "age": 34}'
```
**Response**: `HTTP/1.1 409 Conflict`
```json
{
  "message": "Another user with email 'alice.johnson@example.com' already exists."
}
```

---

## Custom Middleware Explanation

The custom middleware is implemented in `Middleware/RequestLoggingMiddleware.cs`.

### How It Works

1. **Request Interception**: It captures the incoming HTTP method (`context.Request.Method`) and request path (`context.Request.Path`).
2. **Timing Execution**: It initializes a `System.Diagnostics.Stopwatch` before calling `await _next(context)`.
3. **Structured Logging**: After downstream middleware and the controller action complete, it logs the HTTP method, path, response status code, and execution time using `ILogger<RequestLoggingMiddleware>`:
   ```csharp
   _logger.LogInformation(
       "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
       method, path, statusCode, stopwatch.ElapsedMilliseconds);
   ```
4. **Exception Handling & Rethrow**: If an unhandled exception occurs in downstream handlers, the middleware logs an error with the stack trace and elapsed time, and rethrows the exception using `throw;`. This ensures errors are recorded while avoiding silently swallowing exceptions.
5. **Registration**: In `Program.cs`, it is registered early in the pipeline via `app.UseRequestLogging();` to measure full pipeline latency.

### Logging Output Example
When requests are processed, the console logs appear as:
```text
info: UserManagementAPI.Middleware.RequestLoggingMiddleware[0]
      HTTP GET /api/users responded 200 in 33 ms
info: UserManagementAPI.Middleware.RequestLoggingMiddleware[0]
      HTTP POST /api/users responded 201 in 9 ms
info: UserManagementAPI.Middleware.RequestLoggingMiddleware[0]
      HTTP DELETE /api/users/4 responded 204 in 1 ms
```

---

## How to Test the API

### Method 1: Using the Automated Test Script

An end-to-end bash test suite is included in the project: `test_api.sh`. It automatically boots the API server, executes 14 distinct test cases, validates status codes and payload contents, verifies the middleware logs, and terminates the server cleanly.

Run the test suite:
```bash
chmod +x test_api.sh
./test_api.sh
```

**Verified Test Scenarios:**
- [x] Test 1: `GET /api/users` (Initial seed data, returns 200 OK)
- [x] Test 2: `GET /api/users/1` (Existing user, returns 200 OK)
- [x] Test 3: `GET /api/users/999` (Nonexistent user, returns 404 Not Found)
- [x] Test 4: `POST /api/users` with invalid email format (returns 400 Bad Request)
- [x] Test 5: `POST /api/users` with age 150 (returns 400 Bad Request)
- [x] Test 6: `POST /api/users` with duplicate email (returns 409 Conflict)
- [x] Test 7: `POST /api/users` with valid payload (returns 201 Created with Location header)
- [x] Test 8: `GET /api/users/4` (Verifies created user was stored, returns 200 OK)
- [x] Test 9: `PUT /api/users/4` (Valid update, returns 200 OK)
- [x] Test 10: `PUT /api/users/4` with conflicting email (returns 409 Conflict)
- [x] Test 11: `PUT /api/users/999` (Nonexistent user update, returns 404 Not Found)
- [x] Test 12: `DELETE /api/users/4` (Deletes user, returns 204 No Content)
- [x] Test 13: `GET /api/users/4` (Confirms deletion, returns 404 Not Found)
- [x] Test 14: `DELETE /api/users/999` (Nonexistent user deletion, returns 404 Not Found)
- [x] Verification: Custom middleware logs confirm method, path, status, and elapsed ms.

### Method 2: Interactive Swagger UI

1. Run the application: `dotnet run`
2. Open your browser and navigate to: `http://localhost:5123/`
3. Use the interactive **Try it out** button on any endpoint to send requests and inspect responses directly.

---

## GitHub Copilot Debugging Guide

For the Coursera rubric requirement on using GitHub Copilot to review and debug code:

### 1. Suggested Prompt for GitHub Copilot Chat

Open GitHub Copilot Chat in your editor (e.g. VS Code or Visual Studio) and submit the following prompt:

```text
Review UsersController.cs and RequestLoggingMiddleware.cs in my ASP.NET Core 8 Web API.
Please analyze the code for:
1. Concurrency and thread safety: Are atomic operations and locks properly applied to prevent race conditions during user creation, ID increments, and email uniqueness checks?
2. Email uniqueness handling: Does POST reject duplicates case-insensitively? Does PUT allow the current user to keep their email while preventing collisions with other users?
3. Input validation: Are DataAnnotations properly configured for Name (2-80 chars), Email format, and Age (1-120)? Is user input properly trimmed?
4. HTTP Status Codes: Are responses compliant with REST standards (200 for reads/updates, 201 with Location header for create, 204 for delete, 400 for validation errors, 404 for missing resources, 409 for duplicate emails)?
5. Middleware: Does RequestLoggingMiddleware accurately measure elapsed time using Stopwatch, use structured logging without string concatenation, and rethrow exceptions?
Suggest any edge cases or potential improvements.
```

### 2. Practical Copilot Debugging Exercise

You can demonstrate Copilot's debugging capabilities with this simple exercise:

1. In `Controllers/UsersController.cs`, locate the `UpdateUser` method and intentionally alter the email conflict condition from:
   ```csharp
   var emailInUseByOther = _users.Values.Any(u =>
       u.Id != id && string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase));
   ```
   to:
   ```csharp
   var emailInUseByOther = _users.Values.Any(u =>
       string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase));
   ```
2. Ask Copilot Chat:
   > *"When I send a PUT request to update a user's name without modifying their email address, the API returns a 409 Conflict. Why is this happening and how do I fix it?"*
3. **Copilot's Finding**: Copilot will identify that omitting `u.Id != id` causes the method to match the user's *own* existing email in the collection, treating an update as a conflict.
4. **Resolution**: Restore `u.Id != id` to verify that conflict checks only match *other* user entities.

---

## Known Limitations

- **In-Memory Storage**: Data is maintained in memory (`ConcurrentDictionary<int, User>`) for simplicity. Any data created, modified, or deleted will reset back to the initial seeded users whenever the application restarts or crashes. In a production environment, this in-memory store would be replaced with Entity Framework Core and a persistent database (e.g. PostgreSQL, SQL Server, or SQLite).
- **HTTP / HTTPS Certificate**: The application profile is configured to run over HTTP on port `5123` to eliminate HTTPS development certificate trust issues across different student environments.

---

## Publishing to GitHub

To submit your project for Coursera peer review, follow these steps to push this code to a public GitHub repository:

### Step 1: Create a New Public Repository on GitHub
1. Open your web browser and go to [GitHub New Repository](https://github.com/new).
2. Set the **Repository name** to: `UserManagementAPI`.
3. Set the visibility to **Public**.
4. **Do not** initialize the repository with a README, `.gitignore`, or license (these files are already created in your local project).
5. Click **Create repository**.

### Step 2: Initialize Git and Push Local Code
Open your terminal in the `UserManagementAPI` directory and run:

```bash
# Ensure you are inside the UserManagementAPI directory
cd UserManagementAPI

# Initialize a new git repository
git init -b main

# Add all project files
git add .

# Create the initial commit
git commit -m "Initial commit: Complete ASP.NET Core User Management API with CRUD, validation, middleware, and tests"

# Add your GitHub repository as the origin remote (replace YOUR_GITHUB_USERNAME)
git remote add origin https://github.com/YOUR_GITHUB_USERNAME/UserManagementAPI.git

# Push to the main branch
git push -u origin main
```

### Step 3: Submit to Coursera
Copy the URL of your public GitHub repository (`https://github.com/YOUR_GITHUB_USERNAME/UserManagementAPI`) and submit it on the Coursera assignment submission page.
