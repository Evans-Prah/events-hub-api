namespace Entities.UserAccount
{
    public class ForgotPasswordResponse
    {
        public string Message { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ResetCode { get; set; }
        public string ResetCodeExpiry { get; set; }
    }
}
