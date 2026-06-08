using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SensitiveWords.Application.DTOs;
using SensitiveWords.Application.Interfaces;

namespace SensitiveWords.Infrastructure.Caching
{
    public sealed class CachedSensitiveWordRepository(
        ISensitiveWordRepository inner,
        IMemoryCache cache,
        ILogger<CachedSensitiveWordRepository> logger) : ISensitiveWordRepository
    {
        private const string CacheKey = "sensitive_words_all";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        public async Task<IEnumerable<SensitiveWordDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            if (cache.TryGetValue(CacheKey, out IEnumerable<SensitiveWordDto>? cached) && cached is not null)
            {
                logger.LogDebug("Cache hit for sensitive words");
                return cached;
            }

            logger.LogDebug("Cache miss for sensitive words — loading from database");
            var words = await inner.GetAllAsync(cancellationToken);
            var wordList = words.ToList();

            cache.Set(CacheKey, wordList, CacheTtl);
            logger.LogInformation("Sensitive words cache populated with {Count} words, TTL {TTL}m", wordList.Count, CacheTtl.TotalMinutes);
            return wordList;
        }

        public Task<SensitiveWordDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
            => inner.GetByIdAsync(id, cancellationToken);

        public async Task<int> CreateAsync(CreateSensitiveWordRequest request, CancellationToken cancellationToken)
        {
            var id = await inner.CreateAsync(request, cancellationToken);
            cache.Remove(CacheKey);
            logger.LogInformation("Sensitive words cache invalidated after create");
            return id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateSensitiveWordRequest request, CancellationToken cancellationToken)
        {
            var updated = await inner.UpdateAsync(id, request, cancellationToken);
            if (updated)
            {
                cache.Remove(CacheKey);
                logger.LogInformation("Sensitive words cache invalidated after update of Id {Id}", id);
            }
            return updated;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var deleted = await inner.DeleteAsync(id, cancellationToken);
            if (deleted)
            {
                cache.Remove(CacheKey);
                logger.LogInformation("Sensitive words cache invalidated after delete of Id {Id}", id);
            }
            return deleted;
        }
    }
}
