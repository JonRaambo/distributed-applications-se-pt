using System.Text;
using CarService.Api.Data;
using CarService.Api.Models;
using CarService.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CarService API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Auto-create DB + seed (no migrations needed to run)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Customers.Any())
    {
        var c1 = new Customer { FirstName = "Ivan", LastName = "Petrov", Phone = "0888123456", Email = "ivan@example.com", IsActive = true };
        var c2 = new Customer { FirstName = "Maria", LastName = "Georgieva", Phone = "0899123456", Email = "maria@example.com", IsActive = true };
        db.Customers.AddRange(c1, c2);
        db.SaveChanges();

        var v1 = new Vehicle { CustomerId = c1.Id, PlateNumber = "CA1234AB", Brand = "VW", Model = "Golf", Year = 2016, Vin = "WVWZZZ1KZGW000001", EngineVolume = 1.6m };
        var v2 = new Vehicle { CustomerId = c2.Id, PlateNumber = "CB9876AA", Brand = "Toyota", Model = "Corolla", Year = 2019, Vin = "JTDBR32E000000002", EngineVolume = 1.8m };
        db.Vehicles.AddRange(v1, v2);
        db.SaveChanges();

        var r1 = new Repair { VehicleId = v1.Id, Title = "Oil change", Description = "Engine oil + filter", Status = RepairStatus.Completed, LaborHours = 1.0m, PartsCost = 80m, StartDate = DateTime.UtcNow.AddDays(-3), EndDate = DateTime.UtcNow.AddDays(-3), IsPaid = true };
        var r2 = new Repair { VehicleId = v2.Id, Title = "Brake inspection", Description = "Front brakes", Status = RepairStatus.InProgress, LaborHours = 1.5m, PartsCost = 0m, StartDate = DateTime.UtcNow.AddDays(-1), IsPaid = false };
        db.Repairs.AddRange(r1, r2);
        db.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
