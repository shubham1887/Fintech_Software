using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.BrowserUserAgentMaster;
using FintechApp.Responses.BrowserUserAgentMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class BrowserUserAgentMasterService : IBrowserUserAgentMasterService
    {
        private readonly IBrowserUserAgentMasterRepository _repository;
        public BrowserUserAgentMasterService(IBrowserUserAgentMasterRepository repository) { _repository = repository; }

        private static BrowserUserAgentMasterResponse MapToResponse(BrowserUserAgentMaster e) => new()
        {
            BrowserUserAgentMasterID = e.BrowserUserAgentMasterID,
            UserAgent = e.UserAgent,
        };

        public async Task<List<BrowserUserAgentMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<BrowserUserAgentMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateBrowserUserAgentMasterRequest request)
        {
            var entity = new BrowserUserAgentMaster
            {
                UserAgent = request.UserAgent,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateBrowserUserAgentMasterRequest request)
        {
            var entity = await _repository.GetByID(request.BrowserUserAgentMasterID);
            if (entity == null) return Messages.BrowserUserAgentMasterMessages.NotFound;
            entity.UserAgent = request.UserAgent;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.BrowserUserAgentMasterMessages.NotFound;
        }
    }
}
