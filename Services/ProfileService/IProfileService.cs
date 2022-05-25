using Entities;
using System.Text;

namespace Services.ProfileService
{
    public interface IProfileService
    {
        Task<ServiceResponse> GetUserProfile(string username);
        Task<ServiceResponse> UpdateUserProfile(string username, string displayName, string bio, StringBuilder logs);
    }
}