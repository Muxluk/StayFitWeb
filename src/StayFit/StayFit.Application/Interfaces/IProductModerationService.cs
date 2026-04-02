using StayFit.Domain.Entities;
using StayFit.Domain.Results;

namespace StayFit.Application.Interfaces;

public interface IProductModerationService
{
    // Отримання списку неперевірених продуктів
    Task<Result<IEnumerable<Food>>> GetPendingProductsAsync();
    
    // Підтвердження продукту (IsApproved = true)
    Task<Result<bool>> ApproveProductAsync(int id);
    
    // Відхилення продукту (видалення або зміна статусу)
    Task<Result<bool>> RejectProductAsync(int id);
}