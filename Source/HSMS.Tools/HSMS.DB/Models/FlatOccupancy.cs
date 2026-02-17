namespace HSMS.DB.Models
{
    public class FlatOccupancy : BaseModel
    {
        #region Properties
        public Guid FlatId { get; set; }

        public Guid OwnerUserId { get; set; }
        public Guid? TenantUserId { get; set; }

        public OccupancyType OccupancyType { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        #endregion
    }
}
