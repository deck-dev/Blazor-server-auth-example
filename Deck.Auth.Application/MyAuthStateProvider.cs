using Deck.Auth.Domain;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Deck.Auth.Application
{
    public class MyAuthStateProvider : AuthenticationStateProvider
    {
        private User? _user;
        private List<UserClaim>? _claims;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_user is null)
            {
                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                return Task.FromResult(new AuthenticationState(anonymous));
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, _user.Username),
                new(ClaimTypes.Role, _user.Role ?? "User")
            };

            // Custom claims
            if (_claims is not null)
            {
                foreach (var claim in _claims)
                    claims.Add(new Claim("Permission", claim.ClaimName));
            }

            var identity = new ClaimsIdentity(claims, "MyAuth");
            var principal = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(principal));
        }

        public void Login(User? user, IEnumerable<UserClaim>? claims)
        {
            _user = user;
            _claims = claims?.ToList();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void Logout()
        {
            _user = null;
            _claims = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

    }
}
