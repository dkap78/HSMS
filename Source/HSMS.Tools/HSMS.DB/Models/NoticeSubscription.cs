namespace HSMS.DB.Models
{
    public class NoticeSubscription : BaseModel
    {
        #region Properties
        public Guid UserId { get; set; }
        public Guid NoticeId { get; set; }
        public DateTime SubscribedAt { get; set; }
        #endregion
    }

}
