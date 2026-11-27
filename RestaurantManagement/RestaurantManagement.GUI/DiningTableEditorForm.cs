using RestaurantManagement.BUS;
using RestaurantManagement.DTO;

namespace RestaurantManagement.GUI;

internal sealed class DiningTableEditorForm : Form
{
    private readonly TextBox _codeTextBox = new();
    private readonly ComboBox _areaInput = new();
    private readonly NumericUpDown _capacityInput = new();
    private readonly ComboBox _statusInput = new();

    public DiningTable Table => new(
        _codeTextBox.Text.Trim(),
        _areaInput.Text.Trim(),
        (int)_capacityInput.Value,
        _statusInput.SelectedItem?.ToString() ?? string.Empty);

    public DiningTableEditorForm(DiningTable? table = null, IEnumerable<string>? areas = null)
    {
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
        _codeTextBox.ReadOnly = table is not null;
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
        _capacityInput.Minimum = 1;
        _capacityInput.Maximum = 1000;
        _capacityInput.Value = table?.Capacity is > 0 and <= 1000 ? table.Capacity : 4;
        _capacityInput.TextAlign = HorizontalAlignment.Left;

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
        saveButton.Click += (_, _) =>
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

            DialogResult = DialogResult.OK;
            Close();
        };

        Controls.AddRange([title, subtitle, help, cancelButton, saveButton]);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

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
