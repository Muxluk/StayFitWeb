using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersWithFoodLogsAsync();
}
