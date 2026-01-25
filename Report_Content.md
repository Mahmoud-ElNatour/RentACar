# Computer Science Final Year Project – Computing

**Student Name**: [Your Name] - [Your ID]
**Supervisor**: [Supervisor Name]

---

## Abstract

This project presents the design and implementation of "RentACar," a comprehensive web-based car rental management system. Developed using ASP.NET Core 8.0, Entity Framework Core, and SQL Server, the application streamlines the vehicle rental process for both customers and administrators. Key features include a dynamic inventory management system, role-based access control (Admin, Employee, Customer), secure online payments via Stripe, and an advanced audit logging mechanism. The system addresses the inefficiencies of manual rental operations by providing a user-friendly interface for browsing vehicles, managing bookings, and processing financial transactions. This report details the problem analysis, system architecture, database design, and key development challenges overcome during the project lifecycle.

---

## 1. Introduction

### 1.1 Background
The car rental industry relies heavily on efficient fleet and customer management. Traditional manual methods or outdated software solutions often lead to booking errors, inventory discrepancies, and poor customer experiences. With the growing demand for digital services, a robust, web-based platform is essential for modern rental agencies to compete effectively.

### 1.2 Objective
The primary objective of this project is to develop a scalable and secure Car Rental Web Application. The system aims to:
- Automate the booking and rental process.
- Provide real-time vehicle availability and detailed categorization.
- Securely handle customer data and payments.
- Offer comprehensive management tools for administrators and employees, including audit logs and reporting.

### 1.3 Scope
**In Scope:**
- **User Roles:** Customer, Employee, Administrator.
- **Vehicle Management:** CRUD operations for Cars and Categories.
- **Rental Process:** Booking workflow, availability checks, and price calculation.
- **Financials:** Stripe payment integration, saved credit cards, and invoicing.
- **Security:** Identity management, JWT/Cookie authentication, and blacklisting logic.
- **Reporting:** PDF report generation (QuestPDF) and detailed Audit Logs.

**Out of Scope:**
- Mobile application (native).
- Multi-language support (initial release).

### 1.4 Methodology
The project followed an **Agile Software Development** methodology. This allowed for iterative development, continuous feedback, and flexible adaptation to requirements. The development cycle included:
1.  Requirement Gathering (User Stories).
2.  System Design (ERD, DFD).
3.  Implementation (Clean Architecture pattern).
4.  Testing (Unit and Integration testing).

### 1.5 Limitations
- **Time Constraints:** Limited time for advanced AI-driven recommendation features.
- **Deployment:** Currently optimized for local execution with SQL Server LocalDB/Express.

---

## 2. Related Research

### 2.1 Similar Existing Products
Existing solutions like **Hertz** or **Enterprise** offer robust platforms but are often proprietary and complex. Open-source alternatives frequently lack specific auditing features or modern payment integrations required for this specific use case.

### 2.2 Project’s Added Value
"RentACar" differentiates itself by:
- **Clean Architecture:** ensuring maintainability and testability.
- **Integrated Audit Logging:** Providing a detailed history of property changes (OldValue vs. NewValue) for security and accountability.
- **Gold & Dark Theme:** A unique, premium user interface design.
- **Seamless Stripe Integration:** Native support for secure card handling and payments.

---

## 4. Problem Analysis

### 4.1 Project Description
The "RentACar" system is a B2C (Business to Customer) and B2B (internal admin) web application. It serves as a central hub for rental operations, managing the lifecycle of a vehicle rental from acquisition to customer return.

### 4.2 Business Requirements Overview
- **Functional:** Users must be able to search and filter cars. Admins must be able to approve/reject bookings. The system must calculate costs dynamically based on duration and promo codes.
- **Non-Functional:** The system must load pages in under 2 seconds. Passwords must be hashed. Data must be consistent across concurrent bookings.

### 4.3 Use Cases
**Actors:**
- **Guest:** Browse cars, Register, Login.
- **Customer:** Manage Profile, Upload Documents, Book Cars, View Rental History, Pay.
- **Employee:** Handover/Return Cars, Inspect Vehicles, View Dashboard.
- **Admin:** Manage Users (Employee/Customer), Configure Categories/Cars, View System Reports/Audit Logs, Blacklist Customers.

---

## 5. Design

### 5.1 Data Modeling

**Context Data Model:**
The system revolves around three core domains: **Users**, **Inventory** (Cars/Categories), and **Transactions** (Bookings/Payments).

**Key-Base Data Model - Entities:**
- **Car**: Represents a vehicle (Make, Model, Year, LicensePlate, DailyPrice).
- **Category**: Classifies cars (SUV, Sedan, Luxury).
- **Booking**: Links a Customer to a Car for a specific date range.
- **Customer**: Extends the base user with specific profile data (DriverLicense).
- **Payment**: Records financial transactions, linked to bookings.
- **AuditLog**: Tracks system changes by Entity, Action, and User.

### 5.2 User Interface Design
The user interface adopts a "Premium" aesthetic using a **Dark & Gold** color scheme.
- **Framework:** ASP.NET Core Razor Views.
- **Styling:** Custom CSS with Bootstrap/Tailwind utility classes.
- **Layout:** Responsive sidebar navigation for Admins; clean top-bar navigation for Customers.

---

## 6. Development

### 6.1 Technology Stack
- **Framework:** .NET 8.0 (ASP.NET Core MVC).
- **Database:** SQL Server with Entity Framework Core (Code-First Migrations).
- **Authentication:** ASP.NET Core Identity.
- **External Services:** Stripe API (Payments), QuestPDF (Reports).
- **Logging:** Serilog.

### 6.2 Key Features Implementation
**Inventory Management:**
Implemented using a Repository pattern. The `CarController` handles image uploads (stored as Byte Arrays or file paths) and status updates.

**Booking Engine:**
The `BookingsController` processes rental requests. It verifies availability by querying the `Booking` table for overlapping dates before confirming a new reservation.

**Audit Logging:**
A custom `AuditLogManager` intercepts database changes. It uses Reflection to compare `OriginalValues` and `CurrentValues` of tracked entities, storing the diff in the `AuditLogs` table as JSON.

---

## 7. Challenges

- **Concurrency Handling:** preventing double bookings for the same car. Addressed using database transaction locks and validation logic.
- **Image Handling:** Optimizing the loading of high-resolution car images. Addressed by implementing lazy loading and optimizing DTOs (Data Transfer Objects).
- **Complex UI Logic:** Implementing a dynamic "Dark Mode" consistently across all views.

---

## 8. Future Work

- **Mobile App:** diverse native apps for iOS and Android using .NET MAUI.
- **AI Recommendations:** Suggesting cars based on user history.
- **Dynamic Pricing:** Adjusting price per day based on demand/seasonality.

---

## 9. Conclusion

The "RentACar" project successfully delivers a modern, secure, and functional car rental platform. By leveraging the latest .NET technologies and observing clean architecture principles, the system is robust and scalable. It meets the core business requirements of automating rentals and securing payments, providing a solid foundation for future operational expansion.

---
