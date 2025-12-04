using System.Diagnostics.CodeAnalysis;

namespace BrickBreaker.Core.Models;

public sealed class User //define the attributes the class has
{
    public required string Username { get; set; }
    public required string Password { get; set; }

    [SetsRequiredMembers] // attribute to indicate that the constructor sets all required members
    public User(string username, string password) //constructer for the class
    {
        Username = username;
        Password = password;
    }

    [SetsRequiredMembers]
    public User()
    {
        Username = string.Empty;
        Password = string.Empty;
    }
}
