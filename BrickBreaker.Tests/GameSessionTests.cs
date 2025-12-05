using System.Drawing;
using BrickBreaker.Gameplay;

namespace BrickBreaker.Tests;

public sealed class GameSessionTests
{
    [Fact]
    public void Initialize_SetsSnapshotAndFlags()
    {
        var session = new GameSession();
        var area = new Rectangle(50, 100, 400, 600);

        session.Initialize(area);

        Assert.Equal(area, session.Snapshot.PlayArea);
        Assert.True(session.Snapshot.BallReady);
        Assert.False(session.Snapshot.IsPaused);
        Assert.Equal(1, session.Snapshot.Level);
        Assert.Equal(0, session.Snapshot.Score);
    }

    [Fact]
    public void Update_RespectsInputAndMovesPaddle()
    {
        var session = new GameSession();
        session.Initialize(new Rectangle(0, 0, 600, 800));
        var initialX = session.Snapshot.PaddleX;

        session.SetInput(leftPressed: true, rightPressed: false);
        session.Update(0.016);

        Assert.True(session.Snapshot.PaddleX < initialX);
    }

    [Fact]
    public void UpdatePlayArea_RepositionsWorld()
    {
        var session = new GameSession();
        session.Initialize(new Rectangle(0, 0, 400, 600));
        var newArea = new Rectangle(25, 35, 400, 600);

        session.UpdatePlayArea(newArea);

        Assert.Equal(newArea, session.Snapshot.PlayArea);
        Assert.True(session.Snapshot.PaddleX >= newArea.Left);
        Assert.Equal(newArea.Bottom - 40, session.Snapshot.PaddleY);
    }
}
