using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RockTrack.Api.Dto;
using RockTrack.Api.Validation;
using RockTrack.Core;
using RockTrack.Core.Services;

namespace RockTrack.Api.Controllers
{
    [ApiController]
    [Authorize] // every action requires a valid X-Api-Key (ApiKey scheme)
    [Route("members/{memberId}/rocks")]
    public class RocksController : ControllerBase
    {
        private readonly IRockService _rockService;

        public RocksController(IRockService rockService)
        {
            _rockService = rockService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<RockResponseDto>>> GetRocks(
            [PositiveId] int memberId,
            [FromQuery] RockStatus? status)
        {
            var rocks = await _rockService.GetRocksAsync(memberId, status);
            return Ok(rocks.Select(RockResponseDto.FromRock).ToList());
        }

        [HttpPost]
        public async Task<ActionResult<RockResponseDto>> CreateRock(
            [PositiveId] int memberId,
            CreateRockDto request)
        {
            var result = await _rockService.CreateRockAsync(memberId, request.Title, request.Category, request.DueDate, request.Note);

            if (!result.IsSuccess)
            {
                // Same ValidationProblemDetails 400 shape as the data annotation errors
                foreach (var (key, messages) in result.Errors)
                {
                    foreach (var message in messages)
                    {
                        ModelState.AddModelError(key, message);
                    }
                }

                return ValidationProblem(ModelState);
            }

            return Created($"/members/{memberId}/rocks/{result.Rock.Id}", RockResponseDto.FromRock(result.Rock));
        }

        [HttpPatch("{rockId:guid}")]
        public async Task<ActionResult<RockResponseDto>> UpdateRock(
            [PositiveId] int memberId,
            Guid rockId,
            UpdateRockDto request)
        {
            var result = await _rockService.UpdateRockStatusAsync(memberId, rockId, request.Status);

            if (result.IsSuccess)
            {
                return Ok(RockResponseDto.FromRock(result.Rock));
            }

            if (result.IsInvalidTransition)
            {
                return Problem(statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "Invalid status transition",
                    detail: $"Cannot change status from '{result.Rock.Status.ToString().ToLowerInvariant()}' to " +
                            $"'{request.Status.ToString().ToLowerInvariant()}'. Only pending rocks can be marked as completed or missed.");
            }

            return Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Rock not found",
                detail: $"Rock '{rockId}' does not exist for member {memberId}.");
        }
    }
}
