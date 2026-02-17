using HSMS.DAL;
using HSMS.DB.Models;
using HSMS.Services;

namespace HSMS.API
{
    public sealed class PaymentApprovalService
    {
        private readonly HSMSDbContext _db;
        private readonly IEmailService _email;

        public PaymentApprovalService(
            HSMSDbContext db,
            IEmailService email)
        {
            _db = db;
            _email = email;
        }

        public async Task ApproveAsync(Guid paymentId)
        {
            await _db.ExecuteInTransactionAsync(async () =>
            {
                await _db.Payments.ApproveAsync(paymentId);

                var payment = _db.Payments.GetById(paymentId).Result;

                var user = await _db.Users.GetByIdAsync(payment.UserId);

                await _email.SendAsync(
                    user!.Email,
                    "Payment Approved",
                    $"Your payment of ₹{payment.Amount} has been approved."
                );
            });
        }
    }
}
