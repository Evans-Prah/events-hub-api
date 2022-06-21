using Entities;
using System.Text;

namespace Services.UserAccount
{
    public interface IUserAccountService
    {
        Task<ServiceResponse> RegisterUser(string username, string displayName, string password, string email, string phoneNumber, StringBuilder logs);
        Task<ServiceResponse> ResendEmailConfirmationLink(string email, StringBuilder logs);
        Task<ServiceResponse> UserLogin(string username, string password, StringBuilder logs);
        Task<ServiceResponse> VerifyEmail(string email, string emailConfirmationToken, StringBuilder logs);
    }
}