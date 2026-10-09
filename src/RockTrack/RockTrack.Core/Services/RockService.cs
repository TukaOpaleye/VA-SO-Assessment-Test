using System;
using System.Collections.Generic;
using System.Text;
using RockTrack.Core.Repositories;
using RockTrack.Core.Validation;

namespace RockTrack.Core.Services
{
    public sealed class RockService : IRockService
    {
        private readonly IRockRepository _repository;
        private readonly IRockCategoryValidator _categoryValidator;
        private readonly TimeProvider _timeProvider;

        public RockService(IRockRepository repository, IRockCategoryValidator categoryValidator, TimeProvider timeProvider)
        {
            _repository = repository;
            _categoryValidator = categoryValidator;
            _timeProvider = timeProvider;
        }

        public Task<IReadOnlyList<Rock>> GetRocksAsync(int memberId, RockStatus? status)
        {
            return _repository.GetByMemberAsync(memberId, status);
        }

        public async Task<CreateRockResult> CreateRockAsync(int memberId, string title, RockCategory category, DateOnly dueDate, string? note)
        {
            var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

            var errors = _categoryValidator.Validate(title, category, dueDate, note, today);
            if (errors.Count > 0)
            {
                return CreateRockResult.Failure(errors);
            }

            var rock = new Rock(memberId, title, category, dueDate, note);
            await _repository.AddAsync(rock);
            return CreateRockResult.Success(rock);
        }

        public Task<(Rock? Rock, bool Updated)> UpdateRockStatusAsync(int memberId, Guid rockId, RockStatus newStatus)
        {
            return _repository.UpdateStatusAsync(memberId, rockId, newStatus);
        }
    }
}
