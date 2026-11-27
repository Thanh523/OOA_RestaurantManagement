using RestaurantManagement.BUS;

namespace RestaurantManagement.GUI;

internal sealed class DiningTableStatusForm : Form
{
    private readonly ComboBox _statusInput = new();

    public string SelectedStatus => _statusInput.SelectedItem?.ToString() ?? string.Empty;

    public DiningTableStatusForm(string currentStatus)
    {
        Text = "Cập nhật trạng thái bàn";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(390, 180);
        Font = new Font("Segoe UI", 10F);
        BackColor = Color.White;

        Controls.Add(new Label
        {
            Text = "Chọn trạng thái mới",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Location = new Point(28, 30),
            AutoSize = true
        });

        _statusInput.DropDownStyle = ComboBoxStyle.DropDownList;
        _statusInput.Items.AddRange(DiningTableService.AvailableStatuses.Cast<object>().ToArray());
        _statusInput.SelectedItem = currentStatus;
        _statusInput.Location = new Point(30, 65);
        _statusInput.Size = new Size(330, 32);
        Controls.Add(_statusInput);

        var saveButton = new Button
        {
            Text = "Cập nhật",
            Location = new Point(164, 120),
            Size = new Size(95, 34),
            DialogResult = DialogResult.OK,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(225, 141, 65),
            ForeColor = Color.White
        };
        saveButton.FlatAppearance.BorderSize = 0;

        var cancelButton = new Button
        {
            Text = "Hủy",
            Location = new Point(265, 120),
            Size = new Size(95, 34),
            DialogResult = DialogResult.Cancel
        };

        Controls.AddRange([saveButton, cancelButton]);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }
}
