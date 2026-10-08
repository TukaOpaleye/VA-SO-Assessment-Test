using System.ComponentModel.DataAnnotations;
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
        public async Task<ActionResult<IReadOnlyList<RockResponseDto>>> GetRocks([Range(1, int.MaxValue, ErrorMessage = "memberId must be a positive number.")] int memberId, [FromQuery] RockStatus? status)
        {
            var rocks = await _rockService.GetRocksAsync(memberId, status);
            return Ok(rocks.Select(RockResponseDto.FromRock).ToList());
        }

        [HttpPost]
        public async Task<ActionResult<RockResponseDto>> CreateRock([Range(1, int.MaxValue, ErrorMessage = "memberId must be a positive number.")] int memberId, CreateRockDto request)
        {
            var rock = await _rockService.CreateRockAsync(memberId, request.Title, request.Category, request.DueDate, request.Note);

            return Created($"/members/{memberId}/rocks/{rock.Id}", RockResponseDto.FromRock(rock));
        }

        [HttpPatch("{rockId:guid}")]
        public async Task<ActionResult<RockResponseDto>> UpdateRock([Range(1, int.MaxValue, ErrorMessage = "memberId must be a positive number.")] int memberId, Guid rockId, UpdateRockDto request)
        {
            var (rock, updated) = await _rockService.UpdateRockStatusAsync(memberId, rockId, request.Status);

            if (rock is null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, 
                    title: "Rock not found",
                    detail: $"Rock '{rockId}' does not exist for member {memberId}.");
            }

            if (!updated)
            {
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "Invalid status transition",
                    detail: $"Cannot change status from '{rock.Status.ToString().ToLowerInvariant()}' to '{request.Status.ToString().ToLowerInvariant()}'. Only pending rocks can be marked as completed or missed.");
            }

            return Ok(RockResponseDto.FromRock(rock));
        }
    }
}
