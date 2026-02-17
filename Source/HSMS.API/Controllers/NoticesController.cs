using HSMS.DAL;
using HSMS.DB.Models;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.API.Controllers
{
    [ApiController]
    [Route("api/notices")]
    public class NoticesController : ControllerBase
    {
        private readonly HSMSDbContext _db;

        public NoticesController(HSMSDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> List()
            => Ok(await _db.Notices.GetActiveAsync());

        [HttpPost]
        public async Task<IActionResult> Create(Notice notice)
        {
            var id = await _db.Notices.CreateAsync(notice);
            return Ok(new { id });
        }
    }
}
