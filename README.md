# DHA Facilitation APIs

This project is a prototype backend for a facilitation and club/room booking type application. It is built using Clean Architecture with .NET 8 and includes two separate APIs, a Web API for the admin/portal side and a Mobile API for the mobile app side, both sharing the same core business logic and database.

This is a prototype. It was built to demonstrate the architecture, core workflows and main features of a similar application that is meant to go into production later with many more features, more complete validation, better security hardening and a proper testing setup.

Important note. Any images or documents that get uploaded through this application, such as room images, CNIC images or supporting documents, are random placeholder files used only for testing and demonstration. They do not represent real user data.

Another note. All secrets, connection strings, API keys, JWT signing keys and similar sensitive configuration values have been removed or replaced with blank placeholders in this repository for security purposes. You need to supply your own values before running the project.

## Solution Structure

The solution follows Clean Architecture and is organized into the following projects.

1. Domain. Contains the core entities, enums, value objects, exceptions and constants. This layer has no dependency on anything else.
2. Application. Contains the business logic organized by feature, using commands and queries with MediatR. Also contains interfaces, shared models and view models.
3. Infrastructure. Contains the implementation of interfaces defined in Application, such as the database context, identity handling, repositories and services like file storage and SMS.
4. Web. This is the admin/portal facing API. It exposes controllers for users, roles, clubs, rooms and more.
5. MobileAPI. This is a separate API project meant for the mobile application. It exposes controllers for authentication, bookings, dashboard data and the facilitation approval process.

Both Web and MobileAPI reference the same Application and Infrastructure projects, so they share the same business rules and the same database.

## Features Implemented

### Web API (admin/portal side)

1. User management. Login with JWT, registration, listing users, activating and deactivating accounts, soft deleting users, fetching a user by id.
2. Roles, modules, sub modules and permission management for controlling what each role can access.
3. Club management and club services management.
4. Room management, including room categories, residence types, room availability and uploading room images with names, descriptions and categories.
5. Announcements management.
6. Non member related actions.
7. A couple of sample To Do controllers left over from the base project template. These are not part of the actual product features.

### Mobile API (mobile app side)

1. Authentication, including login, registration, OTP verification, resending OTP, setting a password and registering as a non member with document uploads.
2. Approval process endpoints, which handle the core facilitation workflow, such as submitting a request, viewing all requests, checking process steps and tracking approval status.
3. Dashboard data endpoint.
4. Membership purpose lookups.
5. Property lookup by member.
6. Room booking, including searching rooms, viewing room details, creating a reservation, viewing all reservations for a user and checking reservation status.
7. A payment service integration used for handling payments related to bookings.

## File and Document Uploads

The application includes a file storage service used to handle uploaded images and documents. It supports the following.

1. Saving uploaded images with validation on file size, allowed file extensions and file type.
2. Saving multiple files at once, used for room image uploads.
3. Saving documents for non member registration, such as CNIC front image, CNIC back image and an optional supporting document.
4. Deleting stored files and generating public URLs to access them.

Files are currently stored on the local file system in folders organized by feature, such as a folder per room for room images or a folder for CNIC documents. As mentioned above, all files currently used for testing this prototype are random placeholder files and not real documents or photos.

## Authentication and Authorization

1. Uses ASP.NET Core Identity for user accounts and roles.
2. Uses JWT bearer tokens as the main authentication method, with configurable key, issuer, audience and token duration.
3. Includes an OTP based verification flow for mobile registration.
4. Includes a custom authorization policy used specifically for the set password step during mobile registration.
5. Most endpoints require authentication by default unless explicitly marked as allowed for anonymous access.

## Database

1. Uses Entity Framework Core 8 with SQL Server as the database provider.
2. Two separate database contexts are used. One is the main application context that also holds identity tables. The other is used for integration with a separate legacy system.
3. Dapper is also used alongside Entity Framework Core for some raw SQL and stored procedure calls.
4. Database migrations are already included in the project.

## Technologies and Packages Used

1. MediatR for handling commands and queries.
2. AutoMapper for object mapping.
3. FluentValidation for request validation.
4. Ardalis GuardClauses for guard checks.
5. API versioning support.
6. Swagger and NSwag for API documentation.
7. Azure Key Vault support for configuration secrets.
8. Testing related packages such as NUnit, Moq and FluentAssertions are referenced for future test coverage, though a dedicated test project is not part of the solution yet.

## Prerequisites

1. .NET SDK version 8.0.100 or compatible, as specified in global.json.
2. SQL Server instance for the database.
3. Values for the following configuration settings, which are left blank in this repository for security purposes and need to be filled in locally.
   * Connection strings for the main database and the secondary integration database.
   * JWT settings, including signing key, issuer and audience.
   * File storage settings, including root path, request path and public base URL.

## Project Status

This is a working prototype meant to validate the architecture and core feature set. A few things to keep in mind.

1. Some sample To Do controllers from the base project template are still present but are not part of the actual product.
2. There is currently no CI/CD pipeline configured in this repository.
3. A full production version of this application was planned with additional features (Emergency Alarm, House Registration/Derigstration, Incoming Personnel Passes Requests, Clinic/Security Patrolling View, Water Tanker/Bowser Booking, Sports Club Bookings etc), stronger validation, more complete security review and a proper automated test suite; it was implemented separately.
