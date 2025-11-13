using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetByUsernameAsync(string username);
        Task SaveTokenAsync(int userId, string token);
    }
}
