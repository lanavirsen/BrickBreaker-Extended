using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Models;
using Npgsql;
using NpgsqlTypes;

namespace BrickBreaker.Storage;

public sealed class LeaderboardStore : ILeaderboardStore
{
    //Declare the table name in the database and use the connectionstrin to connect to the database
    private const string TableName = "leaderboard";
    private readonly string _connectionString;

    //Method makes sure there is always a PostGreSQL connection
    public LeaderboardStore(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A PostgreSQL connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    //Adds a highscore to the table leaderboard 
    public async Task AddAsync(ScoreEntry entry, CancellationToken cancellationToken = default)
    {
        if (entry is null) throw new ArgumentException(nameof(entry));

        //insert values into the table
        const string sql = $"""
        INSERT INTO {TableName} (username, score, at)
        VALUES (@username, @score, @at);
        """;

        await using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);

        //takes data from entry. and sends it to the db
        command.Parameters.AddWithValue("username", (entry.Username ?? string.Empty).Trim());
        command.Parameters.AddWithValue("score", entry.Score);
        command.Parameters.AddWithValue("at", NpgsqlDbType.TimestampTz, entry.At.UtcDateTime);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    //methods that reads the db using sql command select
    public async Task<List<ScoreEntry>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = $"""
    SELECT username, score, at
    FROM {TableName};
    """;

        return await ExecuteQueryAsync(sql, cancellationToken: cancellationToken);
    }

    public async Task<List<ScoreEntry>> ReadTopAsync(int count, CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return [];
        }

        const string sql = $"""
        SELECT username, score, at
        FROM {TableName}
        ORDER BY score DESC, at ASC, username ASC
        LIMIT @limit;
        """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("limit", count);

        await connection.OpenAsync(cancellationToken);
        return await ReadEntriesAsync(command, cancellationToken);
    }

    public async Task<ScoreEntry?> ReadBestForAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        const string sql = $"""
        SELECT username, score, at
        FROM {TableName}
        WHERE LOWER(username) = LOWER(@username)
        ORDER BY score DESC, at ASC, username ASC
        LIMIT 1;
        """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("username", username.Trim());

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapEntry(reader);
        }

        return null;
    }

    private async Task<List<ScoreEntry>> ExecuteQueryAsync(string sql, Action<NpgsqlCommand>? configure = null, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await using var command = new NpgsqlCommand(sql, connection);
        configure?.Invoke(command);

        await connection.OpenAsync(cancellationToken);
        return await ReadEntriesAsync(command, cancellationToken);
    }

    private static async Task<List<ScoreEntry>> ReadEntriesAsync(NpgsqlCommand command, CancellationToken cancellationToken)
    {
        var entries = new List<ScoreEntry>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(MapEntry(reader));
        }

        return entries;
    }

    private static ScoreEntry MapEntry(NpgsqlDataReader reader)
    {
        var username = reader.GetString(reader.GetOrdinal("username"));
        var score = reader.GetInt32(reader.GetOrdinal("score"));

        var atOrdinal = reader.GetOrdinal("at");
        var atValue = reader.GetFieldValue<DateTime>(atOrdinal);
        var at = DateTime.SpecifyKind(atValue, DateTimeKind.Utc);

        return new ScoreEntry(username, score, new DateTimeOffset(at));
    }
}
