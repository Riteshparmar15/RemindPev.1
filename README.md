# RemindPe - Digital Khata & Customer Payment Management SaaS 📱💼

[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![SQLite](https://img.shields.io/badge/SQLite-WAL%20Mode-blue.svg)](https://www.sqlite.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Tests](https://img.shields.io/badge/Tests-25%2F25%20xUnit%20%7C%2031%2F31%20Integration-success.svg)]()

**RemindPe** is a reliable, high-performance, mobile-first Digital Khata and Customer Payment Management SaaS tailored for Indian local merchants (Kirana & grocery stores, dairies, hardware & electrical shops, wholesalers, garment retailers, and local service providers).

"Manage customer udhar, record payments, track outstanding balances, and send payment reminders from one simple application."

---

## 🚀 Key Features

- **Double-Entry Financial Math**:
  - `Credit` (*Udhar* / Gave): Increases customer dues owed to merchant (+).
  - `Debit` (*Jama* / Received): Decreases customer dues (-).
  - `Net Balance = Total Credit - Total Debit`.
  - Atomic opening balance creation (customer + ledger entry created in one database transaction).
  - Transaction updates and balance recalculation (`PUT /api/transactions/{id}`).
- **Strict Indian Mobile Validation**:
  - Automatically normalizes inputs (e.g. `+91-9876543210`, `09876543210`) down to standard 10-digit format (`^[6-9]\d{9}$`).
  - Enforces database and service-level uniqueness before persistence.
- **WhatsApp Payment Reminders**:
  - One-click click-to-chat links (`wa.me`) with localized templates (*Hinglish*, *Formal Store*, and *Standard*).
  - Guards against sending reminders for settled or advance-balance accounts.
- **Dynamic UPI QR Code Generation**:
  - Instant on-screen payment QR code generation from validated merchant VPA and amount.
- **Live Reports & Reconciliation**:
  - Date-range financial summaries (`GET /api/reports/summary`) tracking total credit, total collections, net shift, debtors count, settled count, and advance accounts.
  - Formula-injection-sanitized CSV exports (neutralizes `=,+,-,@` triggers).
  - Clean printable statements & receipts.
- **Merchant Business Settings**:
  - Server-authoritative store profile (`GET /api/settings`, `PUT /api/settings`) for Store Name, Owner, Phone, UPI ID, Currency, and Receipt Footer.
- **Security & Hardening**:
  - `ApiKeyAuthMiddleware` protecting API endpoints.
  - Restricted CORS policy, CSP headers, nosniff, SAMEORIGIN frame guards, and X-Correlation-ID tracing.

---

## 🏛️ Architecture & Clean Design

```
Presentation (Vanilla HTML5 / Tailwind CSS / QRious in wwwroot/index.html)
   ↓
API Controllers (Thin HTTP routes: Customers, Transactions, Reminders, Reports, Settings, Dashboard)
   ↓
Domain Services (Business validation, phone normalization, financial calculations, reminder formatting)
   ↓
Repositories (ICustomerRepository, ITransactionRepository, ISettingsRepository)
   ↓
Persistence (EF Core 9 + SQLite WAL Mode with 64MB Cache & Foreign Keys)
```

### Directory Structure

```
RemindPe/
├── Common/                    # Standardized envelopes & domain exceptions
├── Controllers/               # REST API controllers
│   ├── CustomersController.cs
│   ├── TransactionsController.cs
│   ├── RemindersController.cs
│   ├── ReportsController.cs
│   ├── SettingsController.cs
│   └── DashboardController.cs
├── Data/                      # KhataDbContext & sample data seeder
├── Middleware/                # Security headers, ApiKey auth, CorrelationId, Global exceptions
├── Models/                    # Customer, Transaction, MerchantSettings, DTOs
├── Repositories/              # Repositories & Data Access Interfaces
├── Services/                  # Business logic services & interfaces
├── tests/
│   ├── RemindPe.Tests/        # xUnit test suite (25 automated unit tests)
│   └── api_security_tests.ps1 # PowerShell integration test suite (31 checks)
├── wwwroot/
│   └── index.html             # Mobile-first single-page dashboard application
├── Program.cs                 # ASP.NET Core Web API startup & SQLite WAL setup
├── appsettings.json
└── RemindPe.csproj
```

---

## ⚡ Quick Start & Local Execution

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### 1. Run the Web Application
```bash
dotnet run --launch-profile http
```
- Web Application: [http://localhost:5033](http://localhost:5033)
- Swagger API Docs: [http://localhost:5033/swagger](http://localhost:5033/swagger)
- Health Check: [http://localhost:5033/health](http://localhost:5033/health)

### 2. Run Automated Unit Tests (xUnit)
```bash
dotnet test tests/RemindPe.Tests/RemindPe.Tests.csproj
```

### 3. Run Integration & Security Test Suite
```powershell
powershell -ExecutionPolicy Bypass -File tests/api_security_tests.ps1 -ApiKey remindpe-secure-api-key-2026
```

---

## 📡 REST API Reference

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/dashboard/stats` | Aggregated receivables, today's credit/collections, customer counts |
| `GET` | `/api/customers` | Paginated customer list with calculated net balances and search |
| `POST` | `/api/customers` | Register customer with optional atomic opening balance |
| `GET` | `/api/customers/{id}` | Customer detail with chronological transaction history |
| `PUT` | `/api/customers/{id}` | Update customer name or phone number |
| `DELETE` | `/api/customers/{id}` | Cascade delete customer and transaction records |
| `GET` | `/api/transactions` | Cross-customer transaction audit ledger |
| `POST` | `/api/transactions` | Record new `Credit` (Udhar) or `Debit` (Jama) entry |
| `PUT` | `/api/transactions/{id}` | Update existing transaction and recalculate balances |
| `DELETE` | `/api/transactions/{id}` | Delete transaction entry |
| `GET` | `/api/reminders/whatsapp/{id}` | Generate localized WhatsApp reminder link (`wa.me`) |
| `GET` | `/api/reports/summary` | Live date-range ledger aggregation & reconciliation summary |
| `GET` | `/api/settings` | Get merchant profile, UPI ID, and store preferences |
| `PUT` | `/api/settings` | Update merchant profile and store preferences |

---

## 📄 License

This project is licensed under the MIT License.
