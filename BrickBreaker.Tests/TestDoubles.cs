// A fake in-memory implementation of IUserStore interface to simulate user data storage and retrieval in tests.
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Models;

internal sealed class FakeUserStore : IUserStore
{
    // Dictionary holding users keyed by username (case-insensitive).
    private readonly Dictionary<string, User> _users = new(StringComparer.OrdinalIgnoreCase);

    // List of users that were added during the test run.
    public List<User> AddedUsers { get; } = new();

    // Tracks the last username checked for existence, useful for verifying test calls.
    public string? LastExistsUsername { get; private set; }

    // Tracks the last username requested by the Get method.
    public string? LastGetUsername { get; private set; }

    // Seeds initial users into the store for test setup.
    public void Seed(User user)
    {
        if (!string.IsNullOrWhiteSpace(user?.Username))
        {
            _users[user.Username.Trim()] = user;
        }
    }

    // Checks if a username exists in the store.
    public Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        LastExistsUsername = username;
        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.FromResult(false);
        }
        return Task.FromResult(_users.ContainsKey(username.Trim()));
    }

    // Adds a user to the store and tracks the addition.
    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        AddedUsers.Add(user);
        if (!string.IsNullOrWhiteSpace(user.Username))
        {
            _users[user.Username.Trim()] = user;
        }
        return Task.CompletedTask;
    }

    // Retrieves a user by username if present, otherwise returns null.
    public Task<User?> GetAsync(string username, CancellationToken cancellationToken = default)
    {
        LastGetUsername = username;
        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.FromResult<User?>(null);
        }
        return Task.FromResult(_users.TryGetValue(username.Trim(), out var user) ? user : null);
    }
}

// Fake in-memory leaderboard store for testing leaderboard persistence and retrieval.
internal sealed class FakeLeaderboardStore : ILeaderboardStore
{
    // List of score entries added during tests.
    public List<ScoreEntry> AddedEntries { get; } = new();

    // List of score entries to return for read/readall operations (simulating stored data).
    public List<ScoreEntry> EntriesToReturn { get; set; } = new();

    // Adds a score entry to the internal list.
    public Task AddAsync(ScoreEntry entry, CancellationToken cancellationToken = default)
    {
        AddedEntries.Add(entry);
        return Task.CompletedTask;
    }

    // returns a copy of the entries to simulate retrieving leaderboard data.
    public Task<List<ScoreEntry>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        // return deep copies to prevent tests from accidentally mutating internal state.
        var clone = EntriesToReturn
            .Select(e => new ScoreEntry(e.Username, e.Score, e.At))
            .ToList();
        return Task.FromResult(clone);
    }
}

internal sealed class FakeProfanityFilter : IProfanityFilter
{
    public bool ShouldFlag { get; set; }
    public string? LastChecked { get; private set; }

    public bool ContainsProfanity(string? text)
    {
        LastChecked = text;
        return ShouldFlag;
    }
}
