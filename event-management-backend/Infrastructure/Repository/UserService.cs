using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context) => _context = context;

        public async Task<User?> GetByUsernameAsync(string username) =>
            await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserName == username);

        public async Task SaveTokenAsync(int userId, string token)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            user.JwtToken = token;
            await _context.SaveChangesAsync();
        }
    }
}
