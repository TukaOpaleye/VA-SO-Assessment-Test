using System;
using System.Collections.Generic;
using System.Text;

namespace RockTrack.Core.Services
{
    public interface IRockService
    {
        Task<IReadOnlyList<Rock>> GetRocksAsync(int memberId, RockStatus? status);
        Task<Rock> CreateRockAsync(int memberId, string title, RockCategory category, DateOnly dueDate, string? note);
        Task<(Rock? Rock, bool Updated)> UpdateRockStatusAsync(int memberId, Guid rockId, RockStatus newStatus);
    }
}
