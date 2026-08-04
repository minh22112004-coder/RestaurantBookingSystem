# Front-end Plan - Restaurant Booking System

## 1. Selected technology and architecture

The front end uses:

- ASP.NET Core MVC.
- Razor Views (`.cshtml`).
- HTML, CSS, and vanilla JavaScript.
- Typed `HttpClient` services for the backend REST API.
- Server-side session storage for the JWT and authenticated user information.

React and Vue are not required for this project. MVC and Razor keep the front end in C# and provide a clear structure for CRUD operations, authentication, authorization, validation, and the Admin dashboard.

```text
Browser
  -> RestaurantBookingSystem.Web (ASP.NET Core MVC + Razor)
  -> RestaurantBookingSystem (ASP.NET Core Web API)
  -> SQL Server
```

The MVC project never accesses `DbContext` or SQL Server directly. All data operations go through the backend API.

## 2. User groups

- Anonymous visitors can browse restaurants, tables, and menus.
- Customers can register, sign in, create and manage reservations, and read notifications.
- Administrators can manage restaurants, tables, categories, menu items, reservations, and dashboard data.

## 3. Page structure

```text
Main Page
|-- Home
|-- Authentication
|   |-- Login
|   |-- Register
|   |-- Profile
|   `-- Unauthorized
|-- Restaurants
|   |-- Restaurant List
|   `-- Restaurant Details
|       |-- Restaurant information
|       |-- Opening hours
|       |-- Menu
|       |-- Tables
|       `-- Reservation form
|-- Customer Area
|   |-- My Reservations
|   |-- Edit Reservation
|   `-- Notifications
`-- Admin Area
    |-- Dashboard
    |-- Restaurant Management
    |-- Table Management
    |-- Category and Menu Management
    `-- Reservation Management
```

## 4. Main routes

| URL | Page | Access |
|---|---|---|
| `/` | Home | Public |
| `/restaurants` | Restaurant list | Public |
| `/restaurants/{id}` | Restaurant details and booking | Public/Customer |
| `/account/login` | Login | Anonymous |
| `/account/register` | Registration | Anonymous |
| `/account/profile` | Profile | Customer |
| `/reservations` | My reservations | Customer |
| `/reservations/{id}/edit` | Edit reservation | Customer |
| `/notifications` | Notifications | Customer |
| `/admin` | Dashboard | Admin |
| `/admin/restaurants` | Restaurant management | Admin |
| `/admin/tables` | Table management | Admin |
| `/admin/menu` | Category and menu management | Admin |
| `/admin/reservations` | Reservation management | Admin |

## 5. Authentication flow

1. The Razor form posts to `AccountController` in the MVC project.
2. The controller calls the backend authentication API.
3. The MVC server stores the JWT in its server-side session.
4. `ApiAuthenticationHandler` adds the bearer token to protected API requests.
5. Admin users are redirected to `/admin`; customers are redirected to their reservation area.
6. A `401` response clears the session and returns the user to Login.
7. A `403` response opens the Unauthorized page.
8. A connection failure or timeout is mapped to `503 Service Unavailable` and shown as a readable English error.

The JWT must never be rendered into HTML or stored in browser JavaScript.

## 6. Reservation flow

1. The customer chooses a restaurant and table.
2. The customer enters a date, start time, end time, and guest count.
3. Client and server validation check the date, time range, restaurant hours, table state, and table capacity.
4. The MVC application sends the reservation request with the JWT.
5. `UserId` is not included in the request; the backend reads it from the JWT.
6. The customer can later update or cancel an active reservation.
7. Reservation changes create English notification messages.

## 7. Admin scope

### Restaurant and table management

- Create, read, update, and delete restaurants.
- Validate that closing time is later than opening time.
- Manage tables by restaurant.
- Support `Available`, `Reserved`, `Occupied`, and `Maintenance` table states.
- Show a readable message for `409 Conflict` when related data prevents deletion.

### Menu management

- Create, update, and delete categories.
- Create, update, and delete menu items.
- Filter menu items by restaurant.
- Validate restaurant, category, price, and availability.
- Prevent deletion when a category or menu item is already in use.

### Reservation management

- View reservations by date.
- Filter reservations by restaurant and status.
- Update or cancel reservations.
- Confirm/Reject is not implemented because the backend does not provide a dedicated status endpoint.

### Dashboard

- Today revenue.
- Today reservations.
- Pending, Confirmed, and Cancelled counts.
- Total and occupied tables.
- Table occupancy percentage.
- Total and new customers.
- Restaurant filter.
- Seven-day reservation trend and summary table.
- No Excel/PDF export or advanced reports.

## 8. Shared quality rules

- All user-facing text, API messages, validation messages, comments, tests, and documentation use English ASCII-safe text.
- All data-changing forms use anti-forgery tokens.
- Controllers coordinate requests; they do not contain database access.
- Razor Views render ViewModels and never call the database.
- API clients own HTTP and JSON behavior.
- Important validation remains enforced by the backend.
- Loading, empty, error, and confirmation states must be available where relevant.
- Keyboard focus, form labels, navigation state, and dialog labels must be accessible.
- The required delivery target is desktop only.
- Backend Models and the database schema must not be changed for front-end work.

## 9. Implementation checklist

Legend:

- `[ ]`: Not completed.
- `[x]`: Completed and verified.

### Phase 1 - Foundation

- [x] Create the `RestaurantBookingSystem.Web` ASP.NET Core MVC project.
- [x] Add the Web and Web.Tests projects to the solution.
- [x] Configure MVC, static files, session, and backend URL.
- [x] Create public, customer, and Admin layouts.
- [x] Create role-aware navigation.
- [x] Create typed API clients and the JWT session handler.
- [x] Add shared API error handling.
- [x] Build and run foundation tests.

Result: build passed with 0 warnings and 0 errors.

### Phase 2 - Authentication

- [x] Implement Login and Registration pages.
- [x] Implement Logout and session clearing.
- [x] Store the JWT in the server-side session.
- [x] Add the JWT to backend requests automatically.
- [x] Protect Customer and Admin routes by role.
- [x] Handle `401`, `403`, connection failure, and timeout responses.
- [x] Add authentication tests.

### Phase 3 - Public pages

- [x] Complete the Home page.
- [x] Complete Restaurant list and details pages.
- [x] Show restaurant hours, menu items, and tables.
- [x] Add search, loading, empty, and error states.
- [x] Add public page tests.

### Phase 4 - Customer features

- [x] Add the reservation form to Restaurant details.
- [x] Validate date, time, guest count, opening hours, and capacity.
- [x] Create reservations without sending `UserId`.
- [x] Complete My Reservations and Edit Reservation.
- [x] Add confirmation before cancellation.
- [x] Prevent updates to cancelled reservations.
- [x] Complete Notifications and unread count.
- [x] Add customer feature tests.

### Phase 5 - Admin management

- [x] Complete the Admin sidebar and layout.
- [x] Complete Restaurant CRUD.
- [x] Complete DiningTable CRUD by restaurant.
- [x] Complete Category CRUD.
- [x] Complete MenuItem CRUD.
- [x] Handle `409 Conflict` for data that is in use.
- [x] Complete reservation management and filters.
- [x] Require the Admin role for every Admin controller.
- [x] Add Admin management tests.

### Phase 6 - Dashboard

- [x] Create a simple Admin Dashboard.
- [x] Show revenue, reservation, customer, and table cards.
- [x] Show reservation counts by status.
- [x] Add the restaurant filter.
- [x] Add the seven-day reservation chart and data table.
- [x] Add loading, empty, and error states.
- [x] Restrict Dashboard access to Admin users.
- [x] Keep export and advanced reporting out of scope.
- [x] Add Dashboard tests.

### Phase 7 - Quality

- [ ] Complete final visual verification on desktop.
- [ ] Verify `1366x768` and `1920x1080` desktop resolutions.
- [x] Keep tablet and mobile optimization out of scope.
- [x] Verify form, dialog, and navigation accessibility.
- [x] Standardize loading, empty, and error states.
- [x] Verify that JWT and sensitive session data are not exposed in HTML.
- [x] Add ViewModel validation tests.
- [x] Add controller tests with fake API clients.
- [x] Add route and role integration tests.
- [x] Add end-to-end authentication tests.
- [x] Add end-to-end reservation tests.
- [x] Add end-to-end Admin tests.
- [x] Run the full build and all automated tests.
- [ ] Complete the final manual visual review before delivery.

Current automated result (2026-08-03): the solution builds with 0 warnings and 0 errors; 64/64 frontend tests and 7/7 backend tests pass. Manual desktop visual checks remain open because the current environment does not provide a browser session.

## 10. Completion criteria

- All delivered UI and messages are English and ASCII-safe.
- Public, Customer, and Admin routes enforce the correct access rules.
- JWT values do not appear in HTML or browser logs.
- Forms show readable validation and API errors.
- Important user and Admin journeys have automated coverage.
- The MVC application never accesses the database directly.
- Backend Models and the database schema remain unchanged.
- The API and Web projects can be started together with the `API and Web` solution launch profile.
