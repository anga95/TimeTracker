# TimeTracker

[**Öppna appen → timetracker-anga.azurewebsites.net**](https://timetracker-anga.azurewebsites.net/)

TimeTracker är en webbaserad applikation för att registrera och följa upp arbetstid. Projektet är byggt med **ASP.NET Core 8** och **Blazor Server** och använder **Entity Framework Core** för datalagring.

---
## Tech stack

- **C# / .NET 8**
- **Blazor Server**
- **Entity Framework Core** (SQL Server)
- **ASP.NET Identity**
- **Azure**: App Service (med slot), Azure SQL, Key Vault, Application Insights, Azure OpenAI

---

## Funktioner

- **Tidregistrering** – logga timmar per dag och projekt
- **Kalender- och dagvyer** – översikt över tidigare registreringar
- **Projekthantering** – skapa och ta bort projekt
- **AI-sammanfattning** – generera en veckorapport med Azure OpenAI
- **Autentisering** – inloggning via ASP.NET Core Identity
- **Telemetri och säkerhet** – stöd för Application Insights och Azure Key Vault
- **Tickets per post** – frivillig koppling via `TicketKey` och `TicketUrl`
- **Tidsstämpling** – `LoggedAt` (UTC) sparas för varje post

---