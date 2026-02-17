using HSMS.DB.Models;
using Npgsql;

namespace HSMS.DAL
{
    public sealed class NoticeDbSet : DbSet<Notice>
    {
        public NoticeDbSet(HSMSDbContext context) : base(context) { }

        public async Task<Guid> CreateAsync(Notice notice)
        {
            var id = (Guid)(await Context.QueryValueAsync(
                commandText: "sp_notices_insert",
                parameters: [new NpgsqlParameter("p_title", notice.Title),
                             new NpgsqlParameter("p_content", notice.Content),
                             new NpgsqlParameter("p_created_by", Context.AppUser.UserId),
                             new NpgsqlParameter("p_expire_date", notice.ExpireDate)]
            ))!;

            notice.Id = id;

            await Context.Audit.LogAsync(
                "notices", id, AuditAction.Insert, null, notice
            );

            return id;
        }

        public Task<List<Notice>> GetActiveAsync()
            => Context.QueryAsync<Notice>("sp_notices_list");
    }
}
