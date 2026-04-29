# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution overview

`UnifiedAutomationFramework` is a .NET 8 test automation solution with five projects and a strict dependency hierarchy:

```
UAF.Core   ← shared foundation (config, logging, DI, base classes)
  ↑           ↑           ↑
UAF.UI    UAF.API    UAF.Reporting
  ↑           ↑           ↑
         UAF.Tests (references all four)
```

No project other than `UAF.Tests` may reference `UAF.UI`, `UAF.API`, or `UAF.Reporting` directly. Cross-cutting concerns go in `UAF.Core`.

## Build & test commands

```bash
# Build entire solution
dotnet build UnifiedAutomationFramework.sln

# Run all tests
dotnet test UAF.Tests/UAF.Tests.csproj

# Run a single test by name
dotnet test UAF.Tests/UAF.Tests.csproj --filter "FullyQualifiedName~MyTestName"

# Run tests in a specific folder category
dotnet test UAF.Tests/UAF.Tests.csproj --filter "TestCategory=UI"
dotnet test UAF.Tests/UAF.Tests.csproj --filter "TestCategory=API"
dotnet test UAF.Tests/UAF.Tests.csproj --filter "TestCategory=E2E"

# Install Playwright browsers (required once after adding/updating Microsoft.Playwright)
pwsh UAF.UI/bin/Debug/net8.0/playwright.ps1 install
```

## Project responsibilities

| Project | Purpose | Key packages |
|---|---|---|
| `UAF.Core` | Config loading, DI container setup, Serilog wiring, base classes, shared utilities | Serilog, Microsoft.Extensions.* |
| `UAF.UI` | Playwright page objects and reusable components — Playwright **only** (Selenium deferred to a future legacy module) | Microsoft.Playwright 1.59.0 |
| `UAF.API` | REST client wrappers, request/response models, service layer | RestSharp 114, Newtonsoft.Json 13 |
| `UAF.Reporting` | Allure report helpers and custom attributes | Allure.NUnit 2.15.0 |
| `UAF.Tests` | All runnable tests, organised under `UI/`, `API/`, `E2E/` sub-folders | NUnit 3.14, FluentAssertions **6.12.2** (pinned), Bogus |

## Key package constraints

- **FluentAssertions is pinned at `6.12.2`** — do not upgrade to 7.x or 8.x.
- **UAF.UI uses Microsoft.Playwright, not Selenium.** Selenium will be added later as a separate legacy module; do not add it to `UAF.UI`.
- **Reporting is Allure** (`Allure.NUnit`) — not ExtentReports or any other framework.

## Folder conventions

```
UAF.Core/
  Config/      # IConfiguration setup, appsettings readers
  Driver/      # Browser/client driver factory or lifecycle
  Base/        # Abstract base classes (e.g. BaseTest, BasePage)
  Listeners/   # NUnit event listeners, hooks
  Utils/       # Generic helpers with no project-specific dependencies

UAF.UI/
  Pages/       # One class per page (Page Object Model)
  Components/  # Reusable UI components shared across pages

UAF.API/
  Client/      # RestSharp client wrappers
  Models/      # Request/response DTOs
  Services/    # Higher-level service classes that compose client calls

UAF.Tests/
  UI/          # Playwright-based browser tests
  API/         # REST API tests
  E2E/         # End-to-end flows that combine UI + API
```
