using Domain.Entities;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllAsync();
        Task<Event?> GetByIdAsync(int id);
        Task<Event> CreateAsync(CreateEventRequest request);
        Task<IEnumerable<Registration>> GetRegistrationsAsync(int eventId);
        Task<bool> DeleteAsync(int id);
    }
}
