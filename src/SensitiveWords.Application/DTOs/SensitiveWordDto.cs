namespace SensitiveWords.Application.DTOs
{
    public sealed record SensitiveWordDto(
        int Id,
        string Word,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
