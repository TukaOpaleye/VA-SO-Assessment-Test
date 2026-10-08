using Microsoft.AspNetCore.Mvc;
using RockTrack.Api.Dto;
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
        public async Task<ActionResult<IReadOnlyList<RockResponseDto>>> GetRocks(int memberId, [FromQuery] RockStatus? status)
        {
            var rocks = await _rockService.GetRocksAsync(memberId, status);
            return Ok(rocks.Select(RockResponseDto.FromRock).ToList());
        }

        [HttpPost]
        public async Task<ActionResult<RockResponseDto>> CreateRock(int memberId, CreateRockDto request)
        {
            var rock = await _rockService.CreateRockAsync(memberId, request.Title, request.Category, request.DueDate, request.Note);

            return Created($"/members/{memberId}/rocks/{rock.Id}", RockResponseDto.FromRock(rock));
        }
    }
}
