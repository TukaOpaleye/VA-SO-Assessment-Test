using System;
using System.Collections.Generic;
using System.Text;
using RockTrack.Core.Repositories;

namespace RockTrack.Core.Services
{
    public class RockService : IRockService
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
    }
}
