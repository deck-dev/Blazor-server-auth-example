namespace Deck.Auth.Domain
{
    public class UserClaim
    {
        public long ClaimId { get; set; }
        public long UserId { get; set; }
        public required string ClaimName { get; set; }
    }
}
