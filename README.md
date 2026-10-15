# NOQTA Invoice

**A professional native Windows desktop invoicing application for NOQTA Agency.**

NOQTA Invoice is being developed as a maintainable Windows desktop application using **.NET 10** and **WPF**. It follows a layered architecture intended to keep business rules independent from persistence and user-interface concerns.

> **Project status:** In development. The latest reported automated validation passed, but the application is not being presented as a production-ready release. Some robustness work and further manual verification remain.

## Overview

The application is designed to support NOQTA Agency's invoice workflow through a native desktop interface. The current project includes work on:

- Customer management, including customer details and editing workflows.
- Company profile management.
- Invoice creation and invoice listing.
- Automatic invoice-number generation.
- SQLite-backed persistence.
- Automated tests across the Domain, Application, and Infrastructure layers.

Features and workflows may continue to evolve as development progresses.

## Technology stack

- **.NET 10**
- **C#**
- **Windows Presentation Foundation (WPF)**
- **Entity Framework Core**
- **SQLite**
- **.NET test projects**

## Architecture

The solution is organized into four main layers:

```text
NOQTA.Invoice
├── src/
│   ├── NOQTA.Invoice.Domain/
│   ├── NOQTA.Invoice.Application/
│   ├── NOQTA.Invoice.Infrastructure/
│   └── NOQTA.Invoice.Desktop/
├── tests/
│   ├── NOQTA.Invoice.Domain.Tests/
│   ├── NOQTA.Invoice.Application.Tests/
│   └── NOQTA.Invoice.Infrastructure.Tests/
└── NOQTA.Invoice.slnx
```

### Layer responsibilities

- **Domain** — core business entities, value objects, and business rules.
- **Application** — use cases and persistence abstractions.
- **Infrastructure** — database persistence and implementations of application abstractions.
- **Desktop** — the WPF user interface and desktop application composition.
- **Tests** — automated verification for the Domain, Application, and Infrastructure layers.

The intended dependency direction is from the Desktop and Infrastructure layers toward the Application layer, and from the Application layer toward the Domain layer. The Domain layer remains independent of infrastructure and UI implementation details.

## Invoice numbering

New invoices use a globally sequential number across customers. The current format is:

```text
Invoice <Customer Name> - <six-digit sequence>
```

Examples:

```text
Invoice ACME SARL - 000001
Invoice Mustapha Ihoum - 000002
Invoice ACME SARL - 000003
```

The sequence is global rather than restarting for each customer. Existing historical invoice numbers using a different format are intended to remain unchanged.

Concurrency behavior, number allocation when persistence fails, and long customer-name constraints remain areas for further hardening and verification.

## Prerequisites

- Windows
- The **.NET 10 SDK**
- Git, if cloning the repository

Verify the installed SDK:

```powershell
dotnet --list-sdks
```

## Build and test

Run these commands from the repository root.

Restore dependencies:

```powershell
dotnet restore NOQTA.Invoice.slnx
```

Build the solution:

```powershell
dotnet build NOQTA.Invoice.slnx
```

Run all tests:

```powershell
dotnet test NOQTA.Invoice.slnx
```

At the latest reported checkpoint, the project had **138 passing tests, 0 failing tests, and 0 skipped tests**, and the full solution build succeeded. These are the results reported for that checkpoint; rerun the commands above to verify the current checkout.

## Run the desktop application

After restoring and building successfully, launch the WPF desktop project from the repository root:

```powershell
dotnet run --project src/NOQTA.Invoice.Desktop/NOQTA.Invoice.Desktop.csproj
```

The application is a native Windows desktop application, not a web application.

## Development principles

The project is developed incrementally with an emphasis on:

- Clear separation of responsibilities.
- Preserving established domain rules.
- Testable application use cases.
- Explicit persistence boundaries.
- Validation through automated tests and full-solution builds.

## Current limitations

This repository is under active development. Before using the application for real business operations, review the current implementation and verify the complete workflow. In particular, automatic invoice numbering still requires additional hardening around concurrent creation, failed saves, and generated-number length constraints.

## Contributing

The project structure and business rules are intentionally controlled. Before proposing broad refactors or changing domain behavior, discuss the requirement and validate the existing implementation and tests.

## License

No license has been specified yet. Unless and until a license is added to this repository, do not assume that the code is licensed for reuse, redistribution, or modification.




APP SCREENSHOTS

<img width="2000" height="1414" alt="1" src="https://github.com/user-attachments/assets/f844fc0e-536b-4b1e-b06f-8b157fb53d9b" />

<img width="2000" height="1414" alt="2" src="https://github.com/user-attachments/assets/6671d182-d280-4faa-90aa-27aaef582a9d" />

<img width="2000" height="1414" alt="3" src="https://github.com/user-attachments/assets/68660063-1f83-443c-b9a8-2f1ac9aac506" />

<img width="2000" height="1414" alt="4" src="https://github.com/user-attachments/assets/73a6870e-d928-4dab-8bcf-f76c304705a7" />

<img width="2000" height="1414" alt="5" src="https://github.com/user-attachments/assets/49e1f30c-7605-49ad-aad5-c9509d4d4c7f" />

<img width="2000" height="1414" alt="6" src="https://github.com/user-attachments/assets/d8618086-8b21-4bd3-8b65-f7f586ec4078" />

<img width="2000" height="1414" alt="7" src="https://github.com/user-attachments/assets/481d5d82-b9d1-4cde-8a29-ff90e0bf6901" />
