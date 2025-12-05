using BrickBreaker.WinFormsClient.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrickBreaker.WinFormsClient.WinUI;

public partial class LauncherForm : Form
{
    private readonly BrickBreakerApiClient _apiClient;
    private string? _currentPlayer;
    private bool _quickPlayMode;

    public LauncherForm()
    {
        InitializeComponent();
        var defaultUrl = Environment.GetEnvironmentVariable("BRICKBREAKER_API_URL") ?? "http://localhost:5080";
        txtApiUrl.Text = defaultUrl;
        _apiClient = new BrickBreakerApiClient(defaultUrl);
        WireEventHandlers();
        ResetPlayer();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await RefreshLeaderboardAsync();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        _apiClient.Dispose();
    }

    private void WireEventHandlers()
    {
        btnApplyApi.Click += async (_, _) => await ApplyApiUrlAsync();
        btnLogin.Click += async (_, _) => await LoginAsync();
        btnRegister.Click += async (_, _) => await RegisterAsync();
        btnQuickPlay.Click += (_, _) => EnableQuickPlay();
        btnStartGame.Click += async (_, _) => await StartGameAsync();
        btnRefreshLeaderboard.Click += async (_, _) => await RefreshLeaderboardAsync();
        btnLogout.Click += (_, _) => ResetPlayer();
    }

    private async Task ApplyApiUrlAsync()
    {
        var value = txtApiUrl.Text.Trim();
        try
        {
            _apiClient.SetBaseAddress(value);
            SetStatus($"API base updated to {value}", success: true);
            await RefreshLeaderboardAsync();
        }
        catch (Exception ex)
        {
            ShowError("Failed to update API URL.", ex.Message);
        }
    }

    private async Task LoginAsync()
    {
        var credentials = ReadCredentials();
        if (credentials is null)
        {
            return;
        }

        ToggleAuthButtons(enabled: false);
        SetStatus("Signing in...");
        var result = await _apiClient.LoginAsync(credentials.Value.username, credentials.Value.password);
        ToggleAuthButtons(enabled: true);

        if (result.Success)
        {
            UpdatePlayer(credentials.Value.username, quickPlay: false);
            SetStatus("Logged in.", success: true);
        }
        else
        {
            ShowError("Login failed", result.ErrorMessage);
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
        SetStatus("Registering account...");
        var result = await _apiClient.RegisterAsync(credentials.Value.username, credentials.Value.password);
        ToggleAuthButtons(enabled: true);

        if (result.Success)
        {
            SetStatus("Registration successful. You can log in now.", success: true);
        }
        else
        {
            ShowError("Registration failed", result.ErrorMessage);
        }
    }

    private void EnableQuickPlay()
    {
        _quickPlayMode = true;
        _currentPlayer = null;
        btnStartGame.Enabled = true;
        btnLogout.Enabled = false;
        lblCurrentUser.Text = "Mode: Quick Play";
        SetStatus("Quick Play ready. Scores are not submitted.", success: true);
    }

    private void ResetPlayer()
    {
        _quickPlayMode = false;
        _currentPlayer = null;
        btnStartGame.Enabled = false;
        btnLogout.Enabled = false;
        lblCurrentUser.Text = "Player: (not logged in)";
        lblLastScore.Text = "Last score: none";
        SetStatus("Signed out.");
    }

    private async Task StartGameAsync()
    {
        btnStartGame.Enabled = false;
        try
        {
            int finalScore = RunGame();
            lblLastScore.Text = finalScore > 0 ? $"Last score: {finalScore}" : "Last score: none";

            if (!_quickPlayMode && !string.IsNullOrWhiteSpace(_currentPlayer) && finalScore > 0)
            {
                SetStatus("Submitting score...");
                var result = await _apiClient.SubmitScoreAsync(_currentPlayer!, finalScore);
                if (result.Success)
                {
                    SetStatus("Score submitted!", success: true);
                    await RefreshLeaderboardAsync();
                }
                else
                {
                    ShowError("Score submission failed", result.ErrorMessage);
                }
            }
            else if (_quickPlayMode)
            {
                SetStatus("Quick Play finished.");
            }
        }
        finally
        {
            btnStartGame.Enabled = _quickPlayMode || !string.IsNullOrWhiteSpace(_currentPlayer);
        }
    }

    private int RunGame()
    {
        int finalScore = 0;

        using var form = new Form1
        {
            CloseOnGameOver = true
        };

        form.GameFinished += (_, score) => finalScore = score;
        form.ShowDialog(this);

        return finalScore;
    }

    private async Task RefreshLeaderboardAsync()
    {
        btnRefreshLeaderboard.Enabled = false;
        var result = await _apiClient.GetLeaderboardAsync(10);
        btnRefreshLeaderboard.Enabled = true;

        if (result.Success && result.Value is IReadOnlyList<LeaderboardEntry> entries)
        {
            RenderLeaderboard(entries);
            SetStatus($"Leaderboard updated at {DateTime.Now:T}", success: true);
        }
        else
        {
            ShowError("Failed to load leaderboard", result.ErrorMessage);
        }
    }

    private void RenderLeaderboard(IReadOnlyList<LeaderboardEntry> entries)
    {
        listLeaderboard.BeginUpdate();
        listLeaderboard.Items.Clear();

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var item = new ListViewItem((i + 1).ToString());
            item.SubItems.Add(entry.Username);
            item.SubItems.Add(entry.Score.ToString());
            item.SubItems.Add(entry.At.ToLocalTime().ToString("g"));
            listLeaderboard.Items.Add(item);
        }

        listLeaderboard.EndUpdate();
    }

    private (string username, string password)? ReadCredentials()
    {
        var username = txtUsername.Text.Trim();
        var password = txtPassword.Text;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show(this, "Enter both username and password.", "BrickBreaker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        return (username, password);
    }

    private void UpdatePlayer(string username, bool quickPlay)
    {
        _currentPlayer = username;
        _quickPlayMode = quickPlay;
        btnStartGame.Enabled = true;
        btnLogout.Enabled = true;
        lblCurrentUser.Text = $"Player: {username}";
    }

    private void ToggleAuthButtons(bool enabled)
    {
        btnLogin.Enabled = enabled;
        btnRegister.Enabled = enabled;
        btnQuickPlay.Enabled = enabled;
    }

    private void SetStatus(string message, bool success = false)
    {
        lblStatus.Text = message;
        lblStatus.ForeColor = success ? Color.ForestGreen : SystemColors.GrayText;
    }

    private void ShowError(string title, string? details)
    {
        var message = string.IsNullOrWhiteSpace(details) ? "An unknown error occurred." : details;
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        SetStatus(message);
    }
}
