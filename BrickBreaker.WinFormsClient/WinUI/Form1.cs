using System.ComponentModel;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using BrickBreaker.Game.Utilities;
using BrickBreaker.Gameplay;
using BrickBreaker.WinFormsClient.Rendering;

namespace BrickBreaker.WinFormsClient.WinUI;

// Standalone and embedded game form. In standalone mode it occupies the full
// primary screen with no border and handles keyboard events directly. In embedded
// mode it sits inside LauncherForm and polls raw key state via Win32 each tick
// because WinForms key routing is unreliable across parent/child form boundaries.
public partial class Form1 : Form
{
    private readonly GameSession _session = new();
    private readonly GameRenderer _renderer = new();
    private readonly System.Windows.Forms.Timer _gameTimer = new();

    // Held-key flags for smooth left/right movement in standalone mode.
    private bool _leftPressed;
    private bool _rightPressed;

    // Font objects created from the custom TTF (or system fallbacks) and bundled
    // into a FontResources record passed to the renderer each frame.
    private FontResources _fonts = null!;
    private Font _fontScore = SystemFonts.DefaultFont;
    private Font _fontMultiplier = SystemFonts.DefaultFont;
    private Font _fontCurrentLevel = SystemFonts.DefaultFont;
    private Font _fontLaunch = SystemFonts.DefaultFont;
    private Font _fontGameOver = SystemFonts.DefaultFont;
    private Font _fontTitle = SystemFonts.DefaultFont;
    private readonly PrivateFontCollection _fontCollection = new();

    private readonly Color _paddleColor = Color.FromArgb(36, 162, 255);
    private readonly bool _embedded;

    // Previous-frame key states used for edge detection in embedded polling mode
    // so discrete actions (launch, pause, restart) fire once per key press.
    private bool _prevLaunchKey;
    private bool _prevPauseKey;
    private bool _prevRestartKey;

    public event EventHandler<int>? GameFinished;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public bool CloseOnGameOver { get; set; }

    public int LatestScore => _session.Snapshot.Score;

    public Form1(bool embedded = false)
    {
        _embedded = embedded;
        InitializeComponent();
        LoadFonts();
        InitializeFormSettings();
        _session.GameFinished += (_, score) => HandleGameFinished(score);
        _session.Initialize(CalculatePlayAreaRect());
        _session.SetInput(false, false);

        // ~60 FPS tick rate. The interval is also used as the physics delta so
        // game speed stays consistent regardless of actual frame delivery time.
        _gameTimer.Interval = 16;
        _gameTimer.Tick += (_, _) => OnTick();
        _gameTimer.Start();
    }

    // Win32 API for reading physical key state regardless of focus.
    // Used only in embedded mode where WinForms key routing is unreliable.
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static bool IsKeyDown(Keys key) => (GetAsyncKeyState((int)key) & 0x8000) != 0;

    private void OnTick()
    {
        if (_embedded)
        {
            PollEmbeddedInput();
        }

        double delta = _gameTimer.Interval / 1000.0;
        _session.Update(delta);
        Invalidate();
    }

    // Polls keyboard state directly each frame so the game responds to input
    // regardless of which WinForms control currently has focus.
    private void PollEmbeddedInput()
    {
        _session.SetInput(
            IsKeyDown(Keys.Left),
            IsKeyDown(Keys.Right));

        // Launch ball — trigger once per key press (edge detection).
        bool launchKey = IsKeyDown(Keys.Up);
        if (launchKey && !_prevLaunchKey)
            _session.TryLaunchBall();
        _prevLaunchKey = launchKey;

        // Pause — toggle once per press.
        bool pauseKey = IsKeyDown(Keys.Space) || IsKeyDown(Keys.Escape);
        if (pauseKey && !_prevPauseKey)
            _session.TogglePause();
        _prevPauseKey = pauseKey;

        // Restart — only when game is over.
        bool restartKey = IsKeyDown(Keys.Return) && _session.Snapshot.IsGameOver;
        if (restartKey && !_prevRestartKey)
            RestartGame();
        _prevRestartKey = restartKey;
    }

    // Applies mode-specific window settings and wires the Paint handler.
    // Embedded mode uses explicit double-buffer styles since the form is hosted
    // inside another form where DoubleBuffered alone may not prevent flicker.
    private void InitializeFormSettings()
    {
        DoubleBuffered = true;
        if (_embedded)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.FromArgb(3, 3, 10);
        }
        else
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            Bounds = GetPrimaryBounds();
            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
        }

        Paint += (_, e) => _renderer.Draw(e.Graphics, _session.Snapshot, _fonts, ClientRectangle, _paddleColor);
    }

    // Falls back to the control's current screen if the primary screen is unavailable.
    private Rectangle GetPrimaryBounds()
        => Screen.PrimaryScreen?.Bounds ?? Screen.FromControl(this).Bounds;

    // Centres the brick grid horizontally and pushes it slightly down to leave
    // room for the "BRICK BREAKER" title rendered above the play area.
    private Rectangle CalculatePlayAreaRect()
    {
        int boardW = (GameConstants.InitialBrickCols - 1) * GameConstants.BrickXSpacing + GameConstants.BrickWidth;
        int boardH = (GameConstants.InitialBrickRows - 1) * GameConstants.BrickYSpacing + GameConstants.BrickHeight + GameConstants.PaddleAreaHeight;
        int startX = (ClientSize.Width - boardW) / 2;
        const int topHeaderHeight = 100; // BRICK BREAKER title sits ~100px above the play area
        int startY = (ClientSize.Height - boardH + topHeaderHeight) / 2;

        return new Rectangle(
            startX - GameConstants.PlayAreaMargin,
            startY - GameConstants.PlayAreaMargin,
            boardW + GameConstants.PlayAreaMargin * 2,
            boardH + GameConstants.PlayAreaMargin
        );
    }

    // Called when the session raises GameFinished. In embedded mode the timer
    // keeps running so the polling loop can still detect the restart key press.
    // CloseOnGameOver is used by the launcher to auto-close after a session ends.
    private void HandleGameFinished(int score)
    {
        if (!_embedded)
        {
            _gameTimer.Stop();
        }

        GameFinished?.Invoke(this, score);

        if (CloseOnGameOver)
        {
            _gameTimer.Stop();
            BeginInvoke(new MethodInvoker(Close));
        }
    }

    public void RestartGame()
    {
        _session.Restart();
        _session.SetInput(_leftPressed, _rightPressed);
        _gameTimer.Start();
    }

    // Toggles between borderless fullscreen and a fixed 1000×900 windowed mode.
    // Recalculates the play area so the brick grid re-centres after resize.
    private void ToggleFullscreen()
    {
        if (_embedded)
        {
            return;
        }

        if (WindowState == FormWindowState.Maximized)
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            WindowState = FormWindowState.Normal;
            Size = new Size(1000, 900);
            CenterToScreen();
        }
        else
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            Bounds = GetPrimaryBounds();
        }

        _session.UpdatePlayArea(CalculatePlayAreaRect());
        Invalidate();
    }

    // Standalone keyboard handler. Arrow keys and WASD both move the paddle;
    // F toggles fullscreen; Space pauses; Enter restarts after game over.
    private void Form1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape) Application.Exit();
        if (e.KeyCode == Keys.F) ToggleFullscreen();
        if (e.KeyCode == Keys.Space) _session.TogglePause();

        if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
        {
            _leftPressed = true;
            _session.SetInput(_leftPressed, _rightPressed);
        }

        if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
        {
            _rightPressed = true;
            _session.SetInput(_leftPressed, _rightPressed);
        }

        if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
            _session.TryLaunchBall();

        if (e.KeyCode == Keys.Return && _session.Snapshot.IsGameOver)
            RestartGame();
    }

    private void Form1_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
        {
            _leftPressed = false;
            _session.SetInput(_leftPressed, _rightPressed);
        }

        if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
        {
            _rightPressed = false;
            _session.SetInput(_leftPressed, _rightPressed);
        }
    }

    // Loads PressStart2P from the Assets folder if present; falls back to system
    // fonts so the game is still playable on machines without the TTF installed.
    private void LoadFonts()
    {
        string path = Path.Combine(Application.StartupPath, "Assets", "PressStart2P-Regular.ttf");
        if (File.Exists(path))
        {
            _fontCollection.AddFontFile(path);
            FontFamily family = _fontCollection.Families[0];
            _fontScore = new Font(family, 12, FontStyle.Regular);
            _fontMultiplier = new Font(family, 12, FontStyle.Regular);
            _fontCurrentLevel = new Font(family, 12, FontStyle.Regular);
            _fontLaunch = new Font(family, 10, FontStyle.Regular);
            _fontGameOver = new Font(family, 15, FontStyle.Bold);
            _fontTitle = new Font(family, 18, FontStyle.Bold);
        }
        else
        {
            _fontScore = new Font("Consolas", 18, FontStyle.Bold);
            _fontMultiplier = new Font("Consolas", 18, FontStyle.Bold);
            _fontCurrentLevel = new Font("Arial", 12, FontStyle.Bold);
            _fontLaunch = new Font("Arial", 16, FontStyle.Bold);
            _fontGameOver = new Font("Arial", 20, FontStyle.Bold);
            _fontTitle = new Font("Arial", 22, FontStyle.Bold);
        }

        _fonts = new FontResources(
            _fontScore,
            _fontMultiplier,
            _fontCurrentLevel,
            _fontLaunch,
            _fontGameOver,
            _fontTitle);
    }

    // Recalculates the play area whenever the window is resized so the brick
    // grid stays centred in both fullscreen and windowed modes.
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        _session.UpdatePlayArea(CalculatePlayAreaRect());
    }
}
