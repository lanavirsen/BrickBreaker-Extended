namespace BrickBreaker.Api;

public sealed class StorageConnectionFactory
{
    public StorageConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A connection string is required.", nameof(connectionString));
        }

        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }
}
