using Microsoft.Extensions.Logging;
using SensitiveWords.Application.DTOs;
using SensitiveWords.Application.Interfaces;
using System.Text.RegularExpressions;

namespace SensitiveWords.Application.Services
{
    public sealed class MessageSanitiserService(
        ISensitiveWordRepository repository,
        ILogger<MessageSanitiserService> logger) : IMessageSanitiserService
    {
        private const string CacheKey = "sensitive_words_regex";

        private static Regex? _cachedRegex;
        private static IEnumerable<SensitiveWordDto>? _cachedWords;

        public async Task<SanitiseResponse> SanitiseAsync(SanitiseRequest request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Sanitising message of length {Length}", request.Input.Length);

            var words = await repository.GetAllAsync(cancellationToken);
            var wordList = words.ToList();

            var regex = GetOrBuildRegex(wordList);

            var replacementCount = regex.Matches(request.Input).Count;
            var sanitised = regex.Replace(request.Input, match => new string('*', match.Length));

            logger.LogInformation("Sanitisation complete — {ReplacementCount} sensitive word(s) replaced", replacementCount);

            return new SanitiseResponse(sanitised);
        }

        private static Regex GetOrBuildRegex(List<SensitiveWordDto> words)
        {
            if (_cachedRegex is not null && _cachedWords == words)
                return _cachedRegex;

            var sorted = words
                .Select(w => w.Word)
                .OrderByDescending(w => w.Length);

            var pattern = string.Join("|", sorted.Select(w => Regex.Escape(w)));

            _cachedRegex = new Regex(
                $@"\b(?:{pattern})\b",
                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                TimeSpan.FromSeconds(2));

            _cachedWords = words;
            return _cachedRegex;
        }
    }
}
