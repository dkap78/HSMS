using HSMS.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSMS.Tools.HSMS.Security
{
    public static class PermissionGuard
    {
        public static void Require(
            IAppUserContext user,
            AppPermission permission)
        {
            if (!user.HasPermission(permission))
                throw new UnauthorizedAccessException(
                    $"Permission '{permission}' is required.");
        }
    }

}
