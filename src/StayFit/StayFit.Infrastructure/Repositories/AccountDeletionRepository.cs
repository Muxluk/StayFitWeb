using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Infrastructure.Data;
using StayFit.Infrastructure.Identity;

namespace StayFit.Infrastructure.Repositories;

public class AccountDeletionRepository : IAccountDeletionRepository
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountDeletionRepository> _logger;

    public AccountDeletionRepository(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountDeletionRepository> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<bool> CheckPasswordAsync(int userId, string password)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> DeleteUserDataAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var profiles = await _context.Set<UserProfile>().Where(x => x.UserId == userId).ToListAsync(cancellationToken);
            _context.Set<UserProfile>().RemoveRange(profiles);

            // ВИПРАВЛЕНО: UserId в NutritionGoal є рядком (string)
            var userIdString = userId.ToString();
            var goals = await _context.Set<NutritionGoal>().Where(x => x.UserId == userIdString).ToListAsync(cancellationToken);
            _context.Set<NutritionGoal>().RemoveRange(goals);

            var sessions = await _context.Set<UserSession>().Where(x => x.UserId == userId).ToListAsync(cancellationToken);
            _context.Set<UserSession>().RemoveRange(sessions);

            var foodLogs = await _context.Set<FoodLog>().Where(x => x.UserId == userId).ToListAsync(cancellationToken);
            _context.Set<FoodLog>().RemoveRange(foodLogs);

            // ВИПРАВЛЕНО: MealEntry використовує UserEmail замість UserId
            var userEmail = user.Email ?? string.Empty;
            var meals = await _context.Set<MealEntry>().Where(x => x.UserEmail == userEmail).ToListAsync(cancellationToken);
            _context.Set<MealEntry>().RemoveRange(meals);

            await _context.SaveChangesAsync(cancellationToken);

            // Видаляємо самого акаунта
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Помилка видалення користувача з UserManager: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час каскадного видалення акаунта для UserId={UserId}", userId);
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }
    }
}