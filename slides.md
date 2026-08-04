# Restaurant Booking System - Presentation Slides

## Slide 1 - Title

- Restaurant Booking System
- Online restaurant table reservation and management platform
- Course name
- Group name and member names
- Instructor name

## Slide 2 - Project Overview

- The system connects customers and restaurant administrators.
- Customers can find restaurants and reserve tables online.
- Administrators can manage restaurants, tables, menus, and reservations.
- The system also provides notifications and a simple dashboard.

## Slide 3 - Problem Statement

- Traditional reservations are often handled by phone or direct contact.
- Customers cannot easily check restaurant information and table availability.
- Restaurant staff may have difficulty tracking reservations and avoiding conflicts.
- Managers need a central system for managing restaurant data.

## Slide 4 - Project Objectives

- Provide a simple online table reservation process.
- Prevent overlapping reservations for the same table and time.
- Allow customers to manage their own reservations.
- Allow administrators to manage restaurant operations.
- Improve communication through reservation notifications.

## Slide 5 - Main Users

### Customer

- Register and sign in.
- Browse restaurants, menus, and tables.
- Create, update, and cancel reservations.
- View reservation notifications.

### Administrator

- View the dashboard.
- Manage restaurants, tables, and menus.
- View and update customer reservations.

## Slide 6 - Main Features

1. Authentication and Authorization
2. Restaurant and Table Management
3. Reservation Management
4. Menu Management
5. Notifications
6. Dashboard and Reports

## Slide 7 - Authentication and Authorization

- Customer account registration and login.
- Secure password hashing.
- JWT authentication between the Web application and API.
- Role-based authorization for Customer and Admin pages.
- Default demo Admin account: `admin / 123456`.

## Slide 8 - Restaurant, Table, and Menu Management

- Administrators can create, update, and delete restaurants.
- Administrators can manage table numbers, capacity, and status.
- Administrators can manage menu categories and menu items.
- Customers can view restaurant information and available menu items.

## Slide 9 - Reservation Management

- Customers select a restaurant, table, date, time, and guest count.
- The system validates table capacity and reservation time.
- The system prevents overlapping reservations.
- Customers can view, update, or cancel their reservations.
- Administrators can review and manage all reservations.

## Slide 10 - Notifications and Dashboard

### Notifications

- Notify customers when a reservation is created.
- Notify customers when reservation information changes.
- Notify customers when a reservation is cancelled.

### Dashboard

- Display reservation statistics.
- Display revenue and customer information.
- Show a seven-day reservation chart.
- Filter dashboard data by restaurant.

## Slide 11 - Technology Stack

- Backend: ASP.NET Core Web API
- Frontend: ASP.NET Core MVC with Razor Views (`.cshtml`)
- Language: C#
- Database: Microsoft SQL Server
- Data access: Entity Framework Core
- Authentication: JWT and password hashing
- Testing: xUnit
- UI technologies: HTML, CSS, and JavaScript

## Slide 12 - System Architecture

```text
Customer or Admin
        |
ASP.NET Core MVC Web Application
        |
ASP.NET Core Web API
        |
Entity Framework Core
        |
Microsoft SQL Server
```

- The MVC application provides the user interface.
- The Web API handles business logic and authorization.
- Entity Framework Core communicates with the database.

## Slide 13 - Validation and Security

- Passwords are stored as hashes, not plain text.
- Admin APIs and pages require the Admin role.
- Customers can only access their own reservations and notifications.
- Reservation time, guest count, and table capacity are validated.
- Invalid and unavailable backend requests show user-friendly messages.

## Slide 14 - Testing (optional)

- Backend integration tests cover authentication and the main APIs.
- Frontend tests cover login, registration, customer pages, and Admin pages.
- Tests verify authorization, reservation conflicts, notifications, and dashboard access.
- Current result: 72 tests passed.

## Slide 15 - Demonstration Flow (Demo)

1. Open the Home page and restaurant list.
2. View restaurant details, tables, and menu items.
3. Register or sign in as a customer.
4. Create, update, and cancel a reservation.
5. View customer notifications.
6. Sign in with the Admin demo account.
7. Show the dashboard and management pages.

## Slide 16 - Results and Future Improvements

### Current Results

- All six planned features are implemented.
- The application supports Customer and Admin workflows.
- The desktop interface is ready for demonstration.

### Future Improvements

- Online payment integration.
- Email or SMS notifications.
- Mobile and tablet responsive design.
- Restaurant reviews and ratings.
- Deployment to a cloud platform.

## Slide 17 - Conclusion

- The project provides a complete restaurant reservation workflow.
- Customers can reserve tables conveniently and manage their bookings.
- Administrators can manage restaurant data from one system.
- The project demonstrates ASP.NET Core, MVC, Web API, SQL Server, security, and automated testing.

## Slide 18 - Questions and Answers

- Thank you for listening.
- Questions and answers.
