using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests.Services
{
    public class RegistrationServiceTests : IDisposable
    {
        private readonly SqliteConnection _conn;
        private readonly ApplicationDbContext _db;
        private readonly RegistrationService _svc;

        public RegistrationServiceTests()
        {
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_conn)
                .Options;

            _db = new ApplicationDbContext(options);
            _db.Database.EnsureCreated();

            _svc = new RegistrationService(_db);
        }

        // Verify that a registration is persisted correctly
        [Fact]
        public async Task RegisterAsync_PersistsRegistration()
        {
            var ev = new Event
            {
                Name = "Target",
                Description = "Desc",
                Location = "City",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            };
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            var req = new RegisterRequest
            {
                Name = "Alice",
                Email = "a@b.com",
                PhoneNumber = "123"
            };

            var reg = await _svc.RegisterAsync(ev.Id, req);

            Assert.True(reg.Id > 0);
            Assert.Equal(ev.Id, reg.EventId);

            var stored = await _db.Registrations.FindAsync(reg.Id);
            Assert.NotNull(stored);
            Assert.Equal("Alice", stored!.Name);
        }

        // Verify that registering for a non-existent event throws
        [Fact]
        public async Task RegisterAsync_Throws_WhenEventMissing()
        {
            var req = new RegisterRequest
            {
                Name = "Bob",
                Email = "b@b.com",
                PhoneNumber = "999"
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _svc.RegisterAsync(9999, req));
        }

        public void Dispose()
        {
            _db?.Dispose();
            _conn?.Dispose();
        }
    }
}
