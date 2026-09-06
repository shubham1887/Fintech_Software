using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.LastChangeUserDetails;
using FintechApp.Responses.LastChangeUserDetails;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class LastChangeUserDetailsService : ILastChangeUserDetailsService
    {
        private readonly ILastChangeUserDetailsRepository _repository;
        public LastChangeUserDetailsService(ILastChangeUserDetailsRepository repository) { _repository = repository; }
        private static LastChangeUserDetailsResponse MapToResponse(LastChangeUserDetails e) => new() { LastChangeUserDetailsID = e.LastChangeUserDetailsID, OnboardingUserID = e.OnboardingUserID, Change = e.Change, OldChanges = e.OldChanges, NewChanges = e.NewChanges, };
        public async Task<List<LastChangeUserDetailsResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<LastChangeUserDetailsResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateLastChangeUserDetailsRequest request) { var entity = new LastChangeUserDetails { OnboardingUserID = request.OnboardingUserID, Change = request.Change, OldChanges = request.OldChanges, NewChanges = request.NewChanges, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateLastChangeUserDetailsRequest request) { var entity = await _repository.GetByID(request.LastChangeUserDetailsID); if (entity == null) return Messages.LastChangeUserDetailsMessages.NotFound; entity.OnboardingUserID = request.OnboardingUserID; entity.Change = request.Change; entity.OldChanges = request.OldChanges; entity.NewChanges = request.NewChanges; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.LastChangeUserDetailsMessages.NotFound; }
    }
}
