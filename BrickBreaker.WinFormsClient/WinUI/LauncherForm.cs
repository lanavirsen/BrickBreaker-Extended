using BrickBreaker.Core.Clients;
using BrickBreaker.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrickBreaker.WinFormsClient.WinUI;

public partial class LauncherForm : Form
{
    private readonly LauncherShell _shell;

    public LauncherForm()
    {
        InitializeComponent();
        var defaultUrl = ApiConfiguration.ResolveBaseAddress();
        txtApiUrl.Text = defaultUrl;
        var apiClient = new GameApiClient(defaultUrl);
        _shell = new LauncherShell(apiClient);
        WireEventHandlers();
        ResetPlayer();
        ApplyState();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await RefreshLeaderboardAsync();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        _shell.Dispose();
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
            _shell.SetBaseAddress(value);
            txtApiUrl.Text = _shell.BaseAddress;
            ApplyState();
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

    private void EnableQuickPlay()
    {
        _shell.EnableQuickPlay();
        ApplyState();
    }

    private void ResetPlayer()
    {
        _shell.Logout();
        ApplyState();
    }

    private async Task StartGameAsync()
    {
        btnStartGame.Enabled = false;
        try
        {
            int finalScore = RunGame();
            _shell.RecordGameFinished(finalScore);
            ApplyState();

            if (_shell.CanSubmitScores && finalScore > 0)
            {
                var result = await _shell.SubmitScoreAsync(finalScore);
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
        finally
        {
            ApplyState();
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
        var result = await _shell.LoadLeaderboardAsync(10);
        btnRefreshLeaderboard.Enabled = true;
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

    private void RenderLeaderboard(IReadOnlyList<ScoreEntry> entries)
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
            _shell.UpdateStatus("Enter both username and password.");
            ApplyState();
            return null;
        }

        return (username, password);
    }

    private void ToggleAuthButtons(bool enabled)
    {
        btnLogin.Enabled = enabled;
        btnRegister.Enabled = enabled;
        btnQuickPlay.Enabled = enabled;
    }

    private void ApplyState()
    {
        var view = _shell.Snapshot();
        lblCurrentUser.Text = view.PlayerLabel;
        lblLastScore.Text = view.LastScoreLabel;
        btnStartGame.Enabled = view.CanStartGame;
        btnLogout.Enabled = view.CanLogout;
        lblStatus.Text = view.StatusMessage;
        lblStatus.ForeColor = view.StatusIsSuccess ? Color.ForestGreen : SystemColors.GrayText;
    }

    private void ShowError(string title, string? details)
    {
        var message = string.IsNullOrWhiteSpace(details) ? "An unknown error occurred." : details;
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        _shell.UpdateStatus(message);
        ApplyState();
    }
}
