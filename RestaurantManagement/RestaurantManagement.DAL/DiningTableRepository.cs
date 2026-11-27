using System.Data;
using Microsoft.Data.SqlClient;
using RestaurantManagement.DTO;

namespace RestaurantManagement.DAL;

public sealed class DiningTableRepository
{
    private readonly string _connectionString;

    public DiningTableRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string không được để trống.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<DiningTable>> GetAllAsync(
        string? code = null,
        string? area = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT MaBan, KhuVuc, SucChua, TrangThaiBan
            FROM dbo.BanAn
            WHERE (@Code = '' OR MaBan LIKE @CodePattern)
              AND (@Area = '' OR KhuVuc LIKE @AreaPattern)
              AND (@Status = '' OR TrangThaiBan = @Status)
            ORDER BY KhuVuc, MaBan;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(query, connection);

        var trimmedCode = code?.Trim() ?? string.Empty;
        var trimmedArea = area?.Trim() ?? string.Empty;
        command.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = trimmedCode;
        command.Parameters.Add("@CodePattern", SqlDbType.NVarChar, 22).Value = $"%{trimmedCode}%";
        command.Parameters.Add("@Area", SqlDbType.NVarChar, 50).Value = trimmedArea;
        command.Parameters.Add("@AreaPattern", SqlDbType.NVarChar, 52).Value = $"%{trimmedArea}%";
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = status ?? string.Empty;

        var tables = new List<DiningTable>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add(ReadDiningTable(reader));
        }

        return tables;
    }

    public async Task<IReadOnlyDictionary<string, int>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT TrangThaiBan, COUNT(*) AS Total
            FROM dbo.BanAn
            GROUP BY TrangThaiBan;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        while (await reader.ReadAsync(cancellationToken))
        {
            counts.Add(reader.GetString(0), reader.GetInt32(1));
        }

        return counts;
    }

    public async Task<DiningTable?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT MaBan, KhuVuc, SucChua, TrangThaiBan
            FROM dbo.BanAn
            WHERE MaBan = @MaBan;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@MaBan", SqlDbType.NVarChar, 20).Value = code;

        await using var reader = await command.ExecuteReaderAsync(
            CommandBehavior.SingleRow,
            cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? ReadDiningTable(reader) : null;
    }

    public async Task AddAsync(DiningTable table, CancellationToken cancellationToken = default)
    {
        const string query = """
            INSERT INTO dbo.BanAn (MaBan, KhuVuc, SucChua, TrangThaiBan)
            VALUES (@MaBan, @KhuVuc, @SucChua, @TrangThaiBan);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(query, connection);
        AddTableParameters(command, table);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        DiningTable table,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            UPDATE dbo.BanAn
            SET KhuVuc = @KhuVuc,
                SucChua = @SucChua,
                TrangThaiBan = @TrangThaiBan
            WHERE MaBan = @MaBan;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(query, connection);
        AddTableParameters(command, table);

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> UpdateStatusAsync(
        string code,
        string status,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            UPDATE dbo.BanAn
            SET TrangThaiBan = @TrangThaiBan
            WHERE MaBan = @MaBan;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@MaBan", SqlDbType.NVarChar, 20).Value = code;
        command.Parameters.Add("@TrangThaiBan", SqlDbType.NVarChar, 30).Value = status;

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> DeleteAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            DELETE FROM dbo.BanAn
            WHERE MaBan = @MaBan AND TrangThaiBan = @FreeStatus;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@MaBan", SqlDbType.NVarChar, 20).Value = code;
        command.Parameters.Add("@FreeStatus", SqlDbType.NVarChar, 30).Value = "Trống";

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    private static DiningTable ReadDiningTable(SqlDataReader reader) =>
        new(reader.GetString(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3));

    private static void AddTableParameters(SqlCommand command, DiningTable table)
    {
        command.Parameters.Add("@MaBan", SqlDbType.NVarChar, 20).Value = table.Code;
        command.Parameters.Add("@KhuVuc", SqlDbType.NVarChar, 50).Value = table.Area;
        command.Parameters.Add("@SucChua", SqlDbType.Int).Value = table.Capacity;
        command.Parameters.Add("@TrangThaiBan", SqlDbType.NVarChar, 30).Value = table.Status;
    }
}
