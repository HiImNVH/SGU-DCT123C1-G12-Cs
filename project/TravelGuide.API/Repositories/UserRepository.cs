using Microsoft.EntityFrameworkCore;
using TravelGuide.API.Data;
using TravelGuide.Core.Models;

namespace TravelGuide.API.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task UpdateLanguageAsync(Guid userId, string langCode);
}

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// SELECT WHERE Username = ?
    /// </summary>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task AddAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// UPDATE PreferredLanguage
    /// </summary>
    public async Task UpdateLanguageAsync(Guid userId, string langCode)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user != null)
        {
            user.PreferredLanguage = langCode;
            await _db.SaveChangesAsync();
        }
    }
}