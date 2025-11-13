using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;
        public EventService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Event>> GetAllAsync() =>
            await _context.Events.AsNoTracking().ToListAsync();

        public async Task<Event?> GetByIdAsync(int id) =>
            await _context.Events.Include(e => e.Registrations).FirstOrDefaultAsync(e => e.Id == id);

        public async Task<Event> CreateAsync(CreateEventRequest request)
        {
            var ev = new Event
            {
                Name = request.Name,
                Description = request.Description,
                Location = request.Location,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            };
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
            return ev;
        }

        public async Task<IEnumerable<Registration>> GetRegistrationsAsync(int eventId) =>
            await _context.Registrations.Where(r => r.EventId == eventId).AsNoTracking().ToListAsync();

        public async Task<bool> DeleteAsync(int id)
        {
            var ev = await _context.Events.Include(e => e.Registrations).FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return false;

            if (ev.Registrations.Count > 0)
            {
                _context.Registrations.RemoveRange(ev.Registrations);
            }

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
