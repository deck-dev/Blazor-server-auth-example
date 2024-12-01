namespace Deck.Auth.Domain
{
    public class User
    {
        public long UserId { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public string? Role { get; set; }
    }
}
