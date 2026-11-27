using System.Text.Json;

namespace RestaurantManagement.DAL;

public static class DatabaseConfiguration
{
    public const string ConnectionStringEnvironmentVariable = "RESTAURANT_DB_CONNECTION";
    private const string ConfigurationFileName = "database.connection.json";

    public static string GetRequiredConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var configurationPath = Path.Combine(AppContext.BaseDirectory, ConfigurationFileName);
        if (!File.Exists(configurationPath))
        {
            throw new InvalidOperationException(
                $"Không tìm thấy file cấu hình kết nối: {configurationPath}. " +
                $"Hãy tạo file {ConfigurationFileName} theo hướng dẫn trong README.md.");
        }

        using var configuration = JsonDocument.Parse(File.ReadAllText(configurationPath));
        if (configuration.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) &&
            connectionStrings.TryGetProperty("RestaurantManagement", out var connectionStringElement) &&
            connectionStringElement.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(connectionStringElement.GetString()))
        {
            return connectionStringElement.GetString()!;
        }

        throw new InvalidOperationException(
            $"File {ConfigurationFileName} cần có mục " +
            "\"ConnectionStrings\": { \"RestaurantManagement\": \"...\" }.");
    }
}
