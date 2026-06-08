namespace SensitiveWords.Application.Validators
{
    internal static class WordValidator
    {
        internal static IEnumerable<string> Validate(string? word)
        {
            if (string.IsNullOrWhiteSpace(word))
                yield return "Word cannot be empty.";

            if (word?.Length > 100)
                yield return "Word cannot exceed 100 characters.";

            if (word != null && word.Contains('\''))
                yield return "Word contains invalid characters.";
        }
    }
}
