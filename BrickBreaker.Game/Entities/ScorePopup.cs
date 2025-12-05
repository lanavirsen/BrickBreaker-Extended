namespace BrickBreaker.Game.Entities;

public class ScorePopup
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Value { get; }
    public int Lifetime { get; } = 30;
    public int Age { get; private set; }
    public bool IsAlive => Age < Lifetime;
    public bool IsMultiplier => _customText is not null;
    public string DisplayText => _customText ?? $"+{Value}";
    public float Opacity
    {
        get
        {
            if (Age > Lifetime - 10)
            {
                var remaining = Math.Max(0, Lifetime - Age);
                return remaining / 10f;
            }
            return 1f;
        }
    }

    private readonly string? _customText;
    private readonly float _riseSpeed = 2f;

    public ScorePopup(int x, int y, int value)
    {
        X = x;
        Y = y;
        Value = value;
        _customText = null;
    }

    public ScorePopup(int x, int y, string text)
    {
        X = x;
        Y = y;
        Value = 0;
        _customText = text;
    }

    public void Update()
    {
        Y -= (int)_riseSpeed;
        Age++;
    }

    public void Shift(int dx, int dy)
    {
        X += dx;
        Y += dy;
    }
}

