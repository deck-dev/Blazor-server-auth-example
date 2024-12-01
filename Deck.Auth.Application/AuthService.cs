using Deck.Auth.Domain;
using System.Security.Cryptography;
using System.Text;

namespace Deck.Auth.Application
{
    public class AuthService(IUserRepository userRepo) : IAuthService
    {
        public async Task<User?> Authenticate(string username, string password)
        {
            var user = await userRepo.ReadUserByUsername(username);
            if (user is null) return null;
            if (VerifyPassword(password, user.PasswordHash)) return user;
            return null;
        }

        public async Task<IEnumerable<UserClaim>?> GetUserClaims(User user)
        {
            if (user is null) return null;
            return await userRepo.ReadUserClaims(user);
        }

        public async Task RegisterUser(string username, string password, string? role)
        {
            var passwordHash = HashPassword(password);
            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                Role = role
            };

            await userRepo.CreateUser(user);
        }

        public async Task<bool> ChangePassword(string username, string oldPassword, string newPassword)
        {
            var user = await userRepo.ReadUserByUsername(username);

            if (!(user is not null && VerifyPassword(oldPassword, user.PasswordHash)))
            {
                return false;
            }

            var newPasswordHash = HashPassword(newPassword);
            return await userRepo.UpdatePassword(username, newPasswordHash);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword.Equals(storedHash);
        }

        public async Task<bool> AddClaim(UserClaim claim)
        {
            return await userRepo.CreateUserClaim(claim);
        }

        public async Task<bool> RemoveClaim(UserClaim claim)
        {
            return await userRepo.DeleteUserClaim(claim);
        }
    }
}
