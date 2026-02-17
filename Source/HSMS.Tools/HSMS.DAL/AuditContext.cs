using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSMS.DAL
{
    public sealed class AuditContext
    {
        public Guid UserId { get; }

        public AuditContext(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            UserId = userId;
        }
    }
}
