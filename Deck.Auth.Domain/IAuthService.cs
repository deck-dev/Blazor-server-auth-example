namespace Deck.Auth.Domain
{
    public interface IAuthService
    {
        Task<User?> Authenticate(string username, string password);
        Task RegisterUser(string username, string password, string? role = null);
        Task<IEnumerable<UserClaim>?> GetUserClaims(User user);
        Task<bool> AddClaim(UserClaim claim);
        Task<bool> RemoveClaim(UserClaim claim);
        Task<bool> ChangePassword(string username, string oldPassword, string newPassword);
    }
}