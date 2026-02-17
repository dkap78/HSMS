namespace HSMS.DB.Models
{
    public class AuditLog : BaseModel
    {
        #region Properties
        public string EntityName { get; set; } = null!;
        public Guid EntityId { get; set; }

        public AuditAction Action { get; set; }

        public Guid ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }

        public string? OldValuesJson { get; set; }
        public string? NewValuesJson { get; set; }
        #endregion
    }
}
