# Restaurant Booking System - Backend

Backend project for CSW306 using ASP.NET Core Web API and Entity Framework Core.

---

# Team Workflow

## 1. Clone Repository

```bash
git clone https://github.com/<owner>/<repository>.git
```

---

## 2. Switch to `dev` branch

```bash
git checkout dev
git pull origin dev
```

---

## 3. Create your own feature branch

Important: create your branch from `dev`.

Example:

```bash
git checkout -b feature/minh-auth
```

Push the branch to GitHub:

```bash
git push -u origin feature/minh-auth
```

Branch naming convention:

```
feature/<name>-<feature>
```

Examples

```
feature/minh-auth
feature/nam-reservation
feature/huy-menu
feature/linh-restaurant
feature/phuc-notification
feature/an-dashboard
```

---

## 4. Start Coding

Only work on your own feature branch.

Do NOT commit directly to:

- master
- dev

---

## 5. Before Every Coding Session

Update your local `dev`.

```bash
git checkout dev
git pull origin dev
```

Go back to your feature branch.

```bash
git checkout feature/<your-branch>
```

Merge latest changes from `dev`.

```bash
git merge dev
```

Resolve conflicts if any.

---

## 6. Commit Changes

```bash
git add .
git commit -m "Add reservation validation"
```

Push

```bash
git push
```

---

## 7. Create Pull Request

After finishing your feature:

```
feature/your-branch
        |
        v
       dev
```

Wait for review before merging.

---

# Git Branch Strategy

```
master
    ^
    |
   PR
    |
   dev
    ^
    |
feature/*
```

- master -> Stable release
- dev -> Development branch
- feature/* -> Personal working branch

---

# Project Structure

```
RestaurantBooking.API
|-- Features
|   |-- Authentication
|   |-- Restaurant
|   |-- Reservation
|   |-- Menu
|   |-- Notification
|   `-- Dashboard
|-- Models
|-- Data
|-- Configurations
|-- Middleware
|-- Helpers
|-- Mapping
`-- Program.cs
```

---

# Coding Rules

- Use meaningful class names.
- Follow RESTful API conventions.
- Validate all request models.
- Do not commit generated files unnecessarily.
- Keep commits small and meaningful.

Good commit examples:

```
Add JWT authentication

Implement reservation service

Fix reservation validation

Add menu CRUD APIs
```

Avoid:

```
fix

update

abc

123
```

---

# Member Responsibilities

| Member | Feature |
|---------|----------|
| Minh | Authentication |
| Nam | Reservation |
| Huy | Menu |
| Linh | Restaurant & Table |
| Phuc | Notification |
| An | Dashboard |

---

# Technology

- ASP.NET Core Web API (.NET 10)
- Entity Framework Core
- SQL Server
- ASP.NET Identity
- JWT Authentication
- Swagger
- xUnit

---

Happy coding.
