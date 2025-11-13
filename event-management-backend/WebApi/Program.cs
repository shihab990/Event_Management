using Infrastructure.Persistence;
using Infrastructure.Repository;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Infrastructure.Security;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.HttpOverrides;
[assembly: InternalsVisibleTo("IntegrationTests")]


var builder = WebApplication.CreateBuilder(args);

// DB (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();

// Controllers & JSON
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// JWT
var jwt = builder.Configuration.GetSection("JwtSettings");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
builder.Services.AddAuthentication("Bearer").AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = key
    };
});

// CORS (Angular https://localhost:4200)
const string corsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, p =>
        p.WithOrigins("https://localhost:4200")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Event API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter token as: Bearer <your_token>"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ensure DB folder
var dataDir = Path.Combine(app.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(dataDir);

var forwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeaderOptions.KnownNetworks.Clear();
forwardedHeaderOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeaderOptions);
app.UseHttpsRedirection();
app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event API v1");
    c.RoutePrefix = "swagger"; // open at /swagger
});
app.MapControllers();

// auto-migrate + seed admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Read AdminUser section safely
    var adminSection = builder.Configuration.GetSection("AdminUser");
    var fullName = adminSection["FullName"] ?? string.Empty;
    var userName = adminSection["UserName"] ?? string.Empty;
    var email = adminSection["Email"] ?? string.Empty;
    var plainPassword = adminSection["Password"] ?? string.Empty;

    // Validate required config values
    if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(plainPassword))
    {
        throw new InvalidOperationException(
            "AdminUser configuration is missing required values (Username or Password).");
    }

    // Check if admin user already exists
    if (!db.Users.Any(u => u.UserName == userName))
    {
        var hashed = PasswordHasher.Hash(plainPassword);
        var admin = new Domain.Entities.User
        {
            FullName = fullName,
            UserName = userName,
            Email = email,
            PasswordHash = hashed
        };

        db.Users.Add(admin);
        db.SaveChanges();
    }

}

app.Run();

public partial class Program { }
