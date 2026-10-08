using Microsoft.AspNetCore.Mvc;

namespace RockTrack.Api.Controllers
{
    [ApiController]
    [Route("members/{memberId}/rocks")]
    public class RocksController : ControllerBase
    {
        private readonly ILogger<RocksController> _logger;

        public RocksController(ILogger<RocksController> logger)
        {
            _logger = logger;
        }
    }
}
