using System.Data.Common;
using RestaurantManagement.BUS;
using RestaurantManagement.DAL;

Console.WriteLine("Khởi tạo tài khoản quản lý đầu tiên");
Console.WriteLine("Tài khoản này chỉ được tạo khi bảng Managers chưa có dữ liệu.");
Console.Write("Tên đăng nhập: ");
var username = Console.ReadLine() ?? string.Empty;
Console.Write("Họ và tên: ");
var fullName = Console.ReadLine() ?? string.Empty;

Console.Write("Mật khẩu (tối thiểu 12 ký tự): ");
var password = ReadPassword();
Console.WriteLine();
Console.Write("Nhập lại mật khẩu: ");
var confirmation = ReadPassword();
Console.WriteLine();

if (!string.Equals(password, confirmation, StringComparison.Ordinal))
{
    Console.Error.WriteLine("Mật khẩu xác nhận không khớp.");
    return 1;
}

try
{
    var repository = new ManagerRepository(DatabaseConfiguration.GetRequiredConnectionString());
    var setupService = new InitialManagerService(repository);
    var created = await setupService.CreateAsync(username, fullName, password);

    Console.WriteLine(created
        ? "Đã tạo tài khoản quản lý đầu tiên thành công."
        : "Đã tồn tại tài khoản quản lý. Không có thay đổi nào được thực hiện.");
    return created ? 0 : 1;
}
catch (Exception exception) when (
    exception is ArgumentException or InvalidOperationException or DbException)
{
    Console.Error.WriteLine($"Không thể tạo tài khoản quản lý: {exception.Message}");
    return 1;
}

static string ReadPassword()
{
    var password = new System.Text.StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            return password.ToString();
        }

        if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password.Length--;
            continue;
        }

        if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
        }
    }
}
