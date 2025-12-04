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

        await using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);

        await connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var entries = new List<ScoreEntry>();


        //reads data from the tables, using reader to iterate rows and GetOrdinal to find the right column/row, then the value gets read wíth GetString or GetInt32
        while (await reader.ReadAsync(cancellationToken))
        {
            var username = reader.GetString(reader.GetOrdinal("username"));
            var score = reader.GetInt32(reader.GetOrdinal("score"));

            var atOrdinal = reader.GetOrdinal("at");
            var atValue = reader.GetFieldValue<DateTime>(atOrdinal);
            var at = DateTime.SpecifyKind(atValue, DateTimeKind.Utc);

            entries.Add(new ScoreEntry(username, score, new DateTimeOffset(at)));
        }
        return entries;
    }
}
