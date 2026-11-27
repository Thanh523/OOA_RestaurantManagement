using System.Data.Common;
using RestaurantManagement.BUS;
using RestaurantManagement.DTO;

namespace RestaurantManagement.GUI;

public sealed class LoginForm : Form
{
    private readonly ManagerAuthenticationService _authenticationService;
    private readonly DiningTableService _diningTableService;
    private readonly TextBox _usernameTextBox = new();
    private readonly TextBox _passwordTextBox = new();
    private readonly Button _loginButton = new();
    private readonly Label _statusLabel = new();

    public LoginForm(
        ManagerAuthenticationService authenticationService,
        DiningTableService diningTableService)
    {
        _authenticationService = authenticationService;
        _diningTableService = diningTableService;
        Text = "Đăng nhập | Restaurant Management";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = true;
        ClientSize = new Size(920, 560);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        BuildLayout();
    }

    private void BuildLayout()
    {
        var brandPanel = new Panel
        {
            BackColor = Color.FromArgb(25, 45, 70),
            Dock = DockStyle.Left,
            Width = 340,
            Padding = new Padding(38)
        };

        var brandMark = new Label
        {
            Text = "RM",
            ForeColor = Color.White,
            BackColor = Color.FromArgb(225, 141, 65),
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(72, 72),
            Location = new Point(38, 90)
        };

        var brandTitle = new Label
        {
            Text = "QUẢN LÝ\nNHÀ HÀNG",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 23F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(38, 190)
        };

        var brandDescription = new Label
        {
            Text = "Hệ thống quản lý dành cho\nngười quản lý nhà hàng.",
            ForeColor = Color.FromArgb(202, 213, 226),
            Font = new Font("Segoe UI", 11F),
            AutoSize = true,
            Location = new Point(40, 310)
        };

        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        Controls.Add(contentPanel);
        brandPanel.Controls.AddRange([brandMark, brandTitle, brandDescription]);
        Controls.Add(brandPanel);

        var heading = new Label
        {
            Text = "Đăng nhập",
            Font = new Font("Segoe UI", 25F, FontStyle.Bold),
            ForeColor = Color.FromArgb(35, 48, 65),
            AutoSize = true,
            Location = new Point(100, 72)
        };

        var subtitle = new Label
        {
            Text = "Vui lòng nhập thông tin tài khoản quản lý.",
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(112, 124, 139),
            AutoSize = true,
            Location = new Point(103, 125)
        };

        var usernameLabel = CreateFieldLabel("Tên đăng nhập", 185);
        _usernameTextBox.Location = new Point(103, 214);
        _usernameTextBox.Size = new Size(380, 34);
        _usernameTextBox.MaxLength = 50;
        _usernameTextBox.PlaceholderText = "Nhập tên đăng nhập";

        var passwordLabel = CreateFieldLabel("Mật khẩu", 270);
        _passwordTextBox.Location = new Point(103, 299);
        _passwordTextBox.Size = new Size(380, 34);
        _passwordTextBox.UseSystemPasswordChar = true;
        _passwordTextBox.PlaceholderText = "Nhập mật khẩu";

        _loginButton.Text = "ĐĂNG NHẬP";
        _loginButton.Location = new Point(103, 365);
        _loginButton.Size = new Size(380, 46);
        _loginButton.FlatStyle = FlatStyle.Flat;
        _loginButton.FlatAppearance.BorderSize = 0;
        _loginButton.BackColor = Color.FromArgb(225, 141, 65);
        _loginButton.ForeColor = Color.White;
        _loginButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _loginButton.Cursor = Cursors.Hand;
        _loginButton.Click += LoginButton_Click;

        _statusLabel.Location = new Point(103, 425);
        _statusLabel.Size = new Size(380, 50);
        _statusLabel.ForeColor = Color.FromArgb(190, 55, 55);
        _statusLabel.TextAlign = ContentAlignment.TopLeft;

        contentPanel.Controls.AddRange(
        [
            heading,
            subtitle,
            usernameLabel,
            _usernameTextBox,
            passwordLabel,
            _passwordTextBox,
            _loginButton,
            _statusLabel
        ]);

        AcceptButton = _loginButton;
    }

    private static Label CreateFieldLabel(string text, int top) =>
        new()
        {
            Text = text,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(55, 68, 84),
            AutoSize = true,
            Location = new Point(103, top)
        };

    private async void LoginButton_Click(object? sender, EventArgs e)
    {
        _statusLabel.Text = string.Empty;
        var username = _usernameTextBox.Text.Trim();
        var password = _passwordTextBox.Text;

        if (username.Length == 0 || password.Length == 0)
        {
            _statusLabel.Text = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
            return;
        }

        SetBusy(true);
        try
        {
            var manager = await _authenticationService.LoginAsync(username, password);
            if (manager is null)
            {
                _statusLabel.Text = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                return;
            }

            using var dashboard = new DashboardForm(manager, _diningTableService);
            Hide();
            dashboard.ShowDialog(this);
            Show();
            _passwordTextBox.Clear();
            _usernameTextBox.Focus();
        }
        catch (DbException exception)
        {
            _statusLabel.Text =
                $"Không thể kết nối cơ sở dữ liệu. Kiểm tra SQL Server và cấu hình kết nối.\n{exception.Message}";
        }
        catch (InvalidOperationException exception)
        {
            _statusLabel.Text = exception.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool isBusy)
    {
        _usernameTextBox.Enabled = !isBusy;
        _passwordTextBox.Enabled = !isBusy;
        _loginButton.Enabled = !isBusy;
        _loginButton.Text = isBusy ? "ĐANG KIỂM TRA..." : "ĐĂNG NHẬP";
        UseWaitCursor = isBusy;
    }
}
