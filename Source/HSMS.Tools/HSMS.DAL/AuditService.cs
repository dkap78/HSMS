using HSMS.DB.Models;
using HSMS.Security;
using Npgsql;
using System.Text.Json;

namespace HSMS.DAL
{
    public class AuditService
    {
        private readonly BaseDbContext _context;
        private readonly IAppUserContext _user;

        public AuditService(BaseDbContext context, IAppUserContext user)
        {
            _context = context;
            _user = user;
        }

        public async Task LogAsync<T>(
            string entityName,
            Guid entityId,
            AuditAction action,
            T? oldValue,
            T? newValue)
        {
            var oldJson = oldValue == null ? null : JsonSerializer.Serialize(oldValue);
            var newJson = newValue == null ? null : JsonSerializer.Serialize(newValue);

            await _context.ExecuteAsync(
                "sp_audit_insert", System.Data.CommandType.StoredProcedure,
                [new NpgsqlParameter("p_entity_name", entityName),
                 new NpgsqlParameter("p_entity_id", entityId),
                 new NpgsqlParameter("p_action", action.ToString().ToUpperInvariant()),
                 new NpgsqlParameter("p_changed_by", _user.UserId),
                 new NpgsqlParameter("p_old_values", (object?)oldJson ?? DBNull.Value),
                 new NpgsqlParameter("p_new_values", (object?)newJson ?? DBNull.Value)]
            );
        }
    }

}
