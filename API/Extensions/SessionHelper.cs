using Entities.UserAccount;
using Newtonsoft.Json;

namespace API.Extensions
{
    public static class SessionHelper
    {
        public static bool IsSessionActive(HttpContext context)
        {
            return !string.IsNullOrWhiteSpace(context.Session.GetString("userDetails"));
        }
        public static LoginResponse? GetCurrentUser(HttpContext context)
        {
            try
            {
                var userDetailsString = context.Session.GetString("userDetails");
                if (string.IsNullOrWhiteSpace(userDetailsString)) return null;

                return JsonConvert.DeserializeObject<LoginResponse>(userDetailsString);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static void SetCurrentUser(HttpContext context, LoginResponse userDetails)
        {

            var userDetailsString = JsonConvert.SerializeObject(userDetails);
            context.Session.SetString("userDetails", userDetailsString);

        }
        public static void ClearSession(HttpContext context)
        {
            context.Session.Remove("userDetails");
            context.Session.Clear();
        }
    }
}
