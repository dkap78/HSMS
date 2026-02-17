using HSMS.DAL;
using HSMS.DB.Models;
using HSMS.Security;
using HSMS.Tools.HSMS.Security;
using Npgsql;

namespace HSMS.DAL
{
    public sealed class AuditDbSet : DbSet<AuditLog>
    {
        public AuditDbSet(HSMSDbContext context) : base(context) { }

        public Task<List<AuditLog>> ListAsync(
            string entity,
            DateTime from,
            DateTime to)
        {
            PermissionGuard.Require(AppUser, AppPermission.ManageUsers);

            return Context.QueryAsync<AuditLog>(
                commandText: "sp_audit_list",
                parameters: [new NpgsqlParameter("p_entity", entity),
                             new NpgsqlParameter("p_from", from),
                             new NpgsqlParameter("p_to", to)]
            );
        }
    }
}
