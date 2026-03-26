using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Domain.Entities;
using StayFit.Domain.Enums;

namespace StayFit.Application.Interfaces;

public interface IProductSearchService
{
    Task<Result<PagedResult<Food>>> SearchAsync(string? searchTerm, FoodCategory? category, int page, int pageSize, int userId);
}
