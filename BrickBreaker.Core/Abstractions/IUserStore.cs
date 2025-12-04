using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Abstractions;

public interface IUserStore
{
    bool Exists(string username);
    void Add(User user);
    User? Get(string username);
}
