# Purchase Service API

A RESTful API for managing Purchase entities built with ASP.NET Core 8.0.

## Features

- **Create Purchase**: Create new purchase records with validation
- **Update Purchase Status**: Update the status of existing purchases
- **FluentValidation**: Comprehensive request validation
- **Entity Framework Core**: Database operations with SQLite
- **Clean Architecture**: Separation of concerns with Services, Controllers, and Data layers

## API Endpoints

### 1. Create Purchase

**Endpoint:** `POST /purchases`

**Request Body:**
```json
{
  "offer_id": 123,
  "buyer_id": 456
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "purchase_id": 789
  },
  "message": "Purchase created successfully"
}
```

**Validation:**
- `offer_id` is required and must be greater than 0
- `buyer_id` is required and must be greater than 0
- `buyer_id` must exist in the Buyer table

**Behavior:**
- Validates that the buyer exists
- Creates a new Purchase record
- Sets StatusTypeId to "Assigned"
- Sets CreatedAt timestamp
- Returns the purchase_id

### 2. Update Purchase Status

**Endpoint:** `PUT /purchases/{purchaseId}/status`

**Request Body:**
```json
{
  "status": "Completed"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": null,
  "message": "Purchase status updated successfully"
}
```

**Allowed Status Values:**
- `Assigned`
- `Canceled`
- `Completed`

**Validation:**
- `purchaseId` must exist
- `status` is required and must be one of the allowed values

**Behavior:**
- Validates that the purchase exists
- Validates the status value
- Updates only StatusTypeId and LastModifiedAt
- No other Purchase fields are modified

## Database Schema

### Buyer
- `Id` (PK, int)
- `Name` (string)
- `Email` (string)
- `CreatedAt` (DateTime)

### Purchase
- `Id` (PK, int)
- `OfferId` (int)
- `BuyerId` (FK to Buyer, int)
- `StatusTypeId` (FK to StatusType, int)
- `CreatedAt` (DateTime)
- `LastModifiedAt` (DateTime)

### StatusType
- `Id` (PK, int)
- `Status` (string)
- `CreatedAt` (DateTime)
- `LastModifiedAt` (DateTime)

## Project Structure

```
purchase-service/
├── Controllers/
│   └── PurchaseController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── Buyer.cs
│   ├── Purchase.cs
│   ├── StatusType.cs
│   └── DTOs/
│       ├── CreatePurchaseRequest.cs
│       ├── UpdatePurchaseStatusRequest.cs
│       ├── PurchaseResponse.cs
│       ├── ApiResponse.cs
│       └── Validators/
│           ├── CreatePurchaseRequestValidator.cs
│           └── UpdatePurchaseStatusRequestValidator.cs
├── Services/
│   ├── IPurchaseService.cs
│   └── PurchaseService.cs
├── Program.cs
├── purchase-service.csproj
└── appsettings.json
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or Rider (optional)

### Installation

1. Restore NuGet packages:
```bash
dotnet restore
```

2. Run the application:
```bash
dotnet run
```

3. The API will be available at:
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`

4. Swagger UI (Development only):
   - `https://localhost:5001/swagger`

### Database

The application uses SQLite by default. The database file (`purchase.db`) will be created automatically on first run. Initial StatusType records (Assigned, Canceled, Completed) are seeded automatically.

## Error Responses

All error responses follow this format:

```json
{
  "success": false,
  "message": "Error message",
  "errors": ["Validation error 1", "Validation error 2"]
}
```

**HTTP Status Codes:**
- `200 OK`: Successful update
- `201 Created`: Successful creation
- `400 Bad Request`: Validation errors or invalid operation
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

## Technologies Used

- **ASP.NET Core 8.0**: Web API framework
- **Entity Framework Core 8.0**: ORM for database operations
- **SQLite**: Database (can be easily switched to SQL Server, PostgreSQL, etc.)
- **FluentValidation**: Request validation
- **Swagger/OpenAPI**: API documentation

## License

This project is part of a competition submission.
