using RestaurantManagement.DAL;

namespace RestaurantManagement.BUS;

public sealed class InitialManagerService
{
    private const int MinimumPasswordLength = 12;
    private readonly ManagerRepository _managerRepository;

    public InitialManagerService(ManagerRepository managerRepository)
    {
        _managerRepository = managerRepository;
    }

    public async Task<bool> CreateAsync(
        string username,
        string fullName,
        string password,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(fullName);
        ArgumentNullException.ThrowIfNull(password);

        username = username.Trim();
        fullName = fullName.Trim();

        if (string.IsNullOrWhiteSpace(username) || username.Length > 50)
        {
            throw new ArgumentException("Tên đăng nhập phải có từ 1 đến 50 ký tự.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 100)
        {
            throw new ArgumentException("Họ tên phải có từ 1 đến 100 ký tự.", nameof(fullName));
        }

        if (password.Length < MinimumPasswordLength)
        {
            throw new ArgumentException(
                $"Mật khẩu phải có ít nhất {MinimumPasswordLength} ký tự.",
                nameof(password));
        }

        var (hash, salt) = PasswordHasher.CreateHash(password);
        return await _managerRepository.CreateInitialManagerAsync(
            username,
            fullName,
            hash,
            salt,
            cancellationToken);
    }
}
