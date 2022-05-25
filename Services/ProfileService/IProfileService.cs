using Entities;

namespace Services.ProfileService
{
    public interface IProfileService
    {
        Task<ServiceResponse> GetUserProfile(string username);
    }
}