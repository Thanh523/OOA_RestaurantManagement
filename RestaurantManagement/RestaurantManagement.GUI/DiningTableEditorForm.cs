using System.Data.Common;
using System.Globalization;
using RestaurantManagement.BUS;
using RestaurantManagement.DTO;

namespace RestaurantManagement.GUI;

internal sealed class DiningTableEditorForm : Form
{
    private readonly TextBox _codeTextBox = new();
    private readonly ComboBox _areaInput = new();
    private readonly TextBox _capacityInput = new();
    private readonly ComboBox _statusInput = new();
    private readonly Func<DiningTable, Task<bool>> _saveAsync;
    private int _capacityValue;

    public DiningTable Table => new(
        _codeTextBox.Text.Trim(),
        _areaInput.Text.Trim(),
        _capacityValue,
        _statusInput.SelectedItem?.ToString() ?? string.Empty);

    public DiningTableEditorForm(
        Func<DiningTable, Task<bool>> saveAsync,
        DiningTable? table = null,
        IEnumerable<string>? areas = null)
    {
        _saveAsync = saveAsync;
        Text = table is null ? "Thêm bàn ăn mới" : $"Chỉnh sửa bàn {table.Code}";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 485);
        Font = new Font("Segoe UI", 9.5F);
        BackColor = Color.White;

        BuildLayout(table, areas);
    }

    private void BuildLayout(DiningTable? table, IEnumerable<string>? areas)
    {
        var title = new Label
        {
            Text = table is null ? "Thêm bàn ăn mới" : $"Chỉnh sửa bàn {table.Code}",
            Font = new Font("Segoe UI", 19F, FontStyle.Bold),
            ForeColor = Color.FromArgb(41, 52, 70),
            AutoSize = true,
            Location = new Point(32, 24)
        };
        var subtitle = new Label
        {
            Text = "Các trường có dấu * là bắt buộc.",
            ForeColor = Color.FromArgb(113, 124, 141),
            AutoSize = true,
            Location = new Point(35, 65)
        };

        AddField("Mã bàn *", 108, _codeTextBox);
        _codeTextBox.MaxLength = 20;
        _codeTextBox.Text = table?.Code ?? string.Empty;
        _codeTextBox.PlaceholderText = "Ví dụ: B09";

        AddField("Khu vực *", 180, _areaInput);
        _areaInput.DropDownStyle = ComboBoxStyle.DropDown;
        _areaInput.MaxLength = 50;
        if (areas is not null)
        {
            _areaInput.Items.AddRange(areas
                .Where(area => !string.IsNullOrWhiteSpace(area))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Cast<object>()
                .ToArray());
        }

        _areaInput.Text = table?.Area ?? string.Empty;

        AddField("Sức chứa *", 252, _capacityInput);
        _capacityInput.MaxLength = 10;
        _capacityInput.Text = table?.Capacity.ToString(CultureInfo.InvariantCulture) ?? "4";
        _capacityInput.PlaceholderText = "Ví dụ: 4";

        AddField("Trạng thái *", 324, _statusInput);
        _statusInput.DropDownStyle = ComboBoxStyle.DropDownList;
        _statusInput.Items.AddRange(DiningTableService.AvailableStatuses.Cast<object>().ToArray());
        _statusInput.SelectedItem = table?.Status ?? DiningTableService.AvailableStatuses[0];

        var help = new Label
        {
            Text = "Có thể chọn khu vực đã dùng hoặc nhập khu vực mới. Sức chứa phải lớn hơn 0.",
            ForeColor = Color.FromArgb(113, 124, 141),
            AutoSize = true,
            Location = new Point(35, 378)
        };

        var cancelButton = CreateButton("Hủy", 310, 426, primary: false);
        cancelButton.DialogResult = DialogResult.Cancel;
        var saveButton = CreateButton(
            table is null ? "Lưu bàn" : "Xác nhận và lưu",
            380,
            426,
            primary: true);
        saveButton.Width = 108;
        saveButton.Click += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_codeTextBox.Text) ||
                string.IsNullOrWhiteSpace(_areaInput.Text))
            {
                MessageBox.Show(
                    this,
                    "Vui lòng nhập mã bàn và khu vực.",
                    "Thiếu thông tin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(
                    _capacityInput.Text,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out _capacityValue) ||
                _capacityValue <= 0)
            {
                MessageBox.Show(
                    this,
                    "Sức chứa phải là số nguyên dương. Vui lòng nhập lại số lớn hơn 0.",
                    "Sức chứa không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _capacityInput.Focus();
                _capacityInput.SelectAll();
                return;
            }

            if (table is not null && IsInUseStatus(table.Status))
            {
                var answer = MessageBox.Show(
                    this,
                    "Bàn đang trong sử dụng bạn có muốn tiếp tục thay đổi không?",
                    "Xác nhận thay đổi bàn",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (answer != DialogResult.Yes)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }
            }

            try
            {
                if (!await _saveAsync(Table))
                {
                    MessageBox.Show(
                        this,
                        "Bàn không còn tồn tại. Vui lòng kiểm tra lại thông tin.",
                        "Không thể lưu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show(
                    this,
                    exception.Message,
                    "Thông tin không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            catch (DbException exception)
            {
                MessageBox.Show(
                    this,
                    $"Không thể lưu thông tin bàn. Vui lòng kiểm tra lại rồi thử lại.\n\n{exception.Message}",
                    "Lỗi lưu dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        };

        Controls.AddRange([title, subtitle, help, cancelButton, saveButton]);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private static bool IsInUseStatus(string status) =>
        string.Equals(status, "Đã đặt", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Đang phục vụ", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Đang sử dụng", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Đang trong sử dụng", StringComparison.OrdinalIgnoreCase);

    private void AddField(string labelText, int top, Control control)
    {
        Controls.Add(new Label
        {
            Text = labelText,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(55, 68, 84),
            Location = new Point(35, top),
            Size = new Size(440, 21)
        });

        control.Location = new Point(35, top + 23);
        control.Size = new Size(440, 32);
        control.Font = new Font("Segoe UI", 9.5F);
        control.BackColor = Color.White;
        Controls.Add(control);
    }

    private static Button CreateButton(string text, int left, int top, bool primary)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(left, top),
            Size = new Size(64, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Color.FromArgb(42, 88, 160) : Color.White,
            ForeColor = primary ? Color.White : Color.FromArgb(55, 68, 84),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(216, 222, 231);
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        return button;
    }
}
