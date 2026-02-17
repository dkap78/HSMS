using HSMS.DAL;
using HSMS.DB.Models;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly HSMSDbContext _db;

        public PaymentsController(HSMSDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Payment payment)
        {
            var id = await _db.Payments.CreateAsync(payment);
            return Ok(new { id });
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            await _db.Payments.ApproveAsync(id);
            return NoContent();
        }
    }
}
