using System;
using System.Collections.Generic;
using System.Text;
using RockTrack.Core;
using RockTrack.Core.Repositories;

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
    }
}
