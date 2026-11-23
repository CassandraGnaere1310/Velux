using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Velux.Api.Data;
using Velux.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON settings (IgnoreCycles: navigation properties point both ways, e.g. Supplier <-> MaterialAlternative)
builder.Services
    .AddControllers(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Database (SQLite file in the API folder; switch to SQL Server by changing provider + connection string)
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=velux.db"));

// Business logic
builder.Services.AddSingleton<CO2eCalculator>();
builder.Services.AddSingleton<CostCalculator>();
builder.Services.AddSingleton<TechnicalAssessor>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IComparisonService, ComparisonService>();

// Allow the Blazor client (runs on another port) to call the API
var clientOrigins = builder.Configuration.GetSection("ClientOrigins").Get<string[]>()
                    ?? ["https://localhost:7002", "http://localhost:5002"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(clientOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

// Create the database and insert illustrative test data on first run.
// (Prototype choice: EnsureCreated. Switch to EF migrations when the model is stable.)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Seed(db);
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();
