using System;
using System.Collections.Generic;
using System.Text;

namespace RockTrack.Core.Services
{
    public interface IRockService
    {
        Task<IReadOnlyList<Rock>> GetRocksAsync(int memberId, RockStatus? status);

    }
}
