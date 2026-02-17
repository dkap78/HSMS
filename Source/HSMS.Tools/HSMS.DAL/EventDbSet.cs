using HSMS.DAL;
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
    public sealed class EventDbSet : DbSet<Event>
    {
        public EventDbSet(HSMSDbContext context) : base(context) { }

        public async Task<Guid> CreateAsync(Event ev)
        {
            PermissionGuard.Require(AppUser, AppPermission.ManageNotices);

            var id = (Guid)(await Context.QueryValueAsync(
                commandText: "sp_events_insert",
                parameters: [new NpgsqlParameter("p_title", ev.Title),
                             new NpgsqlParameter("p_description", ev.Description),
                             new NpgsqlParameter("p_event_date", ev.EventDate),
                             new NpgsqlParameter("p_created_by", AppUser.UserId)]
        ))!;

            ev.Id = id;
            await Context.Audit.LogAsync("events", id, AuditAction.Insert, null, ev);
            return id;
        }

        public Task<List<Event>> GetUpcomingAsync()
            => Context.QueryAsync<Event>("sp_events_upcoming");
    }

}
