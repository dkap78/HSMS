using HSMS.DB.Models;
using HSMS.Security;
using HSMS.Tools.HSMS.Security;
using Npgsql;

namespace HSMS.DAL
{
    public sealed class UserDbSet : DbSet<User>
    {
        public UserDbSet(HSMSDbContext context) : base(context) { }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            var list = await Context.QueryAsync<User>(
                commandText: "sp_users_get_by_id",
                parameters: [new NpgsqlParameter("p_id", id)]
            );
            return list.FirstOrDefault();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var list = await Context.QueryAsync<User>(
                commandText: "sp_users_get_by_email",
                parameters: [new NpgsqlParameter("p_email", email)]
            );
            return list.FirstOrDefault();
        }

        public async Task<Guid> CreateAsync(User user)
        {
            PermissionGuard.Require(AppUser, AppPermission.ManageUsers);

            var id = (Guid)(await Context.QueryValueAsync(
                commandText: "sp_users_insert",
                parameters: [new NpgsqlParameter("p_email", user.Email),
                             new NpgsqlParameter("p_password_hash", user.PasswordHash),
                             new NpgsqlParameter("p_first_name", user.FirstName),
                             new NpgsqlParameter("p_last_name", user.LastName),
                             new NpgsqlParameter("p_phone", user.Phone)]
            ))!;

            user.Id = id;

            await Context.Audit.LogAsync(
                "users", id, AuditAction.Insert, null, user
            );

            return id;
        }

        public async Task UpdateAsync(User user)
        {
            await Context.ExecuteInTransactionAsync(async () =>
            {
                var old = await GetByIdAsync(user.Id)
                          ?? throw new Exception("User not found");

                await Context.ExecuteAsync(
                    commandText: "sp_users_update",
                    parameters: [new NpgsqlParameter("p_id", user.Id),
                                 new NpgsqlParameter("p_first_name", user.FirstName),
                                 new NpgsqlParameter("p_last_name", user.LastName),
                                 new NpgsqlParameter("p_phone", user.Phone)]
                );

                await Context.Audit.LogAsync(
                    "users", user.Id, AuditAction.Update, old, user
                );
            });
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            await Context.ExecuteInTransactionAsync(async () =>
            {
                var old = await GetByIdAsync(id);

                await Context.ExecuteAsync(
                    commandText: "sp_users_soft_delete",
                    parameters: [new NpgsqlParameter("p_id", id)]
                );

                await Context.Audit.LogAsync(
                    "users", id, AuditAction.Delete, old, null
                );
            });
        }
        public Task<List<User>> SearchAsync(string term)
        {
            return Context.QueryAsync<User>(
                commandText: "sp_users_search",
                parameters: [new NpgsqlParameter("p_term", term)]
            );
        }

    }
}
