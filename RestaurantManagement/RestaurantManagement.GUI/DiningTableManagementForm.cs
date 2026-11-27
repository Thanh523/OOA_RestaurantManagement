using System.Data.Common;
using RestaurantManagement.BUS;
using RestaurantManagement.DTO;

namespace RestaurantManagement.GUI;

public sealed class DiningTableManagementForm : Form
{
    private static readonly Color Navy = Color.FromArgb(29, 42, 59);
    private static readonly Color Accent = Color.FromArgb(42, 88, 160);
    private static readonly Color TextColor = Color.FromArgb(41, 52, 70);
    private static readonly Color MutedText = Color.FromArgb(113, 124, 141);
    private static readonly Color PageBackground = Color.FromArgb(246, 248, 251);

    private readonly DiningTableService _tableService;
    private readonly ManagerProfile _manager;
    private readonly TextBox _codeFilter = new();
    private readonly TextBox _areaFilter = new();
    private readonly ComboBox _statusFilter = new();
    private readonly DataGridView _tableGrid = new();
    private readonly Label _resultLabel = new();
    private readonly Dictionary<string, Button> _statusCards = new(StringComparer.Ordinal);

    public DiningTableManagementForm(DiningTableService tableService, ManagerProfile manager)
    {
        _tableService = tableService;
        _manager = manager;
        Text = "Quản lý bàn ăn";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(1080, 680);
        Size = new Size(1280, 790);
        BackColor = PageBackground;
        Font = new Font("Segoe UI", 9.5F);

        BuildLayout();
        Load += async (_, _) => await RefreshTablesAsync();
    }

    private void BuildLayout()
    {
        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = PageBackground,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 205));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var sidebar = BuildSidebar();
        shell.Controls.Add(sidebar, 0, 0);
        shell.SetRowSpan(sidebar, 2);
        shell.Controls.Add(BuildTopBar(), 1, 0);
        shell.Controls.Add(BuildMainContent(), 1, 1);
        Controls.Add(shell);
    }

    private Control BuildSidebar()
    {
        var sidebar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Navy,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty
        };
        sidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 106));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));

        var brand = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 20, 12, 12)
        };
        brand.Controls.Add(new Label
        {
            Text = "RM",
            BackColor = Color.FromArgb(224, 145, 75),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(42, 42),
            Location = new Point(18, 22)
        });
        brand.Controls.Add(new Label
        {
            Text = "HỆ THỐNG\nQUẢN LÝ",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(70, 25)
        });
        sidebar.Controls.Add(brand, 0, 0);

        var sectionLabel = new Label
        {
            Text = "ĐIỀU HƯỚNG",
            ForeColor = Color.FromArgb(151, 165, 184),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 8, 0, 0)
        };
        sidebar.Controls.Add(sectionLabel, 0, 1);

        var navigation = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Navy,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        var tablesButton = CreateSidebarButton("Quản lý bàn ăn", selected: true);
        tablesButton.Click += async (_, _) => await RefreshTablesAsync();
        var homeButton = CreateSidebarButton("Quay lại tổng quan", selected: false);
        homeButton.Click += (_, _) => Close();
        navigation.Controls.Add(tablesButton);
        navigation.Controls.Add(homeButton);
        sidebar.Controls.Add(navigation, 0, 2);

        var userPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18, 14, 12, 8),
            BackColor = Color.FromArgb(24, 36, 52)
        };
        userPanel.Controls.Add(new Label
        {
            Text = $"Đang đăng nhập\n{_manager.FullName} · {_manager.Role}",
            ForeColor = Color.FromArgb(218, 226, 236),
            Font = new Font("Segoe UI", 8.5F),
            Dock = DockStyle.Fill
        });
        sidebar.Controls.Add(userPanel, 0, 3);

        return sidebar;
    }

    private Control BuildTopBar()
    {
        var topBar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(26, 0, 24, 0)
        };
        topBar.Controls.Add(new Label
        {
            Text = "Nhà hàng  /  Quản lý bàn ăn",
            ForeColor = MutedText,
            AutoSize = true,
            Location = new Point(26, 20)
        });
        var logoutButton = CreateSecondaryButton("Đăng xuất");
        logoutButton.Size = new Size(88, 30);
        logoutButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        logoutButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Abort;
            Close();
        };
        topBar.Controls.Add(logoutButton);

        var roleLabel = new Label
        {
            Text = $"Vai trò: {_manager.Role}",
            ForeColor = TextColor,
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        topBar.Controls.Add(roleLabel);
        void PositionRoleLabel()
        {
            logoutButton.Location = new Point(
                Math.Max(26, topBar.ClientSize.Width - logoutButton.Width - 24),
                13);
            roleLabel.Location = new Point(
                Math.Max(26, logoutButton.Left - roleLabel.Width - 20),
                20);
        }

        topBar.Resize += (_, _) => PositionRoleLabel();
        PositionRoleLabel();
        return topBar;
    }

    private Control BuildMainContent()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26, 19, 26, 16),
            ColumnCount = 1,
            RowCount = 5,
            BackColor = PageBackground
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        layout.Controls.Add(BuildPageHeading(), 0, 0);
        layout.Controls.Add(BuildStatusCards(), 0, 1);
        layout.Controls.Add(BuildFilters(), 0, 2);
        layout.Controls.Add(BuildTableGrid(), 0, 3);
        layout.Controls.Add(BuildFooter(), 0, 4);
        return layout;
    }

    private Control BuildPageHeading()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(new Label
        {
            Text = "Quản lý bàn ăn",
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = TextColor,
            AutoSize = true,
            Location = new Point(0, 1)
        });
        panel.Controls.Add(new Label
        {
            Text = "Xem danh sách, cập nhật thông tin và trạng thái bàn ăn.",
            ForeColor = MutedText,
            AutoSize = true,
            Location = new Point(2, 41)
        });

        var addButton = CreatePrimaryButton("+  Thêm bàn mới");
        addButton.Size = new Size(150, 38);
        addButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        addButton.Location = new Point(Math.Max(0, panel.Width - 150), 14);
        addButton.Click += async (_, _) => await AddTableAsync();
        panel.Resize += (_, _) =>
            addButton.Location = new Point(Math.Max(0, panel.ClientSize.Width - addButton.Width), 14);
        panel.Controls.Add(addButton);
        return panel;
    }

    private Control BuildStatusCards()
    {
        var cards = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            AutoScroll = true,
            BackColor = PageBackground,
            Padding = new Padding(0, 5, 0, 4)
        };

        AddStatusCard(cards, string.Empty, "Tất cả");
        foreach (var status in DiningTableService.AvailableStatuses)
        {
            AddStatusCard(cards, status, status);
        }

        return cards;
    }

    private void AddStatusCard(FlowLayoutPanel parent, string status, string label)
    {
        var button = new Button
        {
            Text = $"{label}  0",
            Tag = status,
            Size = new Size(145, 52),
            Margin = new Padding(0, 0, 10, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = TextColor,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(224, 229, 236);
        button.Click += async (_, _) =>
        {
            _statusFilter.SelectedItem = string.IsNullOrEmpty(status) ? "Tất cả" : status;
            UpdateStatusCardSelection();
            await RefreshTablesAsync();
        };
        _statusCards.Add(status, button);
        parent.Controls.Add(button);
    }

    private Control BuildFilters()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(14, 9, 14, 8)
        };

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 2,
            BackColor = Color.White
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 13));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 13));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        fields.Controls.Add(CreateFilterLabel("Mã bàn"), 0, 0);
        fields.Controls.Add(CreateFilterLabel("Khu vực"), 1, 0);
        fields.Controls.Add(CreateFilterLabel("Trạng thái"), 2, 0);
        ConfigureFilterTextBox(_codeFilter, "Nhập mã bàn");
        ConfigureFilterTextBox(_areaFilter, "Nhập khu vực");
        _statusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _statusFilter.Items.Add("Tất cả");
        _statusFilter.Items.AddRange(DiningTableService.AvailableStatuses.Cast<object>().ToArray());
        _statusFilter.SelectedIndex = 0;
        StyleFilterControl(_statusFilter);
        fields.Controls.Add(_codeFilter, 0, 1);
        fields.Controls.Add(_areaFilter, 1, 1);
        fields.Controls.Add(_statusFilter, 2, 1);

        var searchButton = CreatePrimaryButton("Tìm kiếm");
        searchButton.Dock = DockStyle.Fill;
        searchButton.Margin = new Padding(8, 0, 4, 0);
        searchButton.Click += async (_, _) => await RefreshTablesAsync();
        var resetButton = CreateSecondaryButton("Đặt lại");
        resetButton.Dock = DockStyle.Fill;
        resetButton.Margin = new Padding(4, 0, 0, 0);
        resetButton.Click += async (_, _) =>
        {
            _codeFilter.Clear();
            _areaFilter.Clear();
            _statusFilter.SelectedIndex = 0;
            UpdateStatusCardSelection();
            await RefreshTablesAsync();
        };
        fields.Controls.Add(searchButton, 3, 1);
        fields.Controls.Add(resetButton, 4, 1);

        _codeFilter.KeyDown += async (_, e) => await SearchOnEnterAsync(e);
        _areaFilter.KeyDown += async (_, e) => await SearchOnEnterAsync(e);
        _statusFilter.SelectedIndexChanged += (_, _) => UpdateStatusCardSelection();
        panel.Controls.Add(fields);
        return panel;
    }

    private Control BuildTableGrid()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(8)
        };

        _tableGrid.Dock = DockStyle.Fill;
        _tableGrid.ReadOnly = true;
        _tableGrid.AllowUserToAddRows = false;
        _tableGrid.AllowUserToDeleteRows = false;
        _tableGrid.AllowUserToResizeRows = false;
        _tableGrid.MultiSelect = false;
        _tableGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _tableGrid.AutoGenerateColumns = false;
        _tableGrid.RowHeadersVisible = false;
        _tableGrid.BackgroundColor = Color.White;
        _tableGrid.BorderStyle = BorderStyle.None;
        _tableGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _tableGrid.GridColor = Color.FromArgb(235, 238, 243);
        _tableGrid.RowTemplate.Height = 42;
        _tableGrid.ColumnHeadersHeight = 42;
        _tableGrid.EnableHeadersVisualStyles = false;
        _tableGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 252);
        _tableGrid.ColumnHeadersDefaultCellStyle.ForeColor = MutedText;
        _tableGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _tableGrid.DefaultCellStyle.ForeColor = TextColor;
        _tableGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 240, 252);
        _tableGrid.DefaultCellStyle.SelectionForeColor = TextColor;
        _tableGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(251, 252, 254);
        _tableGrid.CellFormatting += FormatStatusCell;
        _tableGrid.CellContentClick += TableGrid_CellContentClick;

        AddTextColumn(nameof(DiningTable.Code), "Mã bàn", 90);
        AddTextColumn(nameof(DiningTable.Area), "Khu vực", 130);
        _tableGrid.Columns[^1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        AddTextColumn(nameof(DiningTable.Capacity), "Sức chứa", 75);
        AddTextColumn(nameof(DiningTable.Status), "Trạng thái", 115);
        AddActionColumn("DetailAction", "Chi tiết", 75);
        AddActionColumn("StatusAction", "Đổi trạng thái", 110);
        AddActionColumn("EditAction", "Chỉnh sửa", 85);
        AddActionColumn("DeleteAction", "Xóa", 60);

        panel.Controls.Add(_tableGrid);
        return panel;
    }

    private Control BuildFooter()
    {
        var footer = new Panel { Dock = DockStyle.Fill };
        _resultLabel.Dock = DockStyle.Left;
        _resultLabel.Width = 430;
        _resultLabel.TextAlign = ContentAlignment.MiddleLeft;
        _resultLabel.ForeColor = MutedText;
        footer.Controls.Add(_resultLabel);
        footer.Controls.Add(new Label
        {
            Text = "Dữ liệu bàn ăn",
            Dock = DockStyle.Right,
            Width = 160,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = MutedText
        });
        return footer;
    }

    private async Task RefreshTablesAsync()
    {
        var codeFilter = _codeFilter.Text.Trim();
        if (codeFilter.Any(character =>
                !char.IsLetterOrDigit(character) &&
                character != '-' &&
                character != '_'))
        {
            MessageBox.Show(
                this,
                "Mã bàn không hợp lệ. Vui lòng nhập lại chỉ bằng chữ cái, chữ số, dấu gạch ngang (-) hoặc gạch dưới (_).",
                "Mã bàn không hợp lệ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            _codeFilter.Focus();
            _codeFilter.SelectAll();
            return;
        }

        try
        {
            var status = _statusFilter.SelectedIndex > 0
                ? _statusFilter.SelectedItem?.ToString()
                : null;
            var tablesTask = _tableService.GetAllAsync(codeFilter, _areaFilter.Text, status);
            var countsTask = _tableService.GetStatusCountsAsync();
            await Task.WhenAll(tablesTask, countsTask);

            var tables = await tablesTask;
            var counts = await countsTask;
            _tableGrid.DataSource = tables.ToList();
            UpdateStatusCardCounts(counts);
            _resultLabel.Text = tables.Count == 0
                ? "Không tìm thấy bàn phù hợp."
                : $"Đang hiển thị {tables.Count} bàn ăn";
            UpdateStatusCardSelection();
        }
        catch (DbException exception)
        {
            ShowDatabaseError("Không thể tải danh sách bàn ăn.", exception);
        }
    }

    private async Task AddTableAsync()
    {
        try
        {
            var tables = await _tableService.GetAllAsync();
            using var editor = new DiningTableEditorForm(
                async table =>
                {
                    await _tableService.AddAsync(table);
                    return true;
                },
                areas: tables.Select(table => table.Area).Distinct(StringComparer.OrdinalIgnoreCase));
            if (editor.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            await RefreshTablesAsync();
            _resultLabel.Text = "Đã thêm bàn thành công.";
        }
        catch (ArgumentException exception)
        {
            ShowValidationError(exception);
        }
        catch (DbException exception)
        {
            ShowDatabaseError("Không thể thêm bàn. Hãy kiểm tra mã bàn có bị trùng không.", exception);
        }
    }

    private async Task EditTableAsync(DiningTable table)
    {
        try
        {
            var tables = await _tableService.GetAllAsync();
            using var editor = new DiningTableEditorForm(
                updatedTable => _tableService.UpdateAsync(table.Code, updatedTable),
                table,
                tables.Select(item => item.Area).Distinct(StringComparer.OrdinalIgnoreCase));
            if (editor.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            await RefreshTablesAsync();
            _resultLabel.Text = "Đã cập nhật thông tin bàn.";
        }
        catch (ArgumentException exception)
        {
            ShowValidationError(exception);
        }
        catch (DbException exception)
        {
            ShowDatabaseError("Không thể cập nhật bàn.", exception);
        }
    }

    private async Task ChangeStatusAsync(DiningTable table)
    {
        using var statusForm = new DiningTableStatusForm(table.Status);
        if (statusForm.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            if (await _tableService.UpdateStatusAsync(table.Code, statusForm.SelectedStatus))
            {
                await RefreshTablesAsync();
                _resultLabel.Text = "Đã cập nhật trạng thái bàn.";
            }
            else
            {
                await RefreshTablesAsync();
            }
        }
        catch (ArgumentException exception)
        {
            ShowValidationError(exception);
        }
        catch (DbException exception)
        {
            ShowDatabaseError("Không thể cập nhật trạng thái bàn.", exception);
        }
    }

    private async Task DeleteTableAsync(DiningTable table)
    {
        var answer = MessageBox.Show(
            this,
            $"Bạn có chắc muốn xóa bàn '{table.Code}' ở khu vực '{table.Area}'?",
            "Xác nhận xóa bàn",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes)
        {
            return;
        }

        try
        {
            if (await _tableService.DeleteAsync(table.Code))
            {
                await RefreshTablesAsync();
                _resultLabel.Text = "Đã xóa bàn thành công.";
            }
            else
            {
                await RefreshTablesAsync();
            }
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Không thể xóa bàn",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        catch (DbException exception)
        {
            ShowDatabaseError(
                "Không thể xóa bàn. Bàn có thể đang được dữ liệu khác sử dụng.",
                exception);
        }
    }

    private async void TableGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0 ||
            _tableGrid.Rows[e.RowIndex].DataBoundItem is not DiningTable table)
        {
            return;
        }

        switch (_tableGrid.Columns[e.ColumnIndex].Name)
        {
            case "DetailAction":
                ShowDetails(table);
                break;
            case "StatusAction":
                await ChangeStatusAsync(table);
                break;
            case "EditAction":
                await EditTableAsync(table);
                break;
            case "DeleteAction":
                await DeleteTableAsync(table);
                break;
        }
    }

    private void ShowDetails(DiningTable table)
    {
        MessageBox.Show(
            this,
            $"Mã bàn: {table.Code}\nKhu vực: {table.Area}\nSức chứa: {table.Capacity} người\nTrạng thái: {table.Status}",
            "Chi tiết bàn ăn",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void UpdateStatusCardCounts(IReadOnlyDictionary<string, int> counts)
    {
        var total = counts.Values.Sum();
        _statusCards[string.Empty].Text = $"Tất cả  {total}";
        foreach (var status in DiningTableService.AvailableStatuses)
        {
            var count = counts.TryGetValue(status, out var value) ? value : 0;
            _statusCards[status].Text = $"{status}  {count}";
        }
    }

    private void UpdateStatusCardSelection()
    {
        if (_statusCards.Count == 0)
        {
            return;
        }

        var selectedStatus = _statusFilter.SelectedIndex > 0
            ? _statusFilter.SelectedItem?.ToString()
            : string.Empty;

        foreach (var (status, button) in _statusCards)
        {
            var selected = string.Equals(status, selectedStatus, StringComparison.Ordinal);
            button.BackColor = selected ? Color.FromArgb(232, 240, 252) : Color.White;
            button.ForeColor = selected ? Accent : TextColor;
            button.FlatAppearance.BorderColor = selected
                ? Color.FromArgb(153, 183, 226)
                : Color.FromArgb(224, 229, 236);
        }
    }

    private async Task SearchOnEnterAsync(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            await RefreshTablesAsync();
        }
    }

    private void AddTextColumn(string propertyName, string title, int width)
    {
        _tableGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = propertyName,
            HeaderText = title,
            Width = width,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private void AddActionColumn(string name, string title, int width)
    {
        _tableGrid.Columns.Add(new DataGridViewButtonColumn
        {
            Name = name,
            HeaderText = title,
            Text = title,
            UseColumnTextForButtonValue = true,
            Width = width,
            FlatStyle = FlatStyle.Flat,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private void FormatStatusCell(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_tableGrid.Columns[e.ColumnIndex].DataPropertyName != nameof(DiningTable.Status) ||
            e.Value is not string status)
        {
            return;
        }

        if (e.CellStyle is not { } cellStyle)
        {
            return;
        }

        cellStyle.ForeColor = status switch
        {
            "Trống" => Color.FromArgb(36, 128, 79),
            "Đã đặt" => Color.FromArgb(49, 102, 171),
            "Đang phục vụ" => Color.FromArgb(174, 111, 28),
            _ => Color.FromArgb(100, 105, 117)
        };
        cellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    }

    private static Label CreateFilterLabel(string text) =>
        new()
        {
            Text = text,
            ForeColor = MutedText,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(4, 0, 4, 2)
        };

    private static void ConfigureFilterTextBox(TextBox textBox, string placeholder)
    {
        textBox.PlaceholderText = placeholder;
        textBox.Dock = DockStyle.Fill;
        StyleFilterControl(textBox);
    }

    private static void StyleFilterControl(Control control)
    {
        control.Margin = new Padding(4, 1, 8, 3);
        control.Font = new Font("Segoe UI", 9F);
        control.BackColor = Color.White;
        control.ForeColor = TextColor;
    }

    private static Button CreateSidebarButton(string text, bool selected)
    {
        var button = new Button
        {
            Text = $"●   {text}",
            Height = 43,
            Width = 205,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(19, 0, 0, 0),
            BackColor = selected ? Accent : Navy,
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private static Button CreatePrimaryButton(string text)
    {
        var button = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = Accent,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private static Button CreateSecondaryButton(string text)
    {
        var button = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = TextColor,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(216, 222, 231);
        return button;
    }

    private void ShowValidationError(ArgumentException exception)
    {
        MessageBox.Show(
            this,
            exception.Message,
            "Thông tin bàn chưa hợp lệ",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private void ShowDatabaseError(string message, DbException exception)
    {
        MessageBox.Show(
            this,
            $"{message}\n\nChi tiết: {exception.Message}",
            "Lỗi cơ sở dữ liệu",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
