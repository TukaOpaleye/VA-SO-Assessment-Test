using System;
using System.Collections.Generic;
using System.Text;

namespace RockTrack.Core.Services
{
    public interface IRockService
    {
        Task<IReadOnlyList<Rock>> GetRocksAsync(int memberId, RockStatus? status);
        Task<CreateRockResult> CreateRockAsync(int memberId, string title, RockCategory category, DateOnly dueDate, string? note);
        Task<UpdateRockStatusResult> UpdateRockStatusAsync(int memberId, Guid rockId, RockStatus newStatus);
    }
}
