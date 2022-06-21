
namespace Services.EmailService
{
    public interface IEmailSenderService
    {
        Task SendEmail(string userEmail, string emailSubject, string msg);
    }
}