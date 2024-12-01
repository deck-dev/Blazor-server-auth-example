using Dapper;
using Deck.Auth.Domain;
using Microsoft.Data.Sqlite;

namespace Deck.Auth.Application
{
    public class SqliteUserRepository : IUserRepository
    {
        private readonly string _connStr;

        public SqliteUserRepository(string connectionString)
        {
            _connStr = connectionString;
            EnsureCreated().GetAwaiter().GetResult();
        }

        #region User
        public async Task<User?> ReadUserByUsername(string username)
        {
            using var connection = new SqliteConnection(_connStr);
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Username=@Username;", new { Username = username });
        }

        public async Task<bool> CreateUser(User user)
        {
            var usr = await ReadUserByUsername(user.Username);
            if (usr is null)
            {
                return await CreateUserAsync(user);
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> UpdatePassword(string username, string newPasswordHash)
        {
            using var connection = new SqliteConnection(_connStr);
            var query = "UPDATE Users SET PasswordHash = @PasswordHash WHERE Username=@Username";
            var result = await connection.ExecuteAsync(query, new { PasswordHash = newPasswordHash, Username = username });
            return result > 0;
        }

        public async Task<bool> DeleteUser(User user)
        {
            using var connection = new SqliteConnection(_connStr);
            return await connection.ExecuteAsync("DELETE Users WHERE UserId=@UserId;", user) > 0;
        }

        private async Task<bool> CreateUserAsync(User user)
        {
            using var connection = new SqliteConnection(_connStr);
            return await connection.ExecuteAsync(
                "INSERT INTO Users(Username, PasswordHash, Role) VALUES(@Username, @PasswordHash, @Role); ",
                user) > 0;
        }
        #endregion

        #region UserClaim
        public async Task<bool> CreateUserClaim(UserClaim claim)
        {
            if (claim.UserId <= 0) return false;
            if (string.IsNullOrWhiteSpace(claim.ClaimName)) return false;

            var clm = await ReadClaim(claim.UserId, claim.ClaimName);
            if (clm is null)
            {
                return await CreateUserClaimAsync(claim);
            }
            else
            {
                return false;
            }
        }

        public async Task<IEnumerable<UserClaim>> ReadUserClaims(User user)
        {
            using var connection = new SqliteConnection(_connStr);
            return await connection.QueryAsync<UserClaim>(
                "SELECT * FROM UserClaims WHERE UserId=@UserId;", new { user.UserId });
        }

        public async Task<bool> DeleteUserClaim(UserClaim claim)
        {
            using var connection = new SqliteConnection(_connStr);
            return await connection.ExecuteAsync(
                "DELETE UserClaims WHERE ClaimId=@ClaimId; ", claim) > 0;
        }

        private async Task<bool> CreateUserClaimAsync(UserClaim claim)
        {
            using var connection = new SqliteConnection(_connStr);
            return await connection.ExecuteAsync(
                "INSERT INTO UserClaims(UserId, ClaimName) VALUES(@UserId, @ClaimName); ", claim) > 0;
        }

        private async Task<UserClaim?> ReadClaim(long userId, string claimName)
        {
            using var connection = new SqliteConnection(_connStr);
            return await connection.QueryFirstOrDefaultAsync<UserClaim>(
                "SELECT * FROM UserClaims WHERE UserId=@UserId AND ClaimName=@ClaimName;",
                new { UserId = userId, ClaimName = claimName });
        }
        #endregion

        private async Task EnsureCreated()
        {
            string query = @"
            CREATE TABLE IF NOT EXISTS ""Users"" (
                ""UserId"" INTEGER NOT NULL UNIQUE,
                ""Username"" TEXT NOT NULL UNIQUE,
                ""PasswordHash"" TEXT NOT NULL,
                ""Role"" TEXT,
                PRIMARY KEY(""UserId"" AUTOINCREMENT)
            );
            CREATE TABLE IF NOT EXISTS ""UserClaims"" (
                ""ClaimId""   INTEGER NOT NULL UNIQUE,
                ""UserId""    INTEGER NOT NULL,
	            ""ClaimName"" TEXT NOT NULL,
	            PRIMARY KEY(""ClaimId"" AUTOINCREMENT),
	            FOREIGN KEY(""UserId"") REFERENCES ""Users""(""UserId"")
            );";
            using var connection = new SqliteConnection(_connStr);
            await connection.ExecuteAsync(query);
        }
    }
}
