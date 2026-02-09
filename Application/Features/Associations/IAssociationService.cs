using Domain.Entities;

namespace Application.Features.Associations;

public interface IAssociationService
{
    Task<string> CreateAsync(Association association);
    Task<string> UpdateAsync(Association association);
    Task<string> DeleteAsync(Association association);
    Task<Association> GetByIdAsync(string associationId);
    Task<List<Association>> GetAllAsync();
    Task<Association> GetByNameAsync(string name);
}
