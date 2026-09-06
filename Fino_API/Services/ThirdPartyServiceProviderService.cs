using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ThirdPartyServiceProvider;
using FintechApp.Responses.ThirdPartyServiceProvider;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class ThirdPartyServiceProviderService : IThirdPartyServiceProviderService
    {
        private readonly IThirdPartyServiceProviderRepository _repository;
        public ThirdPartyServiceProviderService(IThirdPartyServiceProviderRepository repository) { _repository = repository; }
        private static ThirdPartyServiceProviderResponse MapToResponse(ThirdPartyServiceProvider e) => new() { ThirdPartyServiceProviderID = e.ThirdPartyServiceProviderID, Name = e.Name, Token = e.Token, DomainName = e.DomainName, AppID = e.AppID, AllowedServices = e.AllowedServices, IsActive = e.IsActive, };
        public async Task<List<ThirdPartyServiceProviderResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ThirdPartyServiceProviderResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateThirdPartyServiceProviderRequest request) { var entity = new ThirdPartyServiceProvider { Name = request.Name, Token = request.Token, DomainName = request.DomainName, AppID = request.AppID, AllowedServices = request.AllowedServices, IsActive = request.IsActive, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateThirdPartyServiceProviderRequest request) { var entity = await _repository.GetByID(request.ThirdPartyServiceProviderID); if (entity == null) return Messages.ThirdPartyServiceProviderMessages.NotFound; entity.Name = request.Name; entity.Token = request.Token; entity.DomainName = request.DomainName; entity.AppID = request.AppID; entity.AllowedServices = request.AllowedServices; entity.IsActive = request.IsActive; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.ThirdPartyServiceProviderMessages.NotFound; }
    }
}
