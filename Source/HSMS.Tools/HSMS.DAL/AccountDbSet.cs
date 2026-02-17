using HSMS.DB.Models;
using HSMS.Security;
using HSMS.Tools.HSMS.Security;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSMS.DAL
{
    public sealed class AccountDbSet : DbSet<Account>
    {
        public AccountDbSet(HSMSDbContext context) : base(context) { }

        public async Task<Guid> CreateAsync(Account account)
        {
            PermissionGuard.Require(AppUser, AppPermission.ManageAccounting);

            var id = (Guid)(await Context.QueryValueAsync(
                commandText: "sp_accounts_insert",
                parameters: [new NpgsqlParameter("p_name", account.Name),
                             new NpgsqlParameter("p_type", account.Type.ToString())]
            ))!;

            account.Id = id;
            await Context.Audit.LogAsync("accounts", id, AuditAction.Insert, null, account);
            return id;
        }

        public Task<List<Account>> GetAllAsync()
            => Context.QueryAsync<Account>("sp_accounts_list");
    }
}
