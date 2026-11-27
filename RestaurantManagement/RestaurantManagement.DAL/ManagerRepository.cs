using System.Data;
using Microsoft.Data.SqlClient;
using RestaurantManagement.DTO;

namespace RestaurantManagement.DAL;

public sealed class ManagerRepository
{
    private readonly string _connectionString;

    public ManagerRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string không được để trống.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public async Task<ManagerCredential?> GetActiveByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT Id, Username, FullName, Role, PasswordHash, PasswordSalt
            FROM dbo.Managers
            WHERE Username = @Username AND IsActive = 1;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;

        await using var reader = await command.ExecuteReaderAsync(
            CommandBehavior.SingleRow,
            cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new ManagerCredential(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            (byte[])reader["PasswordHash"],
            (byte[])reader["PasswordSalt"]);
    }

    public async Task<bool> CreateInitialManagerAsync(
        string username,
        string fullName,
        byte[] passwordHash,
        byte[] passwordSalt,
        CancellationToken cancellationToken = default)
    {
        const string countQuery = "SELECT COUNT_BIG(1) FROM dbo.Managers WITH (UPDLOCK, HOLDLOCK);";
        const string insertQuery = """
            INSERT INTO dbo.Managers (Username, FullName, Role, PasswordHash, PasswordSalt)
            VALUES (@Username, @FullName, N'Quản lý', @PasswordHash, @PasswordSalt);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

        await using (var countCommand = new SqlCommand(countQuery, connection, transaction))
        {
            var managerCount = Convert.ToInt64(
                await countCommand.ExecuteScalarAsync(cancellationToken));

            if (managerCount > 0)
            {
                transaction.Rollback();
                return false;
            }
        }

        await using (var insertCommand = new SqlCommand(insertQuery, connection, transaction))
        {
            insertCommand.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
            insertCommand.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName;
            insertCommand.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 32).Value = passwordHash;
            insertCommand.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, 16).Value = passwordSalt;
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        transaction.Commit();
        return true;
    }
}
