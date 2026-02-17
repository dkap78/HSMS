namespace HSMS.DB.Models
{
    public class AccountTransaction : BaseModel
    {
        #region Properties
        public Guid AccountId { get; set; }

        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }

        public string? Reference { get; set; }
        public string? Description { get; set; }

        public Guid CreatedByUserId { get; set; }
        #endregion
    }
}
