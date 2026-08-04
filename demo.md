# Restaurant Booking System - Demo Guide

## 1. Start the application

1. Open `RestaurantBookingSystem.slnx` in Visual Studio.
2. Select the `API and Web` launch profile.
3. Press `F5`.
4. Open the Web application at `http://localhost:5270`.

The backend API must also be running at `http://localhost:5170`.

## 2. Customer demo

1. Open the Home page.
2. Select **Restaurants**.
3. Open a restaurant to view its information, menu, and tables.
4. Create a new customer account or sign in.
5. Select an available table and submit a reservation.
6. Open **My Reservations** to view, update, or cancel the reservation.
7. Open **Notifications** to view reservation updates.

## 3. Admin demo

Use the default demo account:

- Username: `admin`
- Password: `123456`

1. Sign in with the Admin demo account.
2. Open the Admin Dashboard.
3. Show the statistic cards, restaurant filter, and seven-day reservation chart.
4. Open **Restaurants** to create or edit a restaurant.
5. Open **Tables** to manage dining tables.
6. Open **Menu** to manage categories and menu items.
7. Open **Reservations** to filter, update, or cancel reservations.

## 4. Suggested demo order

```text
Home
-> Restaurant Details
-> Register or Login
-> Create Reservation
-> Update Reservation
-> Notifications
-> Admin Dashboard
-> Admin Management Pages
```

## 5. Notes

- Start both the API and Web projects before the demo.
- Make sure SQL Server and `RestaurantReservationDB` are available.
- Use future dates when creating reservations.
- Use an available table with enough capacity for the guest count.
- The interface and system messages use English.
