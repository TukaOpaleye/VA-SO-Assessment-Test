using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RockTrack.Api.Dto;
using RockTrack.Api.Validation;
using RockTrack.Core.Profiles;

namespace RockTrack.Api.Controllers
{
    [ApiController]
    [Authorize] // every action requires a valid X-Api-Key (ApiKey scheme)
    [Route("members/{memberId}/profile")]
    public class MemberProfileController : ControllerBase
    {
        private readonly IMemberProfileService _memberProfileService;

        public MemberProfileController(IMemberProfileService memberProfileService)
        {
            _memberProfileService = memberProfileService;
        }

        // Always 200 for a valid memberId: a failed profile call returns Rocks with enrichmentAvailable = false.
        // cancellationToken is bound to HttpContext.RequestAborted and stops the external call if the client disconnects.
        [HttpGet("enriched")]
        public async Task<ActionResult<EnrichedProfileResponseDto>> GetEnrichedProfile(
            [PositiveId] int memberId,
            CancellationToken cancellationToken)
        {
            var result = await _memberProfileService.GetEnrichedProfileAsync(memberId, cancellationToken);
            return Ok(EnrichedProfileResponseDto.FromResult(memberId, result));
        }
    }
}
