using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using purchase_service.Data;
using purchase_service.Models.DTOs;
using purchase_service.Models.DTOs.Validators;
using purchase_service.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=purchase.db";
    options.UseSqlite(connectionString);
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddScoped<IValidator<CreatePurchaseRequest>, CreatePurchaseRequestValidator>();
builder.Services.AddScoped<IValidator<UpdatePurchaseStatusRequest>, UpdatePurchaseStatusRequestValidator>();

// Add services
builder.Services.AddScoped<IPurchaseService, PurchaseService>();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    
    // Seed initial StatusType data if needed
    if (!context.StatusTypes.Any())
    {
        var now = DateTime.UtcNow;
        context.StatusTypes.AddRange(
            new Models.StatusType { Status = "Assigned", CreatedAt = now, LastModifiedAt = now },
            new Models.StatusType { Status = "Canceled", CreatedAt = now, LastModifiedAt = now },
            new Models.StatusType { Status = "Completed", CreatedAt = now, LastModifiedAt = now }
        );
        context.SaveChanges();
    }
}

app.Run();

