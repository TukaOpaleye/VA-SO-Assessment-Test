using System;
using System.Collections.Generic;
using System.Text;
using RockTrack.Core.Repositories;

namespace RockTrack.Core.Services
{
    public sealed class RockService : IRockService
    {
        private readonly IRockRepository _repository; 
        public RockService(IRockRepository repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<Rock>> GetRocksAsync(int memberId, RockStatus? status)
        {
            return _repository.GetByMemberAsync(memberId, status);
        }

        public async Task<Rock> CreateRockAsync(int memberId, string title, RockCategory category, DateOnly dueDate, string? note)
        {
            var rock = new Rock(memberId, title, category, dueDate, note);
            await _repository.AddAsync(rock);
            return rock;
        }
    }
}
