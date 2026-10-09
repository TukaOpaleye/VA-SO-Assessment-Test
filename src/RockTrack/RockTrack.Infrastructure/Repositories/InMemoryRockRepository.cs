using System;
using System.Collections.Generic;
using System.Text;
using RockTrack.Core;
using RockTrack.Core.Repositories;
using RockTrack.Core.Services;

namespace RockTrack.Infrastructure.Repositories
{
    public sealed class InMemoryRockRepository : IRockRepository
    {
        private readonly Dictionary<Guid, Rock> _rocks = new();
        private readonly Lock _lock = new();

        public Task<IReadOnlyList<Rock>> GetByMemberAsync(int memberId, RockStatus? status)
        {
            lock (_lock)
            {
                IReadOnlyList<Rock> rocks = _rocks.Values
                    .Where(r => r.MemberId == memberId && (status is null || r.Status == status))
                    .OrderBy(r => r.DueDate)
                    .ToList();
                return Task.FromResult(rocks);
            }
        }

        public Task AddAsync(Rock rock)
        {
            lock (_lock)
            {
                _rocks.Add(rock.Id, rock);
            }
            return Task.CompletedTask;
        }

        // Find, ownership check and transition run under one lock to avoid check-then-act races
        public Task<UpdateRockStatusResult> UpdateStatusAsync(int memberId, Guid rockId, RockStatus newStatus)
        {
            lock (_lock)
            {
                if (!_rocks.TryGetValue(rockId, out var rock) || rock.MemberId != memberId)
                {
                    return Task.FromResult(UpdateRockStatusResult.NotFound());
                }

                var result = rock.TryTransitionTo(newStatus)
                    ? UpdateRockStatusResult.Updated(rock)
                    : UpdateRockStatusResult.InvalidTransition(rock);

                return Task.FromResult(result);
            }
        }
    }
}
