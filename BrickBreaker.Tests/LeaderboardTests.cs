using System.Collections.Generic;
using System.Threading.Tasks;
using BrickBreaker.Core.Models;
using BrickBreaker.Core.Services;

namespace BrickBreaker.Tests;

public sealed class LeaderboardTests
{
    [Fact]
    public async Task SubmitEntry_ForwardsEntryToStore()
    {
        var store = new FakeLeaderboardStore();
        var sut = new LeaderboardService(store);
        var entry = new ScoreEntry("Alice", 42, DateTimeOffset.Parse("2024-05-01T12:00:00+00:00"));

        await sut.SubmitAsync(entry);

        Assert.Same(entry, Assert.Single(store.AddedEntries));
    }

    [Fact]
    public async Task Submit_WithUsernameAndScore_IgnoresInvalidInputs()
    {
        // Reject empty usernames and negative scores.
        var store = new FakeLeaderboardStore();
        var sut = new LeaderboardService(store);

        await sut.SubmitAsync(" ", 10);
        await sut.SubmitAsync("Alice", -1);

        Assert.Empty(store.AddedEntries);
    }

    [Fact]
    public async Task Submit_WithUsernameAndScore_TrimsNameAndAddsTimestamp()
    {
        // Names should be trimmed and timestamps should be set by the method.
        var store = new FakeLeaderboardStore();
        var sut = new LeaderboardService(store);

        await sut.SubmitAsync("  Alice  ", 100);

        var submitted = Assert.Single(store.AddedEntries);
        Assert.Equal("Alice", submitted.Username);
        Assert.Equal(100, submitted.Score);
        Assert.True((DateTimeOffset.UtcNow - submitted.At).TotalSeconds < 5);
    }

    [Fact]
    public async Task Top_FiltersInvalidEntriesAndSortsByScoreDateAndName()
    {
        // Entries with blanks/negative scores are removed; ordering is Score desc, At asc, Username asc.
        var store = new FakeLeaderboardStore
        {
            EntriesToReturn = new List<ScoreEntry>
            {
                new("Alice", 200, DateTimeOffset.Parse("2024-05-01T12:00:00+00:00")),
                new("  ", 50, DateTimeOffset.Parse("2024-05-01T12:00:00+00:00")),
                new("Bob", -5, DateTimeOffset.Parse("2024-05-01T12:00:00+00:00")),
                new("bob", 150, DateTimeOffset.Parse("2024-05-02T12:00:00+00:00")),
                new("Charlie", 200, DateTimeOffset.Parse("2024-04-01T12:00:00+00:00")),
                new("alice", 200, DateTimeOffset.Parse("2024-05-01T10:00:00+00:00")),
            }
        };
        var sut = new LeaderboardService(store);

        var top = await sut.TopAsync(3);

        Assert.Collection(top,
            entry => Assert.Equal("Charlie", entry.Username), // earliest timestamp overall
            entry => Assert.Equal("alice", entry.Username),   // next earliest timestamp at same score
            entry => Assert.Equal("Alice", entry.Username));  // most recent of the 200 scores
    }

    [Fact]
    public async Task BestFor_ReturnsBestScoreIgnoringCase()
    {
        // Case-insensitive comparisons should find the overall best score.
        var store = new FakeLeaderboardStore
        {
            EntriesToReturn = new List<ScoreEntry>
            {
                new("Alice", 100, DateTimeOffset.Parse("2024-01-01T00:00:00+00:00")),
                new("alice", 150, DateTimeOffset.Parse("2024-01-02T00:00:00+00:00")),
                new("ALICE", 150, DateTimeOffset.Parse("2024-01-01T12:00:00+00:00")),
                new("Bob", 300, DateTimeOffset.Parse("2024-01-01T00:00:00+00:00")),
            }
        };
        var sut = new LeaderboardService(store);

        var best = await sut.BestForAsync("ALICE");

        Assert.NotNull(best);
        Assert.Equal(150, best!.Score);
        Assert.Equal(DateTimeOffset.Parse("2024-01-01T12:00:00+00:00"), best.At);
    }

    [Fact]
    public async Task BestFor_ReturnsNull_WhenUserHasNoEntries()
    {
        // When no matching entries exist, the result should be null.
        var store = new FakeLeaderboardStore
        {
            EntriesToReturn = new List<ScoreEntry>
            {
                new("Bob", 25, DateTimeOffset.Now)
            }
        };
        var sut = new LeaderboardService(store);

        var best = await sut.BestForAsync("Alice");

        Assert.Null(best);
    }
}
