# Rent a Car Online Booking & Administrative Management System 🚗

# Still working on it... Soon will be finished

## 📋 Overview

The **Rent a Car Online Booking & Administrative Management System** is designed to modernize car rental operations for small-to-mid-size businesses in Lebanon. It provides a comprehensive platform for online booking, customer and employee management, real-time car availability tracking, and financial reporting.

## 🎯 Objectives

- Replace paper/excel-based management.
- Eliminate double-booking errors.
- Improve customer experience with self-service booking.
- Enable admins to oversee operations and generate reports.

## 👥 User Roles

| Role      | Capabilities |
|-----------|--------------|
| **Customer** | Browse cars, book & pay online, view booking history |
| **Employee** | Verify customers, manage bookings and car availability |
| **Admin** | Manage users, cars, pricing, reports, blacklist, and discounts |

## 🔧 Key Features

### Customer Portal
- Filter cars by type/date/features
- Secure car reservations and credit card payments
- Booking history tracking
- Upcoming: unified travel booking experience (cars + hotels/flights powered by Booking.com)

### Employee Panel
- In-person customer booking
- Car availability updates
- User verification via document checks

### Admin Dashboard
- User & car management (add/edit/delete)
- Financial and usage reports
- Promotions and blacklisting

## ⚙️ System Architecture

- **Presentation Layer**: HTML, CSS (with modern UI in `login-style.css` and `register-styles.css`), Bootstrap
- **Business Logic Layer**: ASP.NET Core 8.0
- **Data Access Layer**: Entity Framework Core + SQL Server
- **Infrastructure Layer**: API integrations (e.g., Payment Gateway)

## 🗂️ Database Design

- **Users, Roles, Customers, Employees**
- **Cars, Categories, Bookings**
- **ContactInfo, Payments, CreditCards**
- **Promocodes, Blacklist**

Each table follows best practices with normalization, relationship integrity, and clear separation of concerns.

## 📊 Reports and Automation

- Real-time car availability status
- Revenue and booking analytics
- Exportable reports (PDF)

## 🚧 Challenges and Risk Management

- **Data Security**: Encryption for sensitive data
- **Payment Gateway**: Use of real and secure gateway APIs
- **Work Distribution**: Tasks split across team by features and roles

## 👨‍💻 Team Members & Responsibilities

| Name               | Responsibilities |
|--------------------|------------------|
| Mohamad Al Ayoubi | Team Lead, Full-stack |
| Mohamad AL Jawhari | Infrastructure|
| **Mahmoud Al Natour** | Backend, Database |


## 📁 Project Structure:
### 📁 The project followes clean layered structure design pattern - MVC

- RentACar/
  - RentACar.Application/
    - Dependencies/
    - DTOs/
    - Managers/
  - RentACar.Core/
    - Dependencies/
    - Entities/
    - Repositories/
  - RentACar.Infrastructure/
    - Dependencies/
    - Data/
    - Migrations/
    - scaffold command.txt
  - RentACar.Web/
    - Connected Services/
    - Dependencies/
    - Properties/
    - wwwroot/
    - Areas/
    - Controllers/
    - Models/
    - Views/
    - appsettings.json
    - Program.cs

yaml
Copy
Edit

## 🔐 Authentication

- Secure login & role-based access
- Forgot password with email recovery
- Customer document verification

## 💳 Payments

- Add & manage credit cards
- Secure transactions
- Payment status tracking

## 📈 Reports

- View car utilization, earnings, and booking trends
- Admin-only exportable reports

---

## 📌 Notes

This system is built with scalability in mind, making it suitable for Final Year Projects (FYPs) and future enhancements like multi-branch support and dynamic pricing models.

For details on the planned Booking.com hotel and flight integration, see [`docs/booking_api_integration_plan.md`](docs/booking_api_integration_plan.md).

---

## 🔗 License

MIT License or Custom License can be added here.
