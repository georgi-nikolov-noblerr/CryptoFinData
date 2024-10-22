namespace CryptoFinData.Core.Entities;

public class User
{
    public int Id { get; private set; }
    public string Email { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User(string email, string username, string passwordHash)
    {
        Email = email;
        Username = username;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }

    // Required by EF Core
    public User() { }
}
