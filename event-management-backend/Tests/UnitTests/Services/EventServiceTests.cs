using System;
using System.Linq;
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
    public class EventServiceTests : IDisposable
    {
        private readonly SqliteConnection _conn;
        private readonly ApplicationDbContext _db;
        private readonly EventService _svc;

        public EventServiceTests()
        {
            // In-memory SQLite (kept alive by open connection)
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_conn)
                .Options;

            _db = new ApplicationDbContext(options);
            _db.Database.EnsureCreated(); // fast schema creation for tests

            _svc = new EventService(_db);
        }

        // Verify that initially there are no events
        [Fact]
        public async Task GetAllAsync_EmptyAtStart()
        {
            var events = await _svc.GetAllAsync();
            Assert.Empty(events);
        }

        // Create an event and verify it's persisted
        [Fact]
        public async Task CreateAsync_Persists_Event()
        {
            var req = new CreateEventRequest
            {
                Name = "Conf",
                Description = "Tech conference",
                Location = "Munich",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            var created = await _svc.CreateAsync(req);

            Assert.True(created.Id > 0);
            Assert.Equal(req.Name, created.Name);

            var all = await _svc.GetAllAsync();
            Assert.Single(all);
            Assert.Equal("Conf", all.First().Name);
        }

        // Verify that registrations are returned only for the requested event
        [Fact]
        public async Task GetRegistrationsAsync_ReturnsOnlyForRequestedEvent()
        {
            // Seed two events
            var e1 = new Event { Name = "E1", Description = "D1", Location = "L1", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1) };
            var e2 = new Event { Name = "E2", Description = "D2", Location = "L2", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1) };
            _db.Events.AddRange(e1, e2);
            await _db.SaveChangesAsync();

            // Seed registrations
            _db.Registrations.AddRange(
                new Registration { EventId = e1.Id, Name = "Alice", Email = "a@b.com", PhoneNumber = "111" },
                new Registration { EventId = e1.Id, Name = "Bob",   Email = "b@b.com", PhoneNumber = "222" },
                new Registration { EventId = e2.Id, Name = "Shihab",   Email = "c@b.com", PhoneNumber = "333" }
            );
            await _db.SaveChangesAsync();

            var regsE1 = await _svc.GetRegistrationsAsync(e1.Id);
            Assert.Equal(2, regsE1.Count());

            var regsE2 = await _svc.GetRegistrationsAsync(e2.Id);
            Assert.Single(regsE2);
        }

        // Verify that getting by ID returns null when event is missing
        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenEventMissing()
        {
            var result = await _svc.GetByIdAsync(12345);
            Assert.Null(result);
        }

        // Verify that getting by ID returns event with its registrations
        [Fact]
        public async Task GetByIdAsync_ReturnsEventWithRegistrations()
        {
            var ev = new Event
            {
                Name = "Event-1",
                Description = "D",
                Location = "City",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            };
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            var reg = new Registration
            {
                EventId = ev.Id,
                Name = "Shihab",
                Email = "a@b.com",
                PhoneNumber = "123"
            };
            _db.Registrations.Add(reg);
            await _db.SaveChangesAsync();

            var loaded = await _svc.GetByIdAsync(ev.Id);

            Assert.NotNull(loaded);
            Assert.Equal(ev.Id, loaded!.Id);
            Assert.Single(loaded.Registrations);
            Assert.Equal("Shihab", loaded.Registrations.First().Name);
        }

        // Verify that deleting an event removes it and its registrations
        [Fact]
        public async Task DeleteAsync_RemovesEventAndRegistrations()
        {
            var ev = new Event
            {
                Name = "DeleteMe",
                Description = "D",
                Location = "City",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            };
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            _db.Registrations.Add(new Registration
            {
                EventId = ev.Id,
                Name = "Bob",
                Email = "b@b.com",
                PhoneNumber = "999"
            });
            await _db.SaveChangesAsync();

            var deleted = await _svc.DeleteAsync(ev.Id);

            Assert.True(deleted);
            Assert.Empty(_db.Events);
            Assert.Empty(_db.Registrations);
        }

        // Verify that deleting a non-existent event returns false
        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            var deleted = await _svc.DeleteAsync(98765);
            Assert.False(deleted);
        }

        public void Dispose()
        {
            _db?.Dispose();
            _conn?.Dispose();
        }
    }
}
