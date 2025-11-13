using System.Collections.Generic;
using System.Linq;
using Application.DTOs;
using Infrastructure.Persistence;
using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _conn;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                var dict = new Dictionary<string, string?>
                {
                    // Provide AdminUser & JwtSettings so Program.cs seeding/migrations succeed
                    ["AdminUser:FullName"] = "Admin User",
                    ["AdminUser:UserName"] = "admin",
                    ["AdminUser:Email"] = "admin@example.com",
                    ["AdminUser:Password"] = "admin123!",
                    ["JwtSettings:Key"] = "supersecretkey_supersecretkey",
                    ["JwtSettings:Issuer"] = "TestIssuer",
                    ["JwtSettings:Audience"] = "TestAudience"
                };
                cfg.AddInMemoryCollection(dict!);
            });

            builder.ConfigureServices(services =>
            {
                // 1) Override authentication to use our lightweight Test scheme
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                // 2) Swap DbContext to in-memory SQLite
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                _conn = new SqliteConnection("DataSource=:memory:");
                _conn.Open();

                services.AddDbContext<ApplicationDbContext>(opt =>
                    opt.UseSqlite(_conn));

                // 3) Build provider and apply migrations so Program.cs seeding logic sees EF history tables
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureDeleted();
                db.Database.Migrate();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _conn?.Dispose();
        }
    }
}
