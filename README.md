# RemindPe - VyaparKhata & WhatsApp Reminders 📱💼

[![Live Demo](https://img.shields.io/badge/Live%20Demo-GitHub%20Pages-2563eb?style=for-the-badge&logo=github)](https://riteshparmar15.github.io/Remindpe/)
[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512bd4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![TailwindCSS](https://img.shields.io/badge/TailwindCSS-v3-38bdf8?style=for-the-badge&logo=tailwindcss)](https://tailwindcss.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](LICENSE)

> 🚀 **Live Demo on GitHub Pages**: [https://riteshparmar15.github.io/Remindpe/](https://riteshparmar15.github.io/Remindpe/)
> 
> 📦 **GitHub Repository**: [https://github.com/Riteshparmar15/Remindpe](https://github.com/Riteshparmar15/Remindpe)

A lightweight, enterprise-grade micro-SaaS web application tailored for Indian small businesses and local merchants to manage customer credit (*udhar*), payment settlements (*jama*), and one-click WhatsApp payment reminders.

Architected with **Clean Coding**, **SOLID Principles**, **Repository & Service Pattern**, and a **Mobile-First Professional UI**.

---

## 🏛️ SOLID Architecture Breakdown

1. **Single Responsibility Principle (SRP)**:
   - **Controllers**: Thin controllers strictly focused on HTTP routing, request binding, and returning status codes.
   - **CustomerService**: Handles customer business logic, Indian phone number normalization, and net balance calculation.
   - **TransactionService**: Manages transaction validation, credit/debit entries, customer existence checks, and deletion.
   - **WhatsAppReminderService**: Dedicated service for reminder message generation, multi-template localization (Standard English, Polite Hinglish, Formal Store), and URL encoding.
   - **DashboardService**: Aggregates business financial metrics (receivables, cash collected, active customer counts).
   - **Repositories**: Abstract all direct EF Core SQLite database queries and persistence.

2. **Open/Closed Principle (OCP)**:
   - Reminder and notification systems are abstracted behind `IWhatsAppReminderService`. Swapping or adding SMS gateways, WhatsApp Cloud API, or Email providers can be done without modifying controllers or customer services.

3. **Liskov Substitution Principle (LSP)**:
   - Service and repository contracts (`ICustomerRepository`, `ITransactionService`, etc.) guarantee clear invariants and return types, easily substitutable with unit test mocks or alternative database engines.

4. **Interface Segregation Principle (ISP)**:
   - Granular interfaces (`ICustomerService`, `ITransactionService`, `IWhatsAppReminderService`, `IDashboardService`) ensure classes only depend on the methods they require.

5. **Dependency Inversion Principle (DIP)**:
   - High-level modules (Controllers and Services) depend entirely on abstractions/interfaces registered in ASP.NET Core's Dependency Injection container.

---

## 📁 Project Architecture

```
RemindPe/
├── Common/
│   ├── ApiResponse.cs            # Standardized API response envelope
│   └── Exceptions.cs             # Domain exceptions (NotFoundException, ValidationException)
├── Controllers/
│   ├── CustomersController.cs    # Thin REST controller for customer accounts
│   ├── TransactionsController.cs # Thin REST controller for ledger entries
│   └── RemindersController.cs    # Dedicated controller for WhatsApp reminders & templates
├── Data/
│   ├── KhataDbContext.cs         # EF Core DbContext with SQLite configurations
│   └── DbInitializer.cs          # Seed data with authentic Indian merchant accounts
├── Middleware/
│   └── GlobalExceptionMiddleware.cs # Centralized exception handling
├── Models/
│   ├── Customer.cs               # Customer entity (Id, Name, Phone, CreatedAt)
│   ├── Transaction.cs            # Transaction entity (Id, CustomerId, Amount, Type, Note, Date)
│   └── DTOs.cs                   # Strongly-typed request/response records
├── Repositories/
│   ├── ICustomerRepository.cs    # Customer data access abstraction
│   ├── CustomerRepository.cs     # Customer EF Core implementation
│   ├── ITransactionRepository.cs # Transaction data access abstraction
│   └── TransactionRepository.cs  # Transaction EF Core implementation
├── Services/
│   ├── ICustomerService.cs       # Customer business logic contract
│   ├── CustomerService.cs        # Customer service implementation
│   ├── ITransactionService.cs    # Transaction business logic contract
│   ├── TransactionService.cs     # Transaction service implementation
│   ├── IWhatsAppReminderService.cs # Reminder generation contract
│   ├── WhatsAppReminderService.cs  # Reminder service implementation
│   ├── IDashboardService.cs      # Metric aggregation contract
│   └── DashboardService.cs       # Dashboard service implementation
├── wwwroot/
│   └── index.html                # Professional Tailwind UI & Fetch API single-page dashboard
├── Program.cs                    # DI setup, CORS, SQLite, Swagger, Static Files, Middleware
├── appsettings.json              # Connection string: Data Source=khata.db
├── khata.db                      # SQLite database file (auto-generated on first run)
└── RemindPe.csproj
```

---

## ⚡ How to Run

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher.

### Steps

1. **Navigate to the project directory**:
   ```bash
   cd c:\Users\ritesh.parmar.TG\Documents\RP\Services\RemindPe
   ```

2. **Run the application**:
   ```bash
   dotnet run --launch-profile http
   ```

3. **Open the Web Application**:
   Navigate to [http://localhost:5033](http://localhost:5033) in your browser.
   - The frontend is served directly by Kestrel from `wwwroot/index.html`.

4. **Explore the Swagger API Docs**:
   Navigate to [http://localhost:5033/swagger](http://localhost:5033/swagger).

---

## 📡 API Endpoints

| HTTP Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/customers` | Get all customers with calculated net balance (`TotalCredit - TotalDebit`) |
| `GET` | `/api/customers/stats` | Get dashboard summary metrics (Total Udhar, Payments Received, etc.) |
| `GET` | `/api/customers/{id}` | Get single customer with ledger history |
| `POST` | `/api/customers` | Register a new customer (`Name`, `Phone`) |
| `DELETE` | `/api/customers/{id}` | Delete customer and their transaction history |
| `GET` | `/api/customers/{id}/transactions` | Get chronological transaction history for a customer |
| `POST` | `/api/transactions` | Add a new `Credit` (Udhar) or `Debit` (Payment) entry |
| `DELETE` | `/api/transactions/{id}` | Delete a specific transaction and recompute balances |
| `GET` | `/api/reminders/whatsapp/{customerId}` | Generate customized WhatsApp reminder with `wa.me` URL |
| `GET` | `/api/reminders/whatsapp/templates` | Retrieve available message templates |

---

## 💬 WhatsApp Reminder Message Format

The application generates standard `wa.me` links with customizable templates:
```
https://wa.me/91[Phone]?text=Hello%20[Name],%20your%20pending%20balance%20is%20%E2%82%B9[Balance].%20Please%20clear%20it%20soon.
```
- **Templates Available**:
  1. **Standard English**: `Hello [Name], your pending balance is ₹[Balance]. Please clear it soon.`
  2. **Polite Hinglish**: `Namaste [Name] ji, aapka VyaparKhata mein pending balance ₹[Balance] hai. Kripya samay par bhuqtan karein. Dhanyawad!`
  3. **Formal Store Notice**: `Dear [Name], gentle reminder from our store regarding your outstanding balance of ₹[Balance]. Kindly settle at the earliest. Thank you.`
