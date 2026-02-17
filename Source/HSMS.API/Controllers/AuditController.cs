using HSMS.DAL;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.API.Controllers
{
    [ApiController]
    [Route("api/audit")]
    public class AuditController : ControllerBase
    {
        private readonly HSMSDbContext _db;

        public AuditController(HSMSDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> List(
            string entity,
            DateTime from,
            DateTime to)
        {
            return Ok(await _db.AuditLogs.ListAsync(entity, from, to));
        }
    }
}
