namespace Entities.UserAccount
{
    public class JwtInfo
    {
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
