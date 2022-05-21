using Services;

namespace API.Extensions
{

    public class BearerTokenDecoderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly string _jwt_encryption_key;
        public BearerTokenDecoderMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
            _jwt_encryption_key = config["JWT_ENCRYPTION_KEY"];
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var auth = context.Request.Headers["Authorization"];
                var authValue = auth[0];
                var token = authValue.Substring(7);
                context.Request.Headers.Remove("Authorization");
                context.Request.Headers.Add("Authorization",
                    $"Bearer {StringCipher.Decrypt(token, _jwt_encryption_key)}");
            }
            catch (Exception)
            {
            }

            await _next.Invoke(context);
        }
    }
}
