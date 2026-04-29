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

## Naming conventions
- Classes and methods:  PascalCase
- Private fields:       _camelCase with underscore prefix
- Local variables:      camelCase
- Constants:            UPPER_SNAKE_CASE
- Test methods:         Should_ExpectedBehaviour_WhenCondition

## Design patterns in use
- Page Object Model (POM) for UI layer
- Builder Pattern for test data factories
- Singleton for DriverManager and ConfigManager
- Factory Pattern for browser/driver creation
- Strategy Pattern for environment switching

## Commit message rules
- Follow Conventional Commits format strictly
- Allowed prefixes: feat, fix, chore, refactor, test, docs, ci
- Example: feat(ui): add LoginPage page object
- NEVER mention Claude, AI, or any tool in commit messages
- NEVER add Co-authored-by or Generated-by lines
- Subject line must be under 72 characters
- Use present tense ("add feature" not "added feature")

## Environment config
- Base config:    appsettings.json (committed with placeholder values only)
- Local config:   appsettings.local.json (gitignored, never commit)
- Never hardcode URLs, credentials, or environment-specific values in code

## What not to do
- Do not upgrade FluentAssertions beyond 6.12.2
- Do not add Selenium to UAF.UI — it belongs in its own legacy module
- Do not use Thread.Sleep() — use Playwright's built-in auto-waiting
- Do not hardcode test data — use Bogus factories
- Do not put assertions or test logic inside page objects
- Do not commit secrets, API keys, or local config files

## BDD / TDD approach
- TDD supported natively via NUnit — no extra tooling needed
- BDD adopted as a style only — Given/When/Then as comments inside NUnit tests
- No Gherkin runtime — SpecFlow deferred to roadmap
- Tests read like BDD without the overhead of a BDD framework

## Reporting
- Allure.NUnit is the locked reporting choice — do not swap or remove
- Console reporting via Serilog runs alongside Allure on every execution
- Step() wrapper in BaseTest handles all Allure grouping, logging, and evidence capture
- Every Step() call writes to both console and Allure automatically
- Roadmap: pluggable IReporter interface supporting multiple reporters simultaneously

## Steps philosophy
- Step() lives in BaseTest — not in a separate StepClass hierarchy
- Page objects handle UI mechanics only — no reporting, no assertions
- Test authors call Step() with a plain English name and a lambda
- Junior devs should never need to touch AllureApi directly

## Test management integration
- ITestManagementClient interface stubbed in UAF.Core/Integrations/
- [TestCaseId("PROJ-1234")] for Jira Xray
- [AzureTestCaseId("12345")] for Azure DevOps Test Plans
- No live implementation in MVP — architecture only
- Roadmap: Jira Xray client, Azure DevOps client

## Branching strategy

- Never commit directly to main
- All features are developed in a dedicated branch
- Branch naming convention: `feature/issue-number-short-description`
  - `feature/8-config-manager`
  - `feature/9-driver-factory`
  - `feature/12-base-page`
- Before starting any feature, create and checkout the branch:
  ```bash
  git checkout -b feature/N-short-description
  ```
- Write unit tests to validate the feature as part of the same branch
- CI checks must pass before merging
- Merge into main via GitLab merge request only — never merge locally
- Delete the branch after the merge request is accepted

## Architecture decisions

### Configuration philosophy

UAF is a CommonLibrary — it owns no configuration values.

- `ConfigManager` lives in `UAF.Core/Config/` and knows HOW to load config, not what the values are
- At runtime it loads from `AppContext.BaseDirectory` — wherever the executing test binary lives
- `UAF.Core` ships no `appsettings.json` — it is infrastructure only
- `UAF.Tests` owns its own `appsettings.json` and `appsettings.local.json` for framework self-testing only
- Consumer projects (e.g. `ABCBankDotComAutomation`) own their own `appsettings.json` and `appsettings.local.json`
- Consumers are guided by `appsettings.example.json` or README schema documentation

## Roadmap items (do not build in MVP)
- SpecFlow BDD runtime
- StepClass hierarchy extending Steps base
- IReporter multi-reporter interface
- Jira Xray integration
- Azure DevOps Test Plans integration
- Selenium legacy module
- Appium mobile layer
- Database validation layer
- NuGet package publishing