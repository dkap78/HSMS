namespace HSMS.DB.Models
{
    public class UserDevice : BaseModel
    {
        #region Properties
        public Guid UserId { get; set; }

        public string DeviceToken { get; set; } = null!;
        public string? Platform { get; set; }

        public DateTime? LastUsedAt { get; set; }
        #endregion
    }
}
