using BrickBreaker.Core.Clients;
using BrickBreaker.Core.Models;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace BrickBreaker.WinFormsClient.WinUI;

// Main launcher window. The left sidebar handles authentication and shows the
// leaderboard; the right panel hosts Form1 as an embedded child so the game
// runs directly inside the launcher without a separate window.
public partial class LauncherForm : Form
{
    private readonly LauncherShell _shell;
    private readonly Form1 _gameView;

    // Four font sizes cover all sidebar text: pixel font for labels/buttons,
    // smaller pixel font for group-box titles, Consolas for the leaderboard
    // table, and larger Consolas for the username/password input fields.
    private Font _pixelFont = null!;
    private Font _pixelFontSm = null!;
    private Font _consolasFont = null!;
    private Font _consolasFontLg = null!;
    private readonly PrivateFontCollection _launcherFonts = new();

    // Leaderboard colour palette — dark alternating rows with a purple grid.
    private readonly Color _leaderboardBack = Color.FromArgb(15, 12, 32);
    private readonly Color _leaderboardAltBack = Color.FromArgb(20, 16, 40);
    private readonly Color _leaderboardGrid = Color.FromArgb(84, 66, 140);
    private readonly Color _leaderboardHeader = Color.FromArgb(54, 38, 94);

    private IReadOnlyList<ScoreEntry> _leaderboardEntries = Array.Empty<ScoreEntry>();

    // Loading spinner state — angle advances each timer tick while the
    // leaderboard fetch is in flight.
    private bool _leaderboardLoading;
    private float _loadingAngle;
    private readonly System.Windows.Forms.Timer _loadingTimer = new() { Interval = 60 };

    // Win32 API for setting a text margin inside a native TextBox control.
    // The WinForms Padding property is ignored by the native edit control,
    // so EM_SETMARGINS is the only reliable way to add left/right insets.
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    private static void SetTextBoxMargin(TextBox tb, int left)
    {
        const int EM_SETMARGINS = 0xD3;
        const int EC_LEFTMARGIN = 0x1;
        SendMessage(tb.Handle, EM_SETMARGINS, EC_LEFTMARGIN, left);
    }

    // Custom Panel subclass that owns its own painting. Prevents the system
    // from drawing a hover highlight over the leaderboard rows.
    private sealed class LeaderboardPanel : Panel
    {
        public LeaderboardPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }

    public LauncherForm()
    {
        InitializeComponent();
        LoadSidebarFonts();

        // Resolve the API base address and optional bypass token from environment
        // or defaults so the launcher works out of the box without configuration.
        var defaultUrl = ApiConfiguration.ResolveBaseAddress();
        var apiClient = new GameApiClient(defaultUrl, turnstileBypassToken: ApiConfiguration.ResolveBypassToken());
        _shell = new LauncherShell(apiClient);

        // Embed Form1 as a child control so the game renders inside the launcher.
        // TopLevel = false is required; Dock = Fill makes it fill the host panel.
        _gameView = new Form1(embedded: true)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        panelGameHost.Controls.Add(_gameView);
        _gameView.GameFinished += GameView_GameFinished;
        _gameView.Show();

        pnlLeaderboard.Paint += PnlLeaderboard_Paint;

        // Each tick advances the spinner angle and repaints the leaderboard panel.
        _loadingTimer.Tick += (_, _) =>
        {
            _loadingAngle = (_loadingAngle + 30f) % 360f;
            pnlLeaderboard.Invalidate();
        };

        WireEventHandlers();
        ResetPlayer();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // Clear focus so Space/Enter keypresses go to the game rather than
        // accidentally triggering whichever sidebar button is focused.
        ActiveControl = null;
        await RefreshLeaderboardAsync();
    }

    // Disposes all manually created resources. Font objects are not managed by
    // the Designer, so they must be released explicitly here.
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        _loadingTimer.Dispose();
        _shell.Dispose();
        _gameView.Dispose();
        _pixelFont.Dispose();
        _pixelFontSm.Dispose();
        _consolasFont.Dispose();
        _consolasFontLg.Dispose();
        _launcherFonts.Dispose();
    }

    // Loads PressStart2P from the Assets folder and applies it to every sidebar
    // control explicitly to prevent ambient font inheritance from propagating the
    // pixel font into controls (like the cog button) that should keep their own.
    // Falls back to Consolas if the TTF file is not found.
    private void LoadSidebarFonts()
    {
        string path = Path.Combine(Application.StartupPath, "Assets", "PressStart2P-Regular.ttf");
        if (File.Exists(path))
        {
            _launcherFonts.AddFontFile(path);
            _pixelFont = new Font(_launcherFonts.Families[0], 7f, FontStyle.Regular);
            _pixelFontSm = new Font(_launcherFonts.Families[0], 6f, FontStyle.Regular);
        }
        else
        {
            _pixelFont = new Font("Consolas", 8f, FontStyle.Regular);
            _pixelFontSm = new Font("Consolas", 7f, FontStyle.Regular);
        }

        _consolasFont = new Font("Consolas", 10f, FontStyle.Regular);
        _consolasFontLg = new Font("Consolas", 13f, FontStyle.Regular);

        // Labels.
        lblUsername.Font = _pixelFont;
        lblPassword.Font = _pixelFont;
        lblCurrentUser.Font = _pixelFont;
        lblLastScore.Font = _pixelFont;
        lblStatus.Font = _pixelFont;

        // Buttons.
        btnLogin.Font = _pixelFont;
        btnRegister.Font = _pixelFont;
        btnLogout.Font = _pixelFont;

        // Group box titles (slightly smaller to reduce clipping in the title area).
        groupAuth.Font = _pixelFontSm;
        groupLeaderboard.Font = _pixelFontSm;

        // Leaderboard table rows inherit font via pnlLeaderboard in PnlLeaderboard_Paint.
        pnlLeaderboard.Font = _consolasFont;

        // Input fields use a larger size so they stay readable for typing.
        txtUsername.Font = _consolasFontLg;
        txtPassword.Font = _consolasFontLg;
        SetTextBoxMargin(txtUsername, 6);
        SetTextBoxMargin(txtPassword, 6);
    }

    // Wires button clicks and allows Enter in the credential fields to submit login.
    private void WireEventHandlers()
    {
        btnLogin.Click += async (_, _) => await LoginAsync();
        btnRegister.Click += async (_, _) => await RegisterAsync();
        btnLogout.Click += (_, _) => ResetPlayer();
        btnSettings.Click += async (_, _) => await ShowSettingsAsync();

        async void OnCredentialKeyDown(object? s, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await LoginAsync();
            }
        }
        txtUsername.KeyDown += OnCredentialKeyDown;
        txtPassword.KeyDown += OnCredentialKeyDown;
    }

    // Validates fields, disables auth buttons during the request, and updates
    // the sidebar on success or shows an error dialog on failure.
    private async Task LoginAsync()
    {
        var credentials = ReadCredentials();
        if (credentials is null)
        {
            return;
        }

        ToggleAuthButtons(enabled: false);
        var result = await _shell.LoginAsync(credentials.Value.username, credentials.Value.password);
        ToggleAuthButtons(enabled: true);

        if (result.Success)
        {
            ApplyState();
        }
        else
        {
            ShowError("Login failed", result.Error);
        }
    }

    private async Task RegisterAsync()
    {
        var credentials = ReadCredentials();
        if (credentials is null)
        {
            return;
        }

        ToggleAuthButtons(enabled: false);
        var result = await _shell.RegisterAsync(credentials.Value.username, credentials.Value.password);
        ToggleAuthButtons(enabled: true);

        if (result.Success)
        {
            ApplyState();
        }
        else
        {
            ShowError("Registration failed", result.Error);
        }
    }

    // Logs out via the shell and refreshes the sidebar back to the default state.
    private void ResetPlayer()
    {
        _shell.Logout();
        ApplyState();
    }

    // Fetches the top-10 scores, showing a spinner while the request is in flight.
    private async Task RefreshLeaderboardAsync()
    {
        _leaderboardLoading = true;
        _leaderboardEntries = Array.Empty<ScoreEntry>();
        _loadingTimer.Start();

        var result = await _shell.LoadLeaderboardAsync(10);

        _loadingTimer.Stop();
        _leaderboardLoading = false;
        ApplyState();

        if (result.Success && result.Value is IReadOnlyList<ScoreEntry> entries)
        {
            RenderLeaderboard(entries);
        }
        else
        {
            ShowError("Failed to load leaderboard", result.Error);
        }
    }

    // Stores the latest entries and triggers a repaint of the leaderboard panel.
    private void RenderLeaderboard(IReadOnlyList<ScoreEntry> entries)
    {
        _leaderboardEntries = entries;
        pnlLeaderboard.Invalidate();
    }

    // Column definitions: proportional widths, headers, and alignment flags.
    private static readonly string[] _columnHeaders = { "#", "Player", "Score", "Achieved" };
    private static readonly float[] _columnWeights = { 0.08f, 0.35f, 0.22f, 0.35f };
    private static readonly bool[] _columnRightAlign = { false, false, true, false };

    // Draws a 12-dot circular spinner centred in the leaderboard panel.
    // Each dot fades from transparent to opaque to show rotation direction.
    private void DrawLoadingSpinner(Graphics g)
    {
        const int dotCount = 12;
        float cx = pnlLeaderboard.ClientRectangle.Width / 2f;
        float cy = pnlLeaderboard.ClientRectangle.Height / 2f;
        const float radius = 22f;
        const float dotRadius = 4f;

        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        for (int i = 0; i < dotCount; i++)
        {
            float angleRad = (i * (360f / dotCount) - _loadingAngle) * MathF.PI / 180f;
            float x = cx + radius * MathF.Cos(angleRad) - dotRadius;
            float y = cy + radius * MathF.Sin(angleRad) - dotRadius;
            int alpha = (int)(255f * (i + 1) / dotCount);
            using var brush = new SolidBrush(Color.FromArgb(alpha, 111, 224, 255));
            g.FillEllipse(brush, x, y, dotRadius * 2, dotRadius * 2);
        }
    }

    // Custom-drawn leaderboard table. Shows the spinner while loading; otherwise
    // draws a header row, alternating data rows, and purple grid lines.
    private void PnlLeaderboard_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        if (_leaderboardLoading)
        {
            DrawLoadingSpinner(g);
            return;
        }
        int width = pnlLeaderboard.ClientRectangle.Width;
        int height = pnlLeaderboard.ClientRectangle.Height;
        const int headerH = 26;
        const int rowH = 22;
        var font = pnlLeaderboard.Font ?? DefaultFont;

        // Calculate column x positions from proportional weights.
        int[] colX = new int[4];
        int[] colW = new int[4];
        int cx = 0;
        for (int i = 0; i < 3; i++)
        {
            colW[i] = (int)(width * _columnWeights[i]);
            colX[i] = cx;
            cx += colW[i];
        }
        // Last column takes the remaining width to avoid a gap at the right edge.
        colX[3] = cx;
        colW[3] = width - cx;

        // Header row.
        using var headerBrush = new SolidBrush(_leaderboardHeader);
        g.FillRectangle(headerBrush, 0, 0, width, headerH);
        for (int i = 0; i < 4; i++)
        {
            var flags = TextFormatFlags.VerticalCenter |
                        (_columnRightAlign[i] ? TextFormatFlags.Right : TextFormatFlags.Left);
            TextRenderer.DrawText(g, _columnHeaders[i], font,
                new Rectangle(colX[i] + 4, 0, colW[i] - 8, headerH), Color.White, flags);
        }

        // Data rows — alternating background colours for readability.
        for (int i = 0; i < _leaderboardEntries.Count; i++)
        {
            var entry = _leaderboardEntries[i];
            int y = headerH + i * rowH;
            var backColor = i % 2 == 0 ? _leaderboardBack : _leaderboardAltBack;
            using var rowBrush = new SolidBrush(backColor);
            g.FillRectangle(rowBrush, 0, y, width, rowH);

            string[] cells =
            {
                (i + 1).ToString(),
                entry.Username,
                entry.Score.ToString(),
                entry.At.ToLocalTime().ToString("d"),
            };
            for (int j = 0; j < 4; j++)
            {
                var flags = TextFormatFlags.VerticalCenter |
                            (_columnRightAlign[j] ? TextFormatFlags.Right : TextFormatFlags.Left);
                TextRenderer.DrawText(g, cells[j], font,
                    new Rectangle(colX[j] + 4, y, colW[j] - 8, rowH), Color.White, flags);
            }
        }

        // Grid lines — column separators and a line beneath the header.
        using var gridPen = new Pen(_leaderboardGrid);
        g.DrawLine(gridPen, 0, headerH, width, headerH);
        for (int i = 1; i < 4; i++)
        {
            g.DrawLine(gridPen, colX[i], 0, colX[i], height);
        }
    }

    // Reads and trims the credential fields. Returns null and shows a message
    // if either field is empty, leaving the caller to bail out early.
    private (string username, string password)? ReadCredentials()
    {
        var username = txtUsername.Text.Trim();
        var password = txtPassword.Text;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show(this, "Enter both username and password.", "BrickBreaker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _shell.UpdateStatus("Enter both username and password.");
            ApplyState();
            return null;
        }

        return (username, password);
    }

    // Disables both auth buttons during async operations to prevent double-submit.
    private void ToggleAuthButtons(bool enabled)
    {
        btnLogin.Enabled = enabled;
        btnRegister.Enabled = enabled;
    }

    // Reads the latest shell snapshot and updates all sidebar controls to reflect
    // the current session state. Auth buttons dim when a user is logged in so it
    // is clear that Login and Register are not available until sign-out.
    private void ApplyState()
    {
        var view = _shell.Snapshot();
        lblCurrentUser.Text = view.PlayerLabel;
        lblLastScore.Text = view.BestScoreLabel;

        bool loggedIn = view.CanLogout;

        btnLogout.ForeColor = loggedIn ? Color.White : Color.FromArgb(80, 80, 80);
        btnLogout.BackColor = loggedIn
            ? Color.FromArgb(58, 41, 102)
            : Color.FromArgb(28, 22, 50);

        btnLogin.ForeColor = loggedIn ? Color.FromArgb(80, 80, 80) : Color.White;
        btnLogin.BackColor = loggedIn
            ? Color.FromArgb(28, 22, 50)
            : Color.FromArgb(58, 41, 102);

        btnRegister.ForeColor = loggedIn ? Color.FromArgb(80, 80, 80) : Color.White;
        btnRegister.BackColor = loggedIn
            ? Color.FromArgb(28, 22, 50)
            : Color.FromArgb(58, 41, 102);

        lblStatus.Text = view.StatusMessage;
        lblStatus.ForeColor = view.StatusIsSuccess ? Color.FromArgb(111, 224, 255) : SystemColors.GrayText;
    }

    // Shows a modal error dialog and mirrors the message into the status bar.
    private void ShowError(string title, string? details)
    {
        var message = string.IsNullOrWhiteSpace(details) ? "An unknown error occurred." : details;
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        _shell.UpdateStatus(message);
        ApplyState();
    }

    // Raised by the embedded game when a session ends. Updates the sidebar and
    // auto-submits the score if the player is logged in and scored above zero.
    private async void GameView_GameFinished(object? sender, int score)
    {
        _shell.RecordGameFinished(score);
        ApplyState();
        // Clear focus so Space can restart the game without triggering a button.
        ActiveControl = null;

        if (_shell.CanSubmitScores && score > 0)
        {
            var result = await _shell.SubmitScoreAsync(score);
            ApplyState();
            if (result.Success)
            {
                await RefreshLeaderboardAsync();
            }
            else
            {
                ShowError("Score submission failed", result.Error);
            }
        }
    }

    // Opens the settings dialog. If the user confirms, applies the new API base
    // address and refreshes the leaderboard against the updated endpoint.
    private async Task ShowSettingsAsync()
    {
        using var dialog = new SettingsForm(_shell.BaseAddress);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                _shell.SetBaseAddress(dialog.ApiBaseAddress);
                ApplyState();
                await RefreshLeaderboardAsync();
            }
            catch (Exception ex)
            {
                ShowError("Failed to update API URL.", ex.Message);
            }
        }
    }
}
