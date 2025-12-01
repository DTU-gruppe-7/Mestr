# Mestr - Project Management System

<div align="center">

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=csharp)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D6?style=for-the-badge&logo=windows)
![SQLite](https://img.shields.io/badge/SQLite-Database-003B57?style=for-the-badge&logo=sqlite)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**A modern, professional project management system built with .NET 9 and WPF**

[Features](#features) • [Architecture](#architecture) • [Getting Started](#getting-started) • [Usage](#usage) • [Contributing](#contributing)

</div>

---

## ?? Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Usage Guide](#usage-guide)
- [Code Quality](#code-quality)
- [Testing](#testing)
- [Contributing](#contributing)
- [License](#license)

---

## ?? Overview

**Mestr** is a comprehensive project management system designed for Danish businesses to manage clients, projects, expenses, earnings, and generate professional invoices. Built with modern .NET 9 and WPF, it provides an intuitive desktop application with robust data management capabilities.

### Key Highlights

- ? **Modern Architecture**: Clean layered architecture with dependency injection
- ? **Type-Safe**: Full nullable reference type support for enhanced reliability
- ? **Async-First**: Proper async/await implementation throughout
- ? **Validated**: Factory pattern with comprehensive validation
- ? **Maintainable**: Centralized constants and consistent error handling
- ? **Tested**: Comprehensive test suite with 32+ tests
- ? **Professional**: Generate PDF invoices with VAT calculations

---

## ?? Features

### Client Management
- ? Create and manage client profiles (B2B and B2C)
- ? Store complete contact information
- ? CVR validation for business clients
- ? International phone number support (E.164 standard)
- ? Email validation
- ? Client project history

### Project Management
- ? Create and track multiple projects
- ? Project status tracking (Planlagt, Aktiv, Afsluttet, Aflyst)
- ? Deadline management
- ? Project descriptions and notes
- ? Real-time profit/loss calculations
- ? Color-coded project results
- ? Filter projects by status

### Financial Management
- ? Track expenses by category (Materialer, Løn, Transport, Værktøj, etc.)
- ? Manage earnings/invoices
- ? Mark expenses/earnings as paid
- ? Automatic project profit calculation
- ? Danish VAT (25%) support
- ? B2B vs B2C pricing

### Invoice Generation
- ? Professional PDF invoice generation
- ? Automatic VAT calculation for B2B clients
- ? Company profile integration
- ? Payment terms (14 days default)
- ? Bank account information
- ? Danish locale formatting

### Company Profile
- ? Store company information
- ? Bank account details
- ? CVR registration
- ? Contact information
- ? Automatic inclusion in invoices

---

## ??? Architecture

Mestr follows a **clean layered architecture** with clear separation of concerns:

```
???????????????????????????????????????????????????
?              Mestr.UI (WPF)                     ?
?  - Views (XAML)                                 ?
?  - ViewModels (MVVM)                            ?
?  - Commands & Utilities                         ?
???????????????????????????????????????????????????
               ?
???????????????????????????????????????????????????
?          Mestr.Services                         ?
?  - Business Logic                               ?
?  - Service Layer                                ?
?  - PDF Generation                               ?
???????????????????????????????????????????????????
               ?
???????????????????????????????????????????????????
?           Mestr.Data                            ?
?  - Repository Pattern                           ?
?  - Entity Framework Core                        ?
?  - Database Context                             ?
???????????????????????????????????????????????????
               ?
???????????????????????????????????????????????????
?           Mestr.Core                            ?
?  - Domain Models                                ?
?  - Enums                                        ?
?  - Constants                                    ?
?  - Interfaces                                   ?
???????????????????????????????????????????????????
```

### Design Patterns Used

- **MVVM (Model-View-ViewModel)**: UI separation and testability
- **Repository Pattern**: Data access abstraction
- **Factory Pattern**: Client creation with validation
- **Dependency Injection**: Loose coupling and testability
- **Async/Await**: Non-blocking operations throughout

---

## ?? Technology Stack

### Core Technologies
- **.NET 9.0**: Latest .NET framework with C# 13.0
- **WPF**: Windows Presentation Foundation for desktop UI
- **Entity Framework Core**: ORM for database operations
- **SQLite**: Lightweight embedded database

### Key Libraries
- **QuestPDF**: Professional PDF generation
- **Microsoft.Extensions.DependencyInjection**: IoC container
- **xUnit**: Testing framework
- **System.Net.Mail**: Email validation

### Development
- **Visual Studio 2022**: Primary IDE
- **Git**: Version control
- **NuGet**: Package management

---

## ?? Getting Started

### Prerequisites

- **Windows 10/11**: Required for WPF
- **.NET 9.0 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Visual Studio 2022** (recommended) or **Visual Studio Code**
- **Git**: For cloning the repository

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/DTU-gruppe-7/Mestr.git
   cd Mestr
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   cd Mestr.UI
   dotnet run
   ```

### First Run

On first launch, you'll be prompted to create a **Company Profile**:
1. Enter your company information
2. Provide bank account details (for invoices)
3. Add CVR number if applicable
4. Save the profile

The database will be automatically created in the application directory.

---

## ?? Project Structure

```
Mestr/
??? Mestr.Core/                    # Domain layer
?   ??? Constants/                 # Application constants
?   ?   ??? AppConstants.cs        # Centralized constants
?   ??? Enum/                      # Enumerations
?   ?   ??? ProjectStatus.cs       # Project statuses
?   ?   ??? ExpenseCategory.cs     # Expense categories
?   ?   ??? InvoiceStatus.cs       # Invoice statuses
?   ??? Model/                     # Domain models
?       ??? Client.cs              # Client entity (with factory)
?       ??? Project.cs             # Project entity
?       ??? Expense.cs             # Expense entity
?       ??? Earning.cs             # Earning entity
?       ??? Invoice.cs             # Invoice entity
?       ??? CompanyProfile.cs      # Company profile
?
??? Mestr.Data/                    # Data access layer
?   ??? DbContext/                 # Database context
?   ?   ??? dbContext.cs           # EF Core context
?   ??? Interface/                 # Data interfaces
?   ?   ??? IRepository.cs         # Generic repository
?   ??? Repository/                # Repository implementations
?       ??? ClientRepository.cs
?       ??? ProjectRepository.cs
?       ??? ExpenseRepository.cs
?       ??? EarningRepository.cs
?       ??? CompanyProfileRepository.cs
?
??? Mestr.Services/                # Business logic layer
?   ??? Interface/                 # Service interfaces
?   ?   ??? IClientService.cs
?   ?   ??? IProjectService.cs
?   ?   ??? IExpenseService.cs
?   ?   ??? IEarningService.cs
?   ?   ??? IPdfService.cs
?   ??? Service/                   # Service implementations
?       ??? ClientService.cs
?       ??? ProjectService.cs
?       ??? ExpenseService.cs
?       ??? EarningService.cs
?       ??? PdfService.cs
?
??? Mestr.UI/                      # Presentation layer (WPF)
?   ??? View/                      # XAML views
?   ?   ??? MainWindow.xaml
?   ?   ??? DashboardView.xaml
?   ?   ??? ProjectDetailView.xaml
?   ?   ??? ClientView.xaml
?   ?   ??? AddClientWindow.xaml
?   ?   ??? EconomyWindow.xaml
?   ?   ??? AddCompanyInfoWindow.xaml
?   ??? ViewModels/                # ViewModels (MVVM)
?   ?   ??? MainViewModel.cs
?   ?   ??? DashboardViewModel.cs
?   ?   ??? ProjectDetailViewModel.cs
?   ?   ??? ClientViewModel.cs
?   ?   ??? AddClientViewModel.cs
?   ?   ??? EconomyViewModel.cs
?   ?   ??? ViewModelBase.cs
?   ??? Command/                   # Command implementations
?   ?   ??? RelayCommand.cs
?   ??? Utilities/                 # Helper utilities
?   ?   ??? MessageBoxHelper.cs
?   ??? Component/Styles/          # WPF styles
?   ?   ??? Styles.xaml
?   ??? App.xaml.cs                # Application entry point
?
??? Mestr.Test/                    # Test project
    ??? Repository/                # Repository tests
        ??? ClientRepositoryTest.cs
        ??? ProjectRepositoryTest.cs
        ??? ExpenseRepositoryTest.cs
        ??? EarningRepositoryTest.cs
```

---

## ?? Configuration

### appsettings.json

Located in `Mestr.UI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Mestr.db"
  }
}
```

### Database

- **Type**: SQLite
- **Location**: Application directory (`Mestr.db`)
- **Migrations**: Automatic via EF Core `EnsureCreated()`
- **Culture**: Danish (da-DK)

### Constants

All application constants are centralized in `Mestr.Core/Constants/AppConstants.cs`:

- Date/Time formats
- Validation rules
- Error messages (Danish)
- UI constants
- VAT rates
- Invoice templates

---

## ?? Usage Guide

### Creating a Client

1. Navigate to **"Kunder"** (Clients)
2. Click **"Tilføj ny klient"** (Add new client)
3. Fill in required information:
   - Contact person ? Required
   - Email ? Required (validated)
   - Phone number ? Required (8-15 digits)
   - Company name (optional)
   - Address (optional)
   - CVR (optional - for B2B)
4. Click **"Gem"** (Save)

### Creating a Project

1. Click **"Nyt Projekt"** (New Project)
2. Select a client from dropdown
3. Enter project details:
   - Project name ? Required
   - Description (optional)
   - Deadline (optional)
4. Click **"Gem"** (Save)

### Managing Project Finances

1. Open a project from the dashboard
2. Click **"Tilføj Transaktion"** (Add Transaction)
3. Choose type:
   - **Udgift** (Expense): Select category, enter amount
   - **Indtægt** (Earning): Enter amount
4. Mark as paid when completed
5. Click **"Gem Ændringer"** (Save Changes)

### Generating an Invoice

1. Open a project with unpaid earnings
2. Click **"Generer Faktura"** (Generate Invoice)
3. Choose save location
4. PDF is generated with:
   - Company information
   - Client information
   - Unpaid earnings
   - VAT calculation (B2B only)
   - Payment terms
5. Earnings are marked as paid automatically

---

## ?? Code Quality

### Features Implemented

? **Async/Await**: Proper async operations with `ConfigureAwait(false)` in libraries  
? **Nullable Reference Types**: Full nullable support throughout  
? **Factory Pattern**: Client validation at creation  
? **Constants**: 90+ centralized constants  
? **Error Handling**: Comprehensive validation and error messages  
? **Dependency Injection**: IoC container configuration  

### Code Standards

- ? C# 13.0 features utilized
- ? MVVM pattern for UI
- ? Repository pattern for data
- ? Async-first approach
- ? XML documentation (in progress)
- ? Unit of Work pattern (planned)
- ? Logging framework (planned)

---

## ?? Testing

### Running Tests

```bash
dotnet test
```

### Test Coverage

```
? 32/32 tests passing
- ClientRepository: 9 tests
- ProjectRepository: 8 tests
- ExpenseRepository: 8 tests
- EarningRepository: 7 tests
```

### Test Categories

- **Repository Tests**: CRUD operations, validation
- **Integration Tests**: Database interactions
- **Validation Tests**: Email, phone, factory methods

---

## ?? Current Status

### Completed (40%)
- ? Core domain models
- ? Repository pattern
- ? Service layer
- ? WPF UI (MVVM)
- ? PDF invoice generation
- ? Client validation with factory pattern
- ? Constants refactoring
- ? Nullable reference type support
- ? Async/await implementation
- ? Comprehensive test suite

### In Progress
- ? Rename `dbContext` ? `MestrDbContext`
- ? Unit of Work pattern
- ? Logging framework
- ? Service layer tests

### Planned
- ?? i18n support (English)
- ?? Dark theme
- ?? Advanced reporting
- ?? Data export (CSV, Excel)
- ?? Backup/restore functionality

---

## ?? Contributing

Contributions are welcome! This is an educational project for DTU (Technical University of Denmark).

### Development Setup

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Guidelines

- Follow existing code style
- Add tests for new features
- Update documentation
- Use meaningful commit messages
- Ensure all tests pass before PR

---

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## ?? Team

**DTU Gruppe 7** - Technical University of Denmark

---

## ?? Documentation

Additional documentation available:
- `CONFIGUREAWAIT_FIX_SUMMARY.md` - Async/await implementation
- `NULLABLE_REFERENCE_FIXES_SUMMARY.md` - Nullable types
- `CLIENT_MODEL_VALIDATION_SUMMARY.md` - Validation pattern
- `CONSTANTS_REFACTORING_SUMMARY.md` - Constants organization
- `REMAINING_ISSUES_ACTION_PLAN.md` - Future roadmap

---

## ?? Acknowledgments

- **QuestPDF** for PDF generation
- **Entity Framework Core** team
- **Microsoft** for .NET and WPF
- **DTU** for project support

---

## ?? Contact

For questions or support, please open an issue on GitHub.

---

<div align="center">

**Built with ?? by DTU Gruppe 7**

? Star this repo if you find it helpful!

</div>