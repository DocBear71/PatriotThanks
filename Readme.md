# Patriot Thanks

> **📌 Evolution**: This is the advanced .NET II version. For the original Windows Forms version, see: [PatriotThanks-WindowsForms](https://github.com/DocBear71/PatriotThanks-WindowsForms)

A WPF desktop application designed to help veterans, active-duty military personnel, first responders, and their families discover local businesses offering special incentives and discounts.

## 📋 Project Overview

Patriot Thanks serves as a bridge between military communities and supportive local businesses, making it easy to find and utilize available discounts and benefits. This application was developed as a .NET II semester final project, demonstrating professional software engineering practices and n-tier architecture.

## ✨ Key Features

- **User Authentication**: Role-based access control supporting Guest, Member, and Admin user types
- **Business Search**: Comprehensive search functionality with database-driven filters by state and business type
- **Incentive Discovery**: Browse and search available discounts and special offers
- **Business Management**: Full CRUD operations for business profiles and locations
- **Incentive Management**: Add, edit, and manage business incentives
- **User Administration**: Account management with role assignment and access control
- **Interactive Maps**: LocationIQ integration for business location visualization
- **Customizable Preferences**: Theme switching, font size adjustment, and persistent user settings

## 🏗️ Architecture

This application follows **n-tier architecture** with clear separation of concerns:

### Solution Projects

1. **DataDomain** - Data transfer objects (DTOs) and domain models
2. **DataAccessInterfaces** - Interface definitions for data access layer
3. **DataAccessLayer** - SQL Server data access implementation using stored procedures
4. **DataAccessFakes** - Mock implementations for unit testing
5. **LogicLayerInterfaces** - Interface definitions for business logic layer
6. **LogicLayer** - Business logic implementation
7. **LogicLayerTests** - MSTest unit tests with comprehensive coverage
8. **WpfPresentation** - WPF user interface

### Key Design Principles

- **Dependency Injection**: Loose coupling through interface-based design
- **SOLID Principles**: Applied throughout the codebase
- **Test-Driven Development (TDD)**: Comprehensive unit test coverage
- **Repository Pattern**: Clean data access abstraction

## 🛠️ Technologies Used

- **C# / .NET Framework**
- **WPF (Windows Presentation Foundation)** - User interface
- **SQL Server** - Database with stored procedures
- **MSTest** - Unit testing framework
- **Emoji.Wpf** - Enhanced UI with emoji support
- **LocationIQ API** - Mapping integration

## 📊 Database

The application uses SQL Server with:
- 11+ normalized tables
- Stored procedures for all data operations
- Sample data for testing
- Referential integrity constraints
- Automated deployment script (`patriot_thanks_db.sql`)

### Core Tables
- Users (authentication and profiles)
- Business (business information)
- BusinessLocation (physical locations)
- Address (location details)
- Incentive (discount/benefit information)
- BusinessType, IncentiveType (lookup tables)
- And more...

## 🚀 Getting Started

### Prerequisites

- Visual Studio 2022 or later
- SQL Server 2019 or later
- .NET Framework 4.8 or later
- LocationIQ API key (for mapping features)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/DocBear71/PatriotThanks.git
   cd PatriotThanks
   ```

2. **Set up the database**
   - Open SQL Server Management Studio
   - Run the `patriot_thanks_db.sql` script to create the database with sample data
   - Update the connection string in `DBConnection.cs` if needed

3. **Build the solution**
   - Open `PatriotThanks.sln` in Visual Studio
   - Restore NuGet packages
   - Build the solution (Ctrl+Shift+B)

4. **Run the application**
   - Set `WpfPresentation` as the startup project
   - Press F5 to run

### Default Test Accounts

After running the database script, you can log in with:

- **Admin Account**
  - Email: admin@company.com
  - Password: Admin123!

- **Member Account**
  - Email: user@example.com
  - Password: Password1

## 🧪 Testing

The project includes comprehensive unit tests using MSTest:

```bash
# Run all tests in Visual Studio
Test > Run All Tests
```

Tests cover:
- Business logic validation
- CRUD operations
- Error handling
- Edge cases

Test coverage exceeds academic requirements, with focus on:
- Positive test cases
- Validation scenarios
- Exception handling
- Boundary conditions

## 📱 User Interface Highlights

- **Intuitive Navigation**: Tab-based interface with role-appropriate access
- **Responsive Design**: Proper input validation and error messaging
- **Professional Appearance**: Consistent styling with theme support
- **Accessibility**: Clear labels, keyboard navigation, and screen reader support

## 📈 Project Status

**Current Status**: ✅ Complete - Portfolio Ready

This project successfully demonstrates:
- ✅ Full n-tier architecture implementation
- ✅ Comprehensive CRUD functionality
- ✅ Role-based security
- ✅ Professional UI/UX
- ✅ Extensive unit test coverage
- ✅ Database normalization and stored procedures
- ✅ Dependency injection throughout
- ✅ Clean code with XML documentation

## 🎯 Future Enhancements

Potential improvements for future versions:
- Mobile companion app
- Email notifications for new incentives
- Social sharing features
- Advanced analytics dashboard
- Integration with verification services

## 📝 License

This project was developed as an academic project for .NET II coursework.

## 👤 Author

**Edward** - [Your LinkedIn] https://www.linkedin.com/in/doctor-edward/ | [Portfolio] https://docbear-ent.com

## 🙏 Acknowledgments

- **Patriot Thanks Platform**: Integration with patriotthanks.com
- **Course Instructor**: For guidance on best practices and architecture
- **Military Community**: For inspiring this meaningful project

---

**Note**: This is a student portfolio project demonstrating professional software development practices. The application showcases full-stack development capabilities, proper architecture, and production-ready code quality.