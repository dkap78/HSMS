using HSMS.Security;
using System.Security.Claims;

namespace HSMS.API
{
    public sealed class JwtAppUserContext : IAppUserContext
    {
        private readonly IHttpContextAccessor _http;

        public JwtAppUserContext(IHttpContextAccessor http)
        {
            _http = http;
        }

        private ClaimsPrincipal User =>
            _http.HttpContext?.User
            ?? throw new Exception("No HttpContext");

        public Guid UserId
            => Guid.Parse(User.FindFirstValue("user_id")!);

        public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

        public bool HasPermission(AppPermission permission)
        {
            return User.Claims
                .Where(c => c.Type == "permission")
                .Any(c => c.Value == permission.ToString());
        }
    }
}