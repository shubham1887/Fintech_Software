using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.OnboardingUserDocument;
using FintechApp.Responses.OnboardingUserDocument;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class OnboardingUserDocumentService : IOnboardingUserDocumentService
    {
        private readonly IOnboardingUserDocumentRepository _repository;
        public OnboardingUserDocumentService(IOnboardingUserDocumentRepository repository) { _repository = repository; }
        private static OnboardingUserDocumentResponse MapToResponse(OnboardingUserDocument e) => new() { OnboardingUserDocumentID = e.OnboardingUserDocumentID, Ref_ID = e.Ref_ID, Type = e.Type, OldImage = e.OldImage, NewImage = e.NewImage, OnboardingUserID = e.OnboardingUserID, };
        public async Task<List<OnboardingUserDocumentResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<OnboardingUserDocumentResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateOnboardingUserDocumentRequest request) { var entity = new OnboardingUserDocument { Ref_ID = request.Ref_ID, Type = request.Type, OldImage = request.OldImage, NewImage = request.NewImage, OnboardingUserID = request.OnboardingUserID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateOnboardingUserDocumentRequest request) { var entity = await _repository.GetByID(request.OnboardingUserDocumentID); if (entity == null) return Messages.OnboardingUserDocumentMessages.NotFound; entity.Ref_ID = request.Ref_ID; entity.Type = request.Type; entity.OldImage = request.OldImage; entity.NewImage = request.NewImage; entity.OnboardingUserID = request.OnboardingUserID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.OnboardingUserDocumentMessages.NotFound; }
    }
}
