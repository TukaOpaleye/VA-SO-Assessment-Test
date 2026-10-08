using Microsoft.AspNetCore.Mvc;
using RockTrack.Core;
using RockTrack.Core.Services;

namespace RockTrack.Api.Controllers
{
    [ApiController]
    [Route("members/{memberId}/rocks")]
    public class RocksController : ControllerBase
    {
        private readonly ILogger<RocksController> _logger;
        private readonly IRockService _rockService;

        public RocksController(IRockService rockService, ILogger<RocksController> logger)
        {
            _logger = logger;
            _rockService = rockService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRocks(int memberId, [FromQuery] RockStatus? status)
        {
            var rocks = await _rockService.GetRocksAsync(memberId, status);
            return Ok(rocks);
        }





    }
}
