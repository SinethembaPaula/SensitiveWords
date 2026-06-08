using SensitiveWords.Application.DTOs;

namespace SensitiveWords.Application.Interfaces
{
    public interface ISensitiveWordRepository
    {
        Task<IEnumerable<SensitiveWordDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<SensitiveWordDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<int> CreateAsync(CreateSensitiveWordRequest request, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(int id, UpdateSensitiveWordRequest request, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
