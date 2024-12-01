namespace Deck.Auth.Domain
{
    public interface IUserRepository
    {
        Task<bool> CreateUser(User user);
        Task<User?> ReadUserByUsername(string username);
        Task<bool> UpdatePassword(string username, string newPasswordHash);
        Task<bool> DeleteUser(User user);

        Task<bool> CreateUserClaim(UserClaim claim);
        Task<IEnumerable<UserClaim>> ReadUserClaims(User user);
        Task<bool> DeleteUserClaim(UserClaim claim);
    }
}