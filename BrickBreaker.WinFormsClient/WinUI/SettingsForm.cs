using System;
using System.Drawing;
using System.Windows.Forms;

namespace BrickBreaker.WinFormsClient.WinUI;

public sealed class SettingsForm : Form
{
    private readonly TextBox _txtApiUrl = new();

    public SettingsForm(string currentUrl)
    {
        Text = "BrickBreaker Settings";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(420, 150);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var lblTitle = new Label
        {
            Text = "API base URL",
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        _txtApiUrl.Dock = DockStyle.Top;
        _txtApiUrl.Margin = new Padding(0, 6, 0, 6);
        _txtApiUrl.Text = currentUrl;

        var lblHint = new Label
        {
            Text = "Example: https://brickbreaker-api.example.com",
            AutoSize = true,
            ForeColor = SystemColors.GrayText
        };

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill
        };

        var btnSave = new Button
        {
            Text = "Save",
            DialogResult = DialogResult.OK
        };
        var btnCancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel
        };
        buttons.Controls.Add(btnSave);
        buttons.Controls.Add(btnCancel);

        layout.Controls.Add(lblTitle, 0, 0);
        layout.Controls.Add(_txtApiUrl, 0, 1);
        layout.Controls.Add(lblHint, 0, 2);
        layout.Controls.Add(buttons, 0, 3);

        Controls.Add(layout);

        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    public string ApiBaseAddress => _txtApiUrl.Text.Trim();
}
