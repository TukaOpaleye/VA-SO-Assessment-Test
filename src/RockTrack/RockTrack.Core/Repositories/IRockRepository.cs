using System;
using System.Collections.Generic;
using System.Text;
using RockTrack.Core.Services;

namespace RockTrack.Core.Repositories
{
    public interface IRockRepository
    {
        Task<IReadOnlyList<Rock>> GetByMemberAsync(int memberId, RockStatus? status);
        Task AddAsync(Rock rock);
        Task<UpdateRockStatusResult> UpdateStatusAsync(int memberId, Guid rockId, RockStatus newStatus);
    }
}
