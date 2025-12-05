using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using BrickBreaker.Game.Utilities;
using BrickBreaker.Gameplay;
using BrickBreaker.WinFormsClient.Rendering;

namespace BrickBreaker.WinFormsClient.WinUI;

public partial class Form1 : Form
{
    private readonly GameSession _session = new();
    private readonly GameRenderer _renderer = new();
    private readonly System.Windows.Forms.Timer _gameTimer = new();
    private Rectangle _playAreaRect;
    private bool _leftPressed;
    private bool _rightPressed;
    private FontResources _fonts = null!;
    private Font _fontScore = SystemFonts.DefaultFont;
    private Font _fontMultiplier = SystemFonts.DefaultFont;
    private Font _fontCurrentLevel = SystemFonts.DefaultFont;
    private Font _fontTime = SystemFonts.DefaultFont;
    private Font _fontLaunch = SystemFonts.DefaultFont;
    private Font _fontGameOver = SystemFonts.DefaultFont;
    private Font _fontTitle = SystemFonts.DefaultFont;
    private readonly PrivateFontCollection _fontCollection = new();
    private readonly Color _paddleColor = Color.FromArgb(36, 162, 255);

    public event EventHandler<int>? GameFinished;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public bool CloseOnGameOver { get; set; }

    public int LatestScore => _session.Snapshot.Score;

    public Form1()
    {
        InitializeComponent();
        LoadFonts();
        InitializeFormSettings();
        _playAreaRect = CalculatePlayAreaRect();
        _session.GameFinished += (_, score) => HandleGameFinished(score);
        _session.Initialize(_playAreaRect);
        _session.SetInput(false, false);

        _gameTimer.Interval = 16;
        _gameTimer.Tick += (_, _) => OnTick();
        _gameTimer.Start();
    }

    private void OnTick()
    {
        double delta = _gameTimer.Interval / 1000.0;
        _session.Update(delta);
        Invalidate();
    }

    private void InitializeFormSettings()
    {
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        Bounds = GetPrimaryBounds();
        DoubleBuffered = true;
        Paint += (_, e) => _renderer.Draw(e.Graphics, _session.Snapshot, _fonts, ClientRectangle, _paddleColor);
        KeyDown += Form1_KeyDown;
        KeyUp += Form1_KeyUp;
    }

    private Rectangle GetPrimaryBounds()
        => Screen.PrimaryScreen?.Bounds ?? Screen.FromControl(this).Bounds;

    private Rectangle CalculatePlayAreaRect()
    {
        int boardW = (GameConstants.InitialBrickCols - 1) * GameConstants.BrickXSpacing + GameConstants.BrickWidth;
        int boardH = (GameConstants.InitialBrickRows - 1) * GameConstants.BrickYSpacing + GameConstants.BrickHeight + GameConstants.PaddleAreaHeight;
        int startX = (ClientSize.Width - boardW) / 2;
        int startY = (ClientSize.Height - boardH) / 2;

        return new Rectangle(
            startX - GameConstants.PlayAreaMargin,
            startY - GameConstants.PlayAreaMargin,
            boardW + GameConstants.PlayAreaMargin * 2,
            boardH + GameConstants.PlayAreaMargin
        );
    }

    private void HandleGameFinished(int score)
    {
        _gameTimer.Stop();
        GameFinished?.Invoke(this, score);
        if (CloseOnGameOver)
        {
            BeginInvoke(new MethodInvoker(Close));
        }
    }

    private void RestartGame()
    {
        _session.Restart();
        _session.SetInput(_leftPressed, _rightPressed);
        _gameTimer.Start();
    }

    private void ToggleFullscreen()
    {
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

        var newRect = CalculatePlayAreaRect();
        _playAreaRect = newRect;
        _session.UpdatePlayArea(newRect);
        Invalidate();
    }

    private void Form1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape) Application.Exit();
        if (e.KeyCode == Keys.F) ToggleFullscreen();
        if (e.KeyCode == Keys.P) _session.TogglePause();

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

        if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.W))
        {
            _session.TryLaunchBall();
        }

        if (e.KeyCode == Keys.Space && _session.Snapshot.IsGameOver)
        {
            RestartGame();
        }
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
            _fontTime = new Font(family, 12, FontStyle.Regular);
            _fontLaunch = new Font(family, 10, FontStyle.Regular);
            _fontGameOver = new Font(family, 15, FontStyle.Bold);
            _fontTitle = new Font(family, 18, FontStyle.Bold);
        }
        else
        {
            _fontScore = new Font("Consolas", 18, FontStyle.Bold);
            _fontMultiplier = new Font("Consolas", 18, FontStyle.Bold);
            _fontCurrentLevel = new Font("Arial", 12, FontStyle.Bold);
            _fontTime = new Font("Consolas", 18, FontStyle.Bold);
            _fontLaunch = new Font("Arial", 16, FontStyle.Bold);
            _fontGameOver = new Font("Arial", 20, FontStyle.Bold);
            _fontTitle = new Font("Arial", 22, FontStyle.Bold);
        }

        _fonts = new FontResources(
            _fontScore,
            _fontMultiplier,
            _fontCurrentLevel,
            _fontTime,
            _fontLaunch,
            _fontGameOver,
            _fontTitle);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        var newRect = CalculatePlayAreaRect();
        _playAreaRect = newRect;
        _session.UpdatePlayArea(newRect);
    }
}
