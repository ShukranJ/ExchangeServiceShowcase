# Currency Exchange Office System

**Course name:** Network Application Development  
**Project title:** Currency Exchange Office System  
**Author:** Shukran Jabrayilov
**Student ID:** 68089

## Project Description

This project is a network-based currency exchange office system developed on the .NET platform.  
It allows the user to create an account, top up a virtual PLN balance, check current and historical exchange rates, buy and sell currencies, and store balances and transaction history in a database.

The system integrates with the National Bank of Poland API to retrieve real exchange rate data.

## Main Features

- User account creation
- Virtual PLN balance top-up
- Current exchange rates from the NBP API
- Historical exchange rates
- Currency buying and selling
- Transaction history
- Currency balance management
- Database persistence using SQLite

## Technologies Used

- .NET
- C#
- ASP.NET Core MVC
- Entity Framework Core
- SQLite
- HttpClient
- NBP API
- Razor Views
- HTML
- CSS

## How to Run the Project

1. Open the project folder in Visual Studio Code or another .NET-compatible IDE.
2. Open the terminal in the project directory.
3. Restore dependencies:

## Run

```bash
cd ExchangeServiceShowcase
dotnet restore ExchangeServiceShowcase.sln
dotnet run --project ExchangeServiceShowcase.csproj

Open:

- `http://localhost:5068`
- `http://localhost:5068/api/rates/USD`
- `http://localhost:5068/api/rates/history/USD?days=7`

## Features

- account creation
- PLN top-up
- live NBP rates
- historical rate table
- buy currency with PLN
- sell currency back to PLN
- SQLite storage for accounts, balances, and transactions

## Structure

Inside `ExchangeServiceShowcase` the project includes:

/ExchangeServiceShowcase
│
├── Controllers
├── Models
├── Data
├── Services
├── Views
├── wwwroot
├── Documentation
│   └── Project-Documentation.md
└── README.md
