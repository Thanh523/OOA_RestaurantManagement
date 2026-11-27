using RestaurantManagement.BUS;
using RestaurantManagement.DAL;

namespace RestaurantManagement.GUI;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var connectionString = DatabaseConfiguration.GetRequiredConnectionString();
            var managerRepository = new ManagerRepository(connectionString);
            var tableRepository = new DiningTableRepository(connectionString);
            Application.Run(new LoginForm(
                new ManagerAuthenticationService(managerRepository),
                new DiningTableService(tableRepository)));
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(
                exception.Message,
                "Chưa cấu hình cơ sở dữ liệu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
