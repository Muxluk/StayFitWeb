using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

/// <summary>
/// Репозиторій для зберігання та отримання сеансів з бази даних
/// </summary>
public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _context;

    public SessionRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<UserSession> CreateAsync(UserSession session)
    {
        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    /// <inheritdoc/>
    public async Task<IList<UserSession>> GetActiveByUserIdAsync(int userId)
    {
        return await _context.UserSessions
            .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.LastActivityAt ?? s.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<UserSession?> GetByTokenAsync(string token)
    {
        return await _context.UserSessions
            .FirstOrDefaultAsync(s => s.SessionToken == token);
    }

    /// <inheritdoc/>
    public async Task<bool> IsValidAsync(string token)
    {
        return await _context.UserSessions
            .AnyAsync(s => s.SessionToken == token && s.IsActive && s.ExpiresAt > DateTime.UtcNow);
    }

    /// <inheritdoc/>
    public async Task<bool> DeactivateAsync(int sessionId)
    {
        var session = await _context.UserSessions.FindAsync(sessionId);
        if (session == null)
            return false;

        session.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<int> DeactivateAllExceptAsync(int userId, string currentToken)
    {
        var sessions = await _context.UserSessions
            .Where(s => s.UserId == userId && s.IsActive && s.SessionToken != currentToken)
            .ToListAsync();

        foreach (var session in sessions)
            session.IsActive = false;

        await _context.SaveChangesAsync();
        return sessions.Count;
    }

    /// <inheritdoc/>
    public async Task<int> DeactivateAllAsync(int userId)
    {
        var sessions = await _context.UserSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        foreach (var session in sessions)
            session.IsActive = false;

        await _context.SaveChangesAsync();
        return sessions.Count;
    }

    /// <inheritdoc/>
    public async Task UpdateLastActivityAsync(string token)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.SessionToken == token && s.IsActive);

        if (session != null)
        {
            session.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<int> DeleteExpiredAsync()
    {
        var expired = await _context.UserSessions
            .Where(s => s.IsActive && s.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var session in expired)
            session.IsActive = false;

        await _context.SaveChangesAsync();
        return expired.Count;
    }
}
