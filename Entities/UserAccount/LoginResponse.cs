namespace Entities.UserAccount
{
    public class LoginResponse
    {
        public string? ResponseMessage { get; set; }
        public string Username { get; set; }
        public string? DisplayName { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime? LastLogin { get; set; }
        public JwtInfo? Token { get; set; }
    }
}
