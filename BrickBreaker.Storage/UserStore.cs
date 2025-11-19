using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;
using System.Text.Json;
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
    public bool Exists(string username)
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
        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("username", username.Trim()); //prepping the username to be sent to db
        connection.Open();
        return command.ExecuteScalar() is not null;
    }
    public void Add(User user)
    {
        if (user is null) throw new ArgumentException(nameof(user)); //checks if username is null

        var username = (user.Username ?? string.Empty).Trim(); //reads the username from the user object, if the string is null it substitutes it with string.Empty
        var password = (user.Password ?? string.Empty).Trim(); //reads password property from user and checks and changes if null
        if (username.Length == 0 || password.Length == 0)
        {
            throw new InvalidOperationException("Username and password are required!"); //throws error if the user doesnt enter information
        }

        const string sql = //prepares sql command to add username and password to the database
            $""" 
            INSERT INTO {TableName} (username, password)
            VALUES (@username, @password)
            ON CONFLICT (username) DO NOTHING;
            """;
        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("username", username); //sends the info to the database
        command.Parameters.AddWithValue("password", password);

        connection.Open(); 
        command.ExecuteNonQuery(); //executes the sql command
    }
    public User? Get(string username) //method to get information from the db
    {
        if (string.IsNullOrWhiteSpace(username)) //checks if the username and password exist
        {
            return null;
        }//reads from the rows in the table until it finds the right info 
        const string sql = $""" 
        SELECT username, password
        FROM {TableName}
        WHERE LOWER(username) = LOWER(@username)
        LIMIT 1;
        """;
        //establishes a connection and prepares a command to execute sql command
        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("username", username.Trim());

        //connects and then executes command, if it doesnt find anything returns null, else it reads the values from the rows and sends it back
        connection.Open();
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        var storedUsername = reader.GetString(reader.GetOrdinal("username"));
        var storedPassword = reader.GetString(reader.GetOrdinal("password"));
        return new User(storedUsername, storedPassword);

    }
}
