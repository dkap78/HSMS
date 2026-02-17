namespace HSMS.DB.Models
{
    public class Payment : BaseModel
    {
        #region Properties
        public Guid UserId { get; set; }
        public Guid FlatId { get; set; }

        public decimal Amount { get; set; }

        public PaymentType PaymentType { get; set; }
        public string? TransactionRef { get; set; }

        public DateTime PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }

        public Guid EnteredByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovalDate { get; set; }

        public string? Remarks { get; set; }
        #endregion
    }
}
