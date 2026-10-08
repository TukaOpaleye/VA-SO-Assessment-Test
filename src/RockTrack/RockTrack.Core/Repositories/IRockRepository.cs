using System;
using System.Collections.Generic;
using System.Text;

namespace RockTrack.Core.Repositories
{
    public interface IRockRepository
    {
        Task<IReadOnlyList<Rock>> GetByMemberAsync(int memberId, RockStatus? status);
    }
}
