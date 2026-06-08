using SensitiveWords.Application.DTOs;
using SensitiveWords.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace SensitiveWords.Infrastructure.Caching
{
    public sealed class CachedSensitiveWordRepository(
        ISensitiveWordRepository inner,
        IMemoryCache cache) : ISensitiveWordRepository
    {
        private const string CacheKey = "sensitive_words_all";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        public async Task<IEnumerable<SensitiveWordDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            if (cache.TryGetValue(CacheKey, out IEnumerable<SensitiveWordDto>? cached) && cached is not null)
                return cached;

            var words = await inner.GetAllAsync(cancellationToken);
            var wordList = words.ToList();

            cache.Set(CacheKey, wordList, CacheTtl);
            return wordList;
        }

        public Task<SensitiveWordDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
            => inner.GetByIdAsync(id, cancellationToken);

        public async Task<int> CreateAsync(CreateSensitiveWordRequest request, CancellationToken cancellationToken)
        {
            var id = await inner.CreateAsync(request, cancellationToken);
            cache.Remove(CacheKey);
            return id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateSensitiveWordRequest request, CancellationToken cancellationToken)
        {
            var updated = await inner.UpdateAsync(id, request, cancellationToken);
            if (updated) cache.Remove(CacheKey);
            return updated;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var deleted = await inner.DeleteAsync(id, cancellationToken);
            if (deleted) cache.Remove(CacheKey);
            return deleted;
        }
    }
}
