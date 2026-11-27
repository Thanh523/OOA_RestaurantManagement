using RestaurantManagement.BUS;
using RestaurantManagement.DTO;

namespace RestaurantManagement.GUI;

public sealed class DashboardForm : Form
{
    public DashboardForm(ManagerProfile manager, DiningTableService diningTableService)
    {
        Text = "Restaurant Management";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(720, 440);
        Size = new Size(900, 560);
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 10F);

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 74,
            BackColor = Color.FromArgb(25, 45, 70),
            Padding = new Padding(24, 0, 24, 0)
        };

        var title = new Label
        {
            Text = "TỔNG QUAN",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(24, 23)
        };
        header.Controls.Add(title);

        var welcome = new Label
        {
            Text = $"Xin chào, {manager.FullName}",
            ForeColor = Color.FromArgb(35, 48, 65),
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(42, 118)
        };

        var role = new Label
        {
            Text = $"Vai trò: {manager.Role}  |  Tài khoản: {manager.Username}",
            ForeColor = Color.FromArgb(112, 124, 139),
            AutoSize = true,
            Location = new Point(46, 171)
        };

        var information = new Label
        {
            Text = "Chọn một chức năng để tiếp tục quản lý nhà hàng.",
            ForeColor = Color.FromArgb(75, 88, 104),
            AutoSize = true,
            Location = new Point(46, 225)
        };

        var tablesButton = new Button
        {
            Text = "Quản lý bàn ăn",
            Size = new Size(170, 42),
            Location = new Point(46, 285),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(225, 141, 65),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        tablesButton.FlatAppearance.BorderSize = 0;
        tablesButton.Click += (_, _) =>
        {
            using var tableManagement = new DiningTableManagementForm(diningTableService, manager);
            if (tableManagement.ShowDialog(this) == DialogResult.Abort)
            {
                Close();
            }
        };

        var logoutButton = new Button
        {
            Text = "Đăng xuất",
            Size = new Size(130, 42),
            Location = new Point(230, 285),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(55, 68, 84),
            Cursor = Cursors.Hand
        };
        logoutButton.Click += (_, _) => Close();

        Controls.AddRange([welcome, role, information, tablesButton, logoutButton]);
        Controls.Add(header);
    }
}
