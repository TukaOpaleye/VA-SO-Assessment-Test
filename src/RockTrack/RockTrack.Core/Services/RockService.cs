using Microsoft.Extensions.Logging;
using RockTrack.Core.Repositories;
using RockTrack.Core.Validation;

namespace RockTrack.Core.Services
{
    public sealed class RockService : IRockService
    {
        private readonly IRockRepository _repository;
        private readonly IRockCategoryValidator _categoryValidator;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<RockService> _logger;

        public RockService(IRockRepository repository, IRockCategoryValidator categoryValidator,
            TimeProvider timeProvider, ILogger<RockService> logger)
        {
            _repository = repository;
            _categoryValidator = categoryValidator;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<IReadOnlyList<Rock>> GetRocksAsync(int memberId, RockStatus? status)
        {
            var rocks = await _repository.GetByMemberAsync(memberId, status);

            _logger.LogDebug("Found {RockCount} rocks for member {MemberId} with status filter {StatusFilter}",
                rocks.Count, memberId, status);

            return rocks;
        }

        public async Task<CreateRockResult> CreateRockAsync(int memberId, string title, RockCategory category, DateOnly dueDate, string? note)
        {
            var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

            var errors = _categoryValidator.Validate(title, category, dueDate, note, today);
            if (errors.Count > 0)
            {
                // Field names only - no title and note (free text) not logged
                _logger.LogInformation("Rock creation rejected for member {MemberId}: {InvalidFields} failed {Category} rules",
                    memberId, errors.Keys, category);

                return CreateRockResult.Failure(errors);
            }

            var rock = new Rock(memberId, title, category, dueDate, note);
            await _repository.AddAsync(rock);

            _logger.LogInformation("Rock {RockId} created for member {MemberId} in category {Category}, due {DueDate}",
                rock.Id, memberId, category, dueDate);

            return CreateRockResult.Success(rock);
        }

        public async Task<(Rock? Rock, bool Updated)> UpdateRockStatusAsync(int memberId, Guid rockId, RockStatus newStatus)
        {
            var (rock, updated) = await _repository.UpdateStatusAsync(memberId, rockId, newStatus);

            if (rock is null)
            {
                _logger.LogDebug("Rock {RockId} not found for member {MemberId}", rockId, memberId);
            }
            else if (!updated)
            {
                _logger.LogInformation("Rock {RockId} cannot change status from {CurrentStatus} to {RequestedStatus}",
                    rockId, rock.Status, newStatus);
            }
            else
            {
                _logger.LogInformation("Rock {RockId} status is now changed to {Status}", rockId, rock.Status);
            }

            return (rock, updated);
        }
    }
}
