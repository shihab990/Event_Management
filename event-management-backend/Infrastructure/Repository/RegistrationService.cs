using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repository
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _context;
        public RegistrationService(ApplicationDbContext context) => _context = context;

        public async Task<Registration> RegisterAsync(int eventId, RegisterRequest request)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) throw new KeyNotFoundException("Event not found");

            var reg = new Registration
            {
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                EventId = eventId
            };
            _context.Registrations.Add(reg);
            await _context.SaveChangesAsync();
            return reg;
        }
    }
}
