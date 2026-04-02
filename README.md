# 📨 Twilio Message Reporting Portal

An **ASP.NET MVC** web application that integrates with the **Twilio API** to retrieve SMS message data, persist logs to **SQL Server**, and generate exportable daily reporting files — providing a centralised dashboard for monitoring and auditing SMS communications.

---

## 🚀 Features

- **Twilio API Integration** — Fetches sent/received SMS message records via the Twilio REST API
- **Message Log Storage** — Persists message data (sender, recipient, status, timestamp, body) to SQL Server for historical tracking
- **Daily Reporting** — Automated generation of daily summary reports exportable as files (CSV/Excel)
- **Dashboard View** — Web UI for browsing, filtering, and searching message logs
- **Status Tracking** — Monitor delivery status per message (delivered, failed, queued, etc.)
- **Audit Trail** — Full log history for compliance and troubleshooting purposes

---

## 🛠 Tech Stack

- **Language:** C#, JavaScript
- **Framework:** ASP.NET MVC
- **Database:** SQL Server
- **Third-Party API:** Twilio REST API (SMS)
- **Architecture:** MVC + Service Layer
- **Export:** CSV / file-based report generation

---

## 📁 Project Structure

```
twilio-message-reporting-portal-dotnet/
├── Controllers/      # MVC controllers for dashboard and reporting
├── Models/           # Data models and view models
├── Services/         # Twilio API client and message processing logic
├── Data/             # SQL Server data access layer
├── Views/            # Razor views for the web UI
├── Scripts/          # JavaScript for UI interactions
└── TwilioPortal.sln
```

> *Note: Folder names may vary — refer to the solution file for the exact project layout.*

---

## ⚙️ Getting Started

### Prerequisites

- [.NET Framework / .NET Core SDK](https://dotnet.microsoft.com/en-us/download)
- SQL Server
- A [Twilio account](https://www.twilio.com/) with Account SID and Auth Token
- Visual Studio 2019+

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/SavaliyaBhumika/twilio-message-reporting-portal-dotnet.git
   cd twilio-message-reporting-portal-dotnet
   ```

2. **Configure Twilio credentials**

   In `Web.config` or `appsettings.json`, set:
   ```json
   {
     "Twilio": {
       "AccountSid": "YOUR_ACCOUNT_SID",
       "AuthToken": "YOUR_AUTH_TOKEN",
       "FromNumber": "YOUR_TWILIO_NUMBER"
     }
   }
   ```
   > ⚠️ Never commit real credentials. Use environment variables or a secrets manager in production.

3. **Configure the database**
   - Update the connection string in `Web.config` / `appsettings.json`
   - Run any provided SQL setup scripts to create the message log tables

4. **Restore and run**
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

---

## 📊 How It Works

```
Twilio API
    ↓  (REST API call — fetch message records)
Service Layer
    ↓  (parse, validate, transform)
SQL Server
    ↓  (stored message logs)
Report Generator
    ↓  (daily aggregation + export)
MVC Dashboard
    (browse · filter · download)
```

---

## 📌 Key Design Decisions

- **Service layer abstraction** wraps the Twilio SDK so the rest of the application is not coupled to the third-party client — swapping to a different SMS provider requires changes in one place only
- **SQL Server persistence** enables historical queries and trend analysis beyond what the Twilio console provides
- **Exportable reports** allow non-technical stakeholders to consume data without direct dashboard access

---

## 👩‍💻 Author

**Bhumika Satasiya** — Senior Full Stack Engineer  
[LinkedIn](https://linkedin.com/in/bhumikasatasiya) · [GitHub](https://github.com/SavaliyaBhumika)
