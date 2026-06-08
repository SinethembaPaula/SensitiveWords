using Dapper;
using Microsoft.Data.SqlClient;
using SensitiveWords.Application.DTOs;
using SensitiveWords.Application.Interfaces;
namespace SensitiveWords.Infrastructure.Persistence.Repositories
{
    public sealed class SensitiveWordRepository(string connectionString) : ISensitiveWordRepository
    {
        private SqlConnection CreateConnection() => new(connectionString);

        public async Task<IEnumerable<SensitiveWordDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT Id, Word, CreatedAt, UpdatedAt
            FROM SensitiveWords
            ORDER BY Word
            """;

            await using var connection = CreateConnection();
            return await connection.QueryAsync<SensitiveWordDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        }

        public async Task<SensitiveWordDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT Id, Word, CreatedAt, UpdatedAt
            FROM SensitiveWords
            WHERE Id = @Id
            """;

            await using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<SensitiveWordDto>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        }

        public async Task<int> CreateAsync(CreateSensitiveWordRequest request, CancellationToken cancellationToken)
        {
            const string sql = """
            INSERT INTO SensitiveWords (Word)
            OUTPUT INSERTED.Id
            VALUES (@Word)
            """;

            await using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { request.Word }, cancellationToken: cancellationToken));
        }

        public async Task<bool> UpdateAsync(int id, UpdateSensitiveWordRequest request, CancellationToken cancellationToken)
        {
            const string sql = """
            UPDATE SensitiveWords
            SET Word = @Word, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id
            """;

            await using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { request.Word, Id = id }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            const string sql = "DELETE FROM SensitiveWords WHERE Id = @Id";

            await using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
            return rows > 0;
        }
    }
}
