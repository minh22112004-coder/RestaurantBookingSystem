# RESTAURANT BOOKING SYSTEM

## ABSTRACT

The Restaurant Booking System is a web application that allows customers to browse restaurants, view menus, and reserve tables online. Administrators can manage restaurants, tables, menu items, reservations, and view basic business statistics. The project uses ASP.NET Core, Razor Views, Entity Framework Core, and Microsoft SQL Server.

## ACKNOWLEDGEMENT

We would like to thank our instructor for the guidance and feedback provided during this project. We also thank every team member for contributing to the analysis, development, testing, and documentation of the system.

## TABLE OF CONTENTS

1. Overview
2. Technologies
3. Design and Analysis
4. Implementation
5. Conclusion and Future Works
6. References

## LIST OF FIGURES

- Figure 3.1. System Use Case Diagram
- Figure 3.2. Database Diagram
- Figure 3.3. System Architecture
- Figure 4.1. Customer Restaurant Page
- Figure 4.2. Reservation Page
- Figure 4.3. Admin Dashboard

## LIST OF TABLES

- Table 1.1. Main System Features
- Table 3.1. Main Database Entities
- Table 4.1. Feature Implementation Summary
- Table 4.2. Automated Test Results

## LIST OF ABBREVIATIONS

| Abbreviation | Meaning |
|---|---|
| API | Application Programming Interface |
| CRUD | Create, Read, Update, Delete |
| DTO | Data Transfer Object |
| EF Core | Entity Framework Core |
| JWT | JSON Web Token |
| MVC | Model-View-Controller |
| SQL | Structured Query Language |
| UI | User Interface |

# CHAPTER 1. OVERVIEW

## 1.1. Introduction

Restaurant reservations are often handled by phone or direct communication. This process can be inconvenient for customers and difficult for restaurant staff to track. The Restaurant Booking System provides a central website for online reservations and restaurant management.

## 1.2. Objectives

- Allow customers to register, sign in, and reserve tables online.
- Prevent invalid or overlapping reservations.
- Allow customers to manage their reservations and notifications.
- Allow administrators to manage restaurant information from one system.
- Provide basic statistics for business monitoring.

## 1.3. System Features and Architecture

The system contains six main features:

1. Authentication and Authorization
2. Restaurant and Table Management
3. Reservation Management
4. Menu Management
5. Notifications
6. Dashboard and Reports

The system uses a layered architecture. The ASP.NET Core MVC application provides the user interface, the Web API processes business rules, Entity Framework Core manages data access, and SQL Server stores application data.

## 1.4. Workflow and Practical Value

A customer selects a restaurant, checks its tables and menu, signs in, and creates a reservation. The system validates the selected time, guest count, and table availability. Administrators can then review reservations and manage restaurant information. This workflow reduces manual work and improves reservation accuracy.

# CHAPTER 2. TECHNOLOGIES

## 2.1. C#

C# is the main programming language used for the backend, frontend controllers, business logic, and automated tests.

## 2.2. ASP.NET Core Web API

The Web API provides endpoints for authentication, restaurants, tables, menus, reservations, notifications, and reports.

## 2.3. ASP.NET Core MVC

MVC organizes the frontend into models, views, and controllers. It provides clear routing and separation between UI and application logic.

## 2.4. Razor Views

Razor Views use `.cshtml` files to generate the user interface. They combine HTML with C# view models and tag helpers.

## 2.5. Entity Framework Core

Entity Framework Core connects the API to SQL Server and supports queries, relationships, and data updates.

## 2.6. Microsoft SQL Server

SQL Server stores users, roles, restaurants, tables, menu items, reservations, notifications, orders, and related data.

## 2.7. HTML, CSS, and JavaScript

HTML defines page content, CSS creates the desktop interface, and JavaScript supports client-side interaction and validation.

## 2.8. JWT Authentication

JWT access tokens are used to authenticate API requests and identify user roles securely.

## 2.9. xUnit

xUnit is used to test backend and frontend behavior, including authentication, authorization, reservations, notifications, and Admin pages.

## 2.10. Git and GitHub

Git supports version control and team collaboration. GitHub can be used to store and share the project repository.

# CHAPTER 3. DESIGN AND ANALYSIS

## 3.1. Use Case Diagram

The system has two main actors:

- Customer: register, sign in, browse restaurants, view menus, reserve tables, manage reservations, and view notifications.
- Administrator: sign in, manage restaurants, tables, menus, reservations, and view dashboard reports.

The final report can insert a use case diagram showing these relationships.

## 3.2. Database Diagram

The database diagram describes relationships between User, Role, Restaurant, DiningTable, Category, MenuItem, Reservation, Notification, Order, and OrderItem entities. Restaurant data is connected to tables and menu items, while customer data is connected to reservations and notifications.

## 3.3. Database Architecture

SQL Server is the main data store. Entity Framework Core maps database tables to C# entities. Service and repository classes handle database operations so controllers do not directly contain complex data-access logic.

## 3.4. Entity Schemas

| Entity | Purpose |
|---|---|
| User and Role | Store accounts and access permissions |
| Restaurant | Store restaurant information and opening hours |
| DiningTable | Store table number, capacity, and status |
| Category and MenuItem | Store restaurant menu information |
| Reservation | Store booking date, time, table, and guest count |
| Notification | Store reservation messages for customers |
| Order and OrderItem | Store order and payment-related information |

# CHAPTER 4. IMPLEMENTATION

## 4.1. Authentication and Authorization

Customers can create accounts and sign in. Passwords are stored using secure password hashing, while JWT tokens authorize API requests. Customer and Admin routes use role-based access control. A default `admin / 123456` account is seeded for demonstration.

## 4.2. Customer Interfaces

The customer interface includes the Home page, restaurant list, restaurant details, menu, reservation form, My Reservations, notifications, profile, login, and registration pages.

## 4.3. Reservation Interface

Customers choose a table, reservation date, start time, end time, and guest count. The system checks table capacity and overlapping time periods before saving a reservation. Customers can later update or cancel the booking.

## 4.4. Admin Interface

The Admin area contains a simple dashboard and management pages for restaurants, tables, menu categories, menu items, and reservations. All Admin pages require the Admin role.

## 4.5. Key Features Summary

| Feature | Implementation Result |
|---|---|
| Authentication and Authorization | Completed |
| Restaurant and Table Management | Completed |
| Reservation Management | Completed |
| Menu Management | Completed |
| Notifications | Completed |
| Dashboard and Reports | Completed |

Automated tests cover the main Customer and Admin workflows. The current test result is 72 passed tests with no failures.

# CHAPTER 5. CONCLUSION AND FUTURE WORKS

## 5.1. Conclusion

The project successfully implements the main workflow of an online restaurant booking system. Customers can reserve tables conveniently, while administrators can manage restaurant information and review basic statistics. The system demonstrates API development, MVC frontend design, database integration, security, and automated testing.

## 5.2. Project Limitations

- The interface is designed primarily for desktop use.
- Notifications are displayed inside the application only.
- Online payment is not currently implemented.
- Dashboard reports are intentionally simple.
- The application currently runs in a local development environment.

## 5.3. Future Works

- Improve mobile and tablet responsiveness.
- Add email and SMS notifications.
- Integrate online payment services.
- Add customer reviews and restaurant ratings.
- Add more advanced reports and data visualization.
- Deploy the application to a cloud platform.

# REFERENCES

1. Microsoft, "ASP.NET Core Documentation," https://learn.microsoft.com/aspnet/core/
2. Microsoft, "Entity Framework Core Documentation," https://learn.microsoft.com/ef/core/
3. Microsoft, "SQL Server Documentation," https://learn.microsoft.com/sql/sql-server/
4. Microsoft, ".NET and C# Documentation," https://learn.microsoft.com/dotnet/
5. xUnit.net, "xUnit Documentation," https://xunit.net/
