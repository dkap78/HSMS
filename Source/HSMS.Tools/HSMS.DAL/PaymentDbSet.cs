using HSMS.DB.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSMS.DAL
{
    public sealed class PaymentDbSet : DbSet<Payment>
    {
        public PaymentDbSet(HSMSDbContext context) : base(context) { }

        public async Task<Payment> GetById(Guid paymentId)
        {
            var payment = (await Context.QueryAsync<Payment>(
                commandText: "sp_payments_get_by_id",
                parameters: [new NpgsqlParameter("p_id", paymentId)]
            )).First();

            return payment;
        }

        public async Task<Guid> CreateAsync(Payment payment)
        {
            var id = (Guid)(await Context.QueryValueAsync(
                commandText: "sp_payments_insert",
                parameters: [new NpgsqlParameter("p_user_id", payment.UserId),
                             new NpgsqlParameter("p_flat_id", payment.FlatId),
                             new NpgsqlParameter("p_amount", payment.Amount),
                             new NpgsqlParameter("p_payment_type", payment.PaymentType.ToString()),
                             new NpgsqlParameter("p_transaction_ref", payment.TransactionRef),
                             new NpgsqlParameter("p_payment_date", payment.PaymentDate),
                             new NpgsqlParameter("p_entered_by", Context.AppUser.UserId)]
            ))!;

            payment.Id = id;

            await Context.Audit.LogAsync(
                "payments", id, AuditAction.Insert, null, payment
            );

            return id;
        }

        public async Task ApproveAsync(Guid paymentId)
        {
            await Context.ExecuteInTransactionAsync(async () =>
            {
                await Context.ExecuteAsync(
                    commandText: "sp_payments_approve",
                    parameters: [new NpgsqlParameter("p_payment_id", paymentId),
                                 new NpgsqlParameter("p_admin_id", Context.AppUser.UserId)]
                );

                await Context.Audit.LogAsync(
                    "payments",
                    paymentId,
                    AuditAction.Update,
                    null,
                    new { Status = "Approved" }
                );
            });
        }
    }
}