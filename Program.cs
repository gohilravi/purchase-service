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
        ?? "Host=buyer.cbses6ayg1ge.ap-south-1.rds.amazonaws.com;Database=TransportServiceDb;Username=postgres;Password=D6(Nwd_1V*-=;Trust Server Certificate=true;";
    options.UseNpgsql(connectionString);
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

// No EnsureCreated or seeding logic here for DB-first approach

app.Run();
