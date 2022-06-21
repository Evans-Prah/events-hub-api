namespace Entities.UserAccount
{
    public class RefreshToken
    {
        public int UserAccountId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime Revoked { get; set; }
    }
}
