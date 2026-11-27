namespace RestaurantManagement.DTO;

public sealed record ManagerCredential(
    int Id,
    string Username,
    string FullName,
    string Role,
    byte[] PasswordHash,
    byte[] PasswordSalt);
