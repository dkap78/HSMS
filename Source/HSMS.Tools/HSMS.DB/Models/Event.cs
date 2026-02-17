namespace HSMS.DB.Models
{
    public class Event : BaseModel
    {
        #region Properties
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public DateTime EventDate { get; set; }
        public DateTime? EventEndDate { get; set; }

        public string? Location { get; set; }

        public Guid CreatedByUserId { get; set; }
        #endregion
    }
}
