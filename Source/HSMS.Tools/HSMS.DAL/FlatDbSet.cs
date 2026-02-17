using HSMS.DB.Models;
using Npgsql;

namespace HSMS.DAL
{
    public sealed class FlatDbSet : DbSet<Flat>
    {
        public FlatDbSet(HSMSDbContext context) : base(context) { }

        public async Task<Guid> CreateAsync(Flat flat)
        {
            var id = (Guid)(await Context.QueryValueAsync(
                commandText: "sp_flats_insert", 
                parameters: [new NpgsqlParameter("p_flat_number", flat.FlatNumber),
                             new NpgsqlParameter("p_wing", flat.Wing),
                             new NpgsqlParameter("p_floor", flat.Floor)]
            ))!;

            flat.Id = id;

            await Context.Audit.LogAsync(
                "flats", id, AuditAction.Insert, null, flat
            );

            return id;
        }

        public Task<List<Flat>> GetAllAsync()
            => Context.QueryAsync<Flat>("sp_flats_list");
    }
}
