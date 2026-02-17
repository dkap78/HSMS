namespace HSMS.DB.Models
{
    public enum OccupancyType
    {
        OwnerOccupied,
        Rented
    }

    public enum PaymentStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum PaymentType
    {
        Cash,
        Cheque,
        Online
    }

    public enum AccountType
    {
        Income,
        Expense
    }

    public enum AuditAction
    {
        Insert,
        Update,
        Delete
    }
}
