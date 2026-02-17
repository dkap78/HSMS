using HSMS.DB.Models;
using HSMS.Security;
using HSMS.Tools.HSMS.Security;
using Npgsql;

namespace HSMS.DAL
{
    public sealed class RoleDbSet : DbSet<Role>
    {
        public RoleDbSet(HSMSDbContext context) : base(context) { }

        public async Task<Guid> CreateAsync(Role role)
        {
            PermissionGuard.Require(AppUser, AppPermission.ManageUsers);

            var id = (Guid)(await Context.QueryValueAsync(
                commandText: "sp_roles_insert",
                parameters: [new NpgsqlParameter("p_name", role.Name)]
            ))!;

            role.Id = id;
            await Context.Audit.LogAsync("roles", id, AuditAction.Insert, null, role);
            return id;
        }

        public Task<List<Role>> GetAllAsync()
            => Context.QueryAsync<Role>("sp_roles_list");

        public async Task AssignPermissionAsync(Guid roleId, string permission)
        {
            await Context.ExecuteAsync(
                commandText: "sp_role_permission_add",
                parameters: [new NpgsqlParameter("p_role_id", roleId),
                             new NpgsqlParameter("p_permission", permission)]
            );
        }
    }
}
