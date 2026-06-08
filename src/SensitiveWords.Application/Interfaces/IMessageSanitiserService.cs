using SensitiveWords.Application.DTOs;

namespace SensitiveWords.Application.Interfaces
{
    public interface IMessageSanitiserService
    {
        Task<SanitiseResponse> SanitiseAsync(SanitiseRequest request, CancellationToken cancellationToken);
    }
}
