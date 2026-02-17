using HSMS.DAL;
using HSMS.DB.Models;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.API.Controllers
{
    [ApiController]
    [Route("api/flats")]
    public class FlatsController : ControllerBase
    {
        private readonly HSMSDbContext _db;

        public FlatsController(HSMSDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> List()
            => Ok(await _db.Flats.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Create(Flat flat)
        {
            var id = await _db.Flats.CreateAsync(flat);
            return Ok(new { id });
        }
    }

}
