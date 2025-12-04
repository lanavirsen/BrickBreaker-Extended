using System.Threading;
using System.Threading.Tasks;
using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Models;
using BrickBreaker.Core.Services;
using Npgsql;

namespace BrickBreaker.Storage;

public sealed class UserStore : IUserStore
{
    //defines which table in the db to use, defines where to connect
    private const string TableName = "users";
    private readonly string _connectionString;

    //checks for the connection to the db
    public UserStore(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A PostGreSQL connection string is required.", nameof(connectionString));
        }
        _connectionString = connectionString;
    }

    //method to check if a suername exists
    public async Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        //if it doesnt exist the bool returns false, else it takes the new information
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }
        const string sql =
             $"""
             SELECT 1
             FROM {TableName}
             WHERE LOWER(username) = LOWER(@username)
             LIMIT 1;
             """;
        await using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("username", username.Trim()); //prepping the username to be sent to db
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user is null) throw new ArgumentException(nameof(user)); //checks if username is null

        var username = (user.Username ?? string.Empty).Trim(); //reads the username from the user object, if the string is null it substitutes it with string.Empty
        var password = (user.Password ?? string.Empty).Trim(); //reads password property from user and checks and changes if null
        if (username.Length == 0 || password.Length == 0)
        {
            throw new InvalidOperationException("Username and password are required!"); //throws error if the user doesnt enter information
        }

        if (!PasswordHasher.TryParse(password, out var components) || !components.IsValid)
        {
            throw new InvalidOperationException("Password must be hashed before storage.");
        }

        const string sql = //prepares command for the db
            $""" 
            INSERT INTO {TableName} (username, password_hash, salt)
            VALUES (@username, @password_hash, @salt)
            ON CONFLICT (username) DO NOTHING;
            """;
        await using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("password_hash", password);
        command.Parameters.AddWithValue("salt", components.Salt);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken); //executes the sql command
    }
    public async Task<User?> GetAsync(string username, CancellationToken cancellationToken = default) //method to get information from the db
    {
        if (string.IsNullOrWhiteSpace(username)) //checks if the username and password exist
        {
            return null;
        }
        const string sql = $"""
        SELECT username, password_hash, salt
        FROM {TableName}
        WHERE LOWER(username) = LOWER(@username)
        LIMIT 1;
        """;
        //establishes a connection and prepares a command to execute sql command
        await using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("username", username.Trim());

        //connects and then executes command, if it doesnt find anything returns null, else it reads the values from the rows and sends it back
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        var storedUsername = reader.GetString(reader.GetOrdinal("username")); //fetches username from username row
        var storedPassword = reader.GetString(reader.GetOrdinal("password_hash")); //fetches password hash from its row
        var storedSaltOrdinal = reader.GetOrdinal("salt"); //fethes salt from its row
        if (!PasswordHasher.TryParse(storedPassword, out var components) && !reader.IsDBNull(storedSaltOrdinal)) //checks validity of the fetched information
        {
            var salt = reader.GetString(storedSaltOrdinal);
            var hashComponents = new PasswordHasher.HashComponents(
                PasswordHasher.AlgorithmName,
                PasswordHasher.DefaultIterations,
                salt,
                storedPassword);
            storedPassword = PasswordHasher.Compose(hashComponents);
        }
        return new User(storedUsername, storedPassword);
    }
}
