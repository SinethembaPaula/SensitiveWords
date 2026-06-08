using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SensitiveWords.Application.DTOs;
using SensitiveWords.Application.Interfaces;

namespace SensitiveWords.Infrastructure.Persistence.Repositories
{
    public sealed class SensitiveWordRepository(
        string connectionString,
        ILogger<SensitiveWordRepository> logger) : ISensitiveWordRepository
    {
        private SqlConnection CreateConnection() => new(connectionString);

        public async Task<IEnumerable<SensitiveWordDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug("Fetching all sensitive words from database");

            const string sql = """
            SELECT Id, Word, CreatedAt, UpdatedAt
            FROM SensitiveWords
            ORDER BY LEN(Word) DESC
            """;

            await using var connection = CreateConnection();
            var result = (await connection.QueryAsync<SensitiveWordDto>(new CommandDefinition(sql, cancellationToken: cancellationToken))).ToList();

            logger.LogDebug("Fetched {Count} sensitive words from database", result.Count);
            return result;
        }

        public async Task<SensitiveWordDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            logger.LogDebug("Fetching sensitive word with Id {Id}", id);

            const string sql = """
            SELECT Id, Word, CreatedAt, UpdatedAt
            FROM SensitiveWords
            WHERE Id = @Id
            """;

            await using var connection = CreateConnection();
            var word = await connection.QuerySingleOrDefaultAsync<SensitiveWordDto>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

            if (word is null)
                logger.LogWarning("Sensitive word with Id {Id} was not found", id);
            else
                logger.LogDebug("Fetched sensitive word with Id {Id}", id);

            return word;
        }

        public async Task<int> CreateAsync(CreateSensitiveWordRequest request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating sensitive word '{Word}'", request.Word);

            const string sql = """
            INSERT INTO SensitiveWords (Word)
            OUTPUT INSERTED.Id
            VALUES (@Word)
            """;

            await using var connection = CreateConnection();
            var id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { request.Word }, cancellationToken: cancellationToken));

            logger.LogInformation("Created sensitive word '{Word}' with Id {Id}", request.Word, id);
            return id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateSensitiveWordRequest request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating sensitive word with Id {Id} to '{Word}'", id, request.Word);

            const string sql = """
            UPDATE SensitiveWords
            SET Word = @Word, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id
            """;

            await using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { request.Word, Id = id }, cancellationToken: cancellationToken));

            if (rows == 0)
                logger.LogWarning("Update failed — sensitive word with Id {Id} was not found", id);
            else
                logger.LogInformation("Updated sensitive word with Id {Id}", id);

            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting sensitive word with Id {Id}", id);

            const string sql = "DELETE FROM SensitiveWords WHERE Id = @Id";

            await using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

            if (rows == 0)
                logger.LogWarning("Delete failed — sensitive word with Id {Id} was not found", id);
            else
                logger.LogInformation("Deleted sensitive word with Id {Id}", id);

            return rows > 0;
        }
    }
}
