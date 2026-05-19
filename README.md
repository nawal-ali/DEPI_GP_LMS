# 🎓 TOTC LMS — Learning Management System

A full-featured Learning Management System built with **ASP.NET Core MVC (.NET 10)** as a graduation project. The system supports multiple user roles with dedicated dashboards, AI-powered features, automated parent reporting, and a complete ticketing system.

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Roles & Dashboards](#-roles--dashboards)
- [AI Features](#-ai-features)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [Database Setup](#-database-setup)
- [n8n Automation](#-n8n-automation)
- [Screenshots](#-screenshots)

---

## ✨ Features

### Core Academic Features
- 📚 **Course Management** — create, assign instructors, enroll students
- 📝 **Exam System** — MCQ + text questions, timed exams, auto-grading
- 📋 **Assignment System** — file/text submissions, instructor grading with feedback
- 📢 **Announcements** — role-targeted, pinnable, expiry dates
- 📁 **Course Materials** — file uploads per course

### User Management
- 👤 Multi-role: **SuperAdmin, Admin, Instructor, Student, Parent**
- 🔐 ASP.NET Core Identity authentication
- 📧 Welcome email on account creation (Gmail SMTP)
- 👨‍👩‍👧 Parent–child linking system
- 🎓 Grade / Stage / Subject / Sub-Subject / Term configuration

### Communication
- 🎫 **Ticketing System** — role-based, status tracking, forwarding (Admin → SuperAdmin)
- 🔔 **Notification Bell** — real-time dropdown for all roles
- 📬 **Weekly Parent Reports** — automated via n8n + Gmail

### AI Features
- 🤖 **Student AI Chatbot** — RAG-based, document Q&A (PDF/DOCX)
- 📚 **AI Study Planner** — personalized schedule from exam/assignment deadlines
- ✍️ **AI Exam Generator** (Instructor) — MCQ + True/False from uploaded materials
- 🔑 **Key Points & Summaries** — from uploaded documents
- 📊 **MCQ Practice Generator** — auto-generates from study material

### UX / UI
- 🎨 Consistent Poppins-based design across all dashboards
- 📱 Fully responsive (mobile offcanvas sidebar)
- 🔒 Exam tab-lock — prevents navigation during active exams
- ✅ Bootstrap modals instead of browser alerts/confirms
- 🏷️ Sidebar notification badges (announcements, tickets)

---

## 🛠 Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | ASP.NET Core MVC (.NET 10) |
| **ORM** | Entity Framework Core 10 |
| **Database** | SQL Server (MSSQL) |
| **AI Storage** | MongoDB Atlas |
| **Identity** | ASP.NET Core Identity |
| **AI API** | GitHub Models (OpenAI-compatible) — `gpt-4o-mini` |
| **PDF Extraction** | PdfPig |
| **DOCX Extraction** | DocumentFormat.OpenXml |
| **Email** | MailKit (Gmail SMTP) |
| **Automation** | n8n (self-hosted) |
| **UI** | Bootstrap 5.3, Font Awesome 6, Chart.js |
| **Architecture** | Repository + Unit of Work pattern |

---

## 📁 Project Structure

```
LMS/
├── LMSProject/                    # Main web app
│   ├── Areas/
│   │   ├── Admin/                 # Admin dashboard
│   │   ├── Instructor/            # Instructor dashboard
│   │   ├── Parent/                # Parent dashboard
│   │   ├── Student/               # Student dashboard
│   │   └── SuperAdmin/            # SuperAdmin dashboard
│   ├── AI/
│   │   ├── Models/                # MongoDB models (RAG)
│   │   └── Services/
│   │       ├── AiChatService.cs
│   │       ├── DocumentProcessingService.cs
│   │       ├── EmailService.cs
│   │       ├── GithubAiService.cs
│   │       ├── InstructorAiService.cs
│   │       ├── MongoDbService.cs
│   │       └── N8nAndReportService.cs
│   ├── Controllers/               # Shared controllers + API
│   ├── Services/                  # TicketService
│   ├── ViewModels/                # Shared ViewModels
│   └── Views/Shared/              # Layouts, partials, chatbot
├── MLSCore/                       # Domain models + interfaces
│   ├── Models/                    # EF Core entities
│   └── Interfaces/                # Repository interfaces
└── MLSEF/                         # EF Core infrastructure
    ├── AppDbContext.cs
    └── Repositories/
```

---

## 👥 Roles & Dashboards

| Role | Capabilities |
|---|---|
| **SuperAdmin** | Full system access, user management, course management, configuration (Grades/Stages/Subjects/Terms), system reports |
| **Admin** | User management, course monitoring, tickets, announcements, weekly reports |
| **Instructor** | Course materials, exams, assignments, AI exam generator, tickets |
| **Student** | Course access, exam taking, assignment submission, AI chatbot, tickets, parent info |
| **Parent** | Children's progress, announcements, tickets, weekly email reports |

---

## 🤖 AI Features

### Student AI Chatbot (RAG)
1. Student uploads PDF or DOCX
2. Text is extracted (PdfPig / OpenXml) and split into chunks
3. Chunks stored in MongoDB
4. On chat: keyword matching retrieves relevant chunks
5. GPT-4o-mini answers based on document context

**Supported actions:**
- Free-form Q&A about documents
- Auto-summarize document
- Generate MCQ practice questions
- Extract key points
- Generate personalized study plan from deadlines

### Instructor AI Exam Generator
- Upload course materials (PDF/DOCX/image)
- Configure MCQ count (1–20) and True/False count (0–10)
- AI generates a complete formatted exam

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (Express or full)
- [MongoDB Atlas](https://www.mongodb.com/cloud/atlas) account (free tier works)
- [GitHub Models API key](https://github.com/marketplace/models)
- Gmail account with [App Password](https://myaccount.google.com/apppasswords) enabled

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/your-username/totc-lms.git
cd totc-lms/LMS

# 2. Restore NuGet packages
dotnet restore

# 3. Copy and configure appsettings
cp LMSProject/appsettings.json LMSProject/appsettings.Development.json
# Edit appsettings.Development.json with your credentials (see Configuration)

# 4. Run the SQL setup scripts
# Execute SQL/CreateTicketTables.sql in SSMS against your database

# 5. Run the application
cd LMSProject
dotnet run
```

### First Login
Create the SuperAdmin user by running this SQL after starting the app once:

```sql
-- After first run, seed a SuperAdmin manually via registration
-- OR use the seeder in Program.cs (if implemented)
```

---

## ⚙️ Configuration

Copy `appsettings.json` to `appsettings.Development.json` and fill in:

```json
{
  "ConnectionStrings": {
    "conString": "Server=YOUR_SERVER\\SQLEXPRESS;DataBase=MLSDB;Integrated Security=True;TrustServerCertificate=True;"
  },
  "MongoDB": {
    "ConnectionString": "mongodb+srv://user:password@cluster.mongodb.net/",
    "DatabaseName": "TOTC_LMS_AI"
  },
  "AI": {
    "ApiKey": "YOUR_GITHUB_MODELS_PAT",
    "BaseUrl": "https://models.inference.ai.azure.com",
    "ChatModel": "gpt-4o-mini"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your@gmail.com",
    "Password": "your-16-char-app-password"
  },
  "N8N": {
    "WebhookBaseUrl": "http://localhost:5678",
    "WebhookSecret": "your-shared-secret"
  }
}
```

> ⚠️ **Never commit `appsettings.Development.json`** — it is excluded by `.gitignore`

---

## 🗄️ Database Setup

```bash
# The app uses EF Core with an existing SQL Server DB
# Run the ticket tables migration script first:
# SQL/CreateTicketTables.sql

# Required tables (auto-created by EF if using Code First):
# AspNetUsers, Students, Instructors, Parents, Courses,
# Tests, Assignments, Tickets, TicketReplies, ...
```

---

## 🔄 n8n Automation

The `n8n_WeeklyParentReport.json` workflow automates weekly parent reports.

### Setup
```bash
# Install n8n locally
npm install -g n8n
n8n start
# Open http://localhost:5678
```

### Import workflow
1. Open n8n → **Workflows** → **Import from file**
2. Select `n8n_WeeklyParentReport.json`
3. Configure credentials:
   - **Microsoft SQL** → your SQL Server connection
   - **SMTP** → Gmail + App Password
4. Set environment variables or hardcode URLs
5. Activate workflow

**Schedule:** Every Friday at 8:00 AM — sends each parent a styled HTML report of their child's weekly progress including exam scores, assignment status, and upcoming deadlines.

---

## 📦 NuGet Packages

| Package | Purpose |
|---|---|
| `MongoDB.Driver` 3.1.0 | RAG document storage |
| `MailKit` 4.7.1.1 | Email sending (SMTP) |
| `PdfPig` 0.1.9 | PDF text extraction |
| `DocumentFormat.OpenXml` 3.2.0 | DOCX text extraction |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Authentication |
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL Server ORM |
| `AutoMapper` | Object mapping |

---

## 🔐 Security Notes

- All AI endpoints use `[IgnoreAntiforgeryToken]` for XHR compatibility
- n8n webhook secured via shared secret header (`X-N8N-Key`)
- Students can only access their own uploaded files (userId isolation)
- Role-based authorization enforced on all controllers
- File uploads validated: type (PDF/DOCX/image) and size (≤ 20 MB)

---

## 👨‍💻 Team

**Graduation Project — TOTC LMS**

Built with ❤️ using ASP.NET Core MVC, Entity Framework Core, MongoDB, and GitHub Models AI.

---

## 📄 License

This project is for educational purposes as a graduation project.
