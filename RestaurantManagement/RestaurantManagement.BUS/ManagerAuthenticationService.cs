using RestaurantManagement.DAL;
using RestaurantManagement.DTO;

namespace RestaurantManagement.BUS;

public sealed class ManagerAuthenticationService
{
    private readonly ManagerRepository _managerRepository;

    public ManagerAuthenticationService(ManagerRepository managerRepository)
    {
        _managerRepository = managerRepository;
    }

    public async Task<ManagerProfile?> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        var credential = await _managerRepository.GetActiveByUsernameAsync(
            username.Trim(),
            cancellationToken);

        if (credential is null ||
            !PasswordHasher.Verify(password, credential.PasswordHash, credential.PasswordSalt))
        {
            return null;
        }

        return new ManagerProfile(
            credential.Id,
            credential.Username,
            credential.FullName,
            credential.Role);
    }
}
