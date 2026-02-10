using Domain.Entities;

namespace Application.Features.Associados;

public interface IAssociadoService
{
    Task<string> CreateAsync(CreateAssociadoRequest request);
    Task<string> UpdateAsync(UpdateAssociadoRequest request, string id);
    Task<string> DeleteAsync(string id);
    Task<AssociadoResponse> GetByIdAsync(string id);
    Task<List<AssociadoResponse>> GetAllAsync();
}
