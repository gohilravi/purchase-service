# Purchase Service Tests

This project contains XUnit test cases for the Purchase Service API.

## Test Coverage

The test suite covers:

### Services
- **PurchaseServiceTests**: Tests for purchase creation and status update operations
  - Valid purchase creation
  - Buyer validation
  - Status type validation
  - Status updates with case-insensitive matching
  - Error handling scenarios

### Controllers
- **PurchaseControllerTests**: Tests for API endpoints
  - Create purchase endpoint (success and error cases)
  - Update purchase status endpoint (success and error cases)
  - Validation error handling
  - HTTP status code verification
  - API response format validation

### Validators
- **CreatePurchaseRequestValidatorTests**: Tests for purchase creation request validation
  - Valid requests
  - Invalid OfferId scenarios
  - Invalid BuyerId scenarios
  - Edge cases (zero, negative, large values)

- **UpdatePurchaseStatusRequestValidatorTests**: Tests for status update request validation
  - Valid status values (case-insensitive)
  - Invalid status values
  - Empty/null status handling
  - Error message validation

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run tests with coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Run specific test class
```bash
dotnet test --filter FullyQualifiedName~PurchaseServiceTests
```

### Run tests with detailed output
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Structure

```
purchase-service.Tests/
├── Controllers/
│   └── PurchaseControllerTests.cs
├── Services/
│   └── PurchaseServiceTests.cs
├── Validators/
│   ├── CreatePurchaseRequestValidatorTests.cs
│   └── UpdatePurchaseStatusRequestValidatorTests.cs
├── Helpers/
│   └── TestDbContextHelper.cs
└── purchase-service.Tests.csproj
```

## Test Dependencies

- **xunit**: Testing framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Fluent assertion library
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing

## Code Coverage

The following files are excluded from code coverage (marked with `[ExcludeFromCodeCoverage]`):
- Models (Buyer, Purchase, StatusType)
- DTOs (CreatePurchaseRequest, UpdatePurchaseStatusRequest, PurchaseResponse, ApiResponse)
- Data/ApplicationDbContext
- Services/IPurchaseService (interface)

These are simple data classes or infrastructure code that don't require unit testing.

