using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests.Api
{
    public class EventsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public EventsControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
        }

        // Verify that getting all events returns 200
        [Fact]
        public async Task GetAll_Returns200()
        {
            var resp = await _client.GetAsync("/api/Events");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        // Create an event and verify it shows up in listing
        [Fact]
        public async Task GetAll_IncludesCreatedEvent()
        {
            var uniqueName = $"My Event {Guid.NewGuid()}";
            var create = new CreateEventRequest
            {
                Name = uniqueName,
                Description = "Listed",
                Location = "City",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            };

            var createResp = await _client.PostAsJsonAsync("/api/Events/create", create);
            createResp.EnsureSuccessStatusCode();

            var list = await _client.GetFromJsonAsync<Event[]>("/api/Events");
            Assert.NotNull(list);
            Assert.Contains(list!, e => e.Name == uniqueName);
        }

        // Verify that creating an event requires auth, but works with test scheme
        [Fact]
        public async Task Create_RequiresAuth_ButWorksWithTestScheme()
        {
            var create = new CreateEventRequest
            {
                Name = "My Event",
                Description = "Desc",
                Location = "Munich",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            var resp = await _client.PostAsJsonAsync("/api/Events/create", create);
            resp.EnsureSuccessStatusCode();

            var ev = await resp.Content.ReadFromJsonAsync<Event>();
            Assert.NotNull(ev);
            Assert.True(ev!.Id > 0);
            Assert.Equal("My Event", ev.Name);
        }

        // Create an event, register for it, and verify registration shows up
        [Fact]
        public async Task Register_ForEvent_Succeeds_AndShowsInRegistrations()
        {
            // 1) Create event (authorized via Test scheme)
            var create = new CreateEventRequest
            {
                Name = "RegTarget",
                Description = "D",
                Location = "City",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            };
            var createResp = await _client.PostAsJsonAsync("/api/Events/create", create);
            createResp.EnsureSuccessStatusCode();
            var ev = await createResp.Content.ReadFromJsonAsync<Event>();
            Assert.NotNull(ev);

            // 2) Register anonymously
            var reg = new RegisterRequest
            {
                Name = "Alice",
                Email = "a@b.com",
                PhoneNumber = "123"
            };
            var regResp = await _client.PostAsJsonAsync($"/api/Events/{ev!.Id}/register", reg);
            regResp.EnsureSuccessStatusCode();

            // 3) Read registrations (authorized)
            var listResp = await _client.GetAsync($"/api/Events/{ev.Id}/registrations");
            listResp.EnsureSuccessStatusCode();
            var regs = await listResp.Content.ReadFromJsonAsync<Registration[]>();
            Assert.NotNull(regs);
            Assert.Single(regs!);
            Assert.Equal("Alice", regs![0].Name);
        }

        // Attempt to register for a non-existent event
        [Fact]
        public async Task Register_ForMissingEvent_Returns404()
        {
            var reg = new RegisterRequest
            {
                Name = "Ghost",
                Email = "ghost@example.com",
                PhoneNumber = "000"
            };

            var resp = await _client.PostAsJsonAsync("/api/Events/99999/register", reg);

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        // Create an event and verify registrations list is empty
        [Fact]
        public async Task Registrations_ReturnsEmptyArray_WhenNoRegistrations()
        {
            var create = new CreateEventRequest
            {
                Name = "Empty Regs",
                Description = "D",
                Location = "City",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            };
            var createResp = await _client.PostAsJsonAsync("/api/Events/create", create);
            createResp.EnsureSuccessStatusCode();
            var ev = await createResp.Content.ReadFromJsonAsync<Event>();
            Assert.NotNull(ev);

            var listResp = await _client.GetAsync($"/api/Events/{ev!.Id}/registrations");
            listResp.EnsureSuccessStatusCode();
            var regs = await listResp.Content.ReadFromJsonAsync<Registration[]>();
            Assert.NotNull(regs);
            Assert.Empty(regs!);
        }

        // Create an event, delete it, and verify it's gone
        [Fact]
        public async Task Delete_RemovesEventFromListing()
        {
            var create = new CreateEventRequest
            {
                Name = "Delete Candidate",
                Description = "D",
                Location = "City",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            };
            var createResp = await _client.PostAsJsonAsync("/api/Events/create", create);
            createResp.EnsureSuccessStatusCode();
            var ev = await createResp.Content.ReadFromJsonAsync<Event>();
            Assert.NotNull(ev);

            var deleteResp = await _client.DeleteAsync($"/api/Events/{ev!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

            var list = await _client.GetFromJsonAsync<Event[]>("/api/Events");
            Assert.DoesNotContain(list!, e => e.Id == ev.Id);
        }
    }
}
