using Domain.Entities;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IRegistrationService
    {
        Task<Registration> RegisterAsync(int eventId, RegisterRequest request);
    }
}
