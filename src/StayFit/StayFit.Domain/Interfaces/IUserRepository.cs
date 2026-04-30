using System.Collections.Generic;
using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersWithFoodLogsAsync();
    Task<IEnumerable<User>> GetActiveAsync();
    Task<IEnumerable<User>> GetByRoleAsync(string role);
}
