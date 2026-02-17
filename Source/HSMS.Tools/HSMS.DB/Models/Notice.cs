namespace HSMS.DB.Models
{
    public class Notice : BaseModel
    {
        #region Properties
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;

        public Guid CreatedByUserId { get; set; }

        public DateTime PublishDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        #endregion
    }
}
