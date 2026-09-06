using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.VirtualAccountMaster;
using FintechApp.Responses.VirtualAccountMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class VirtualAccountMasterService : IVirtualAccountMasterService
    {
        private readonly IVirtualAccountMasterRepository _repository;
        public VirtualAccountMasterService(IVirtualAccountMasterRepository repository) { _repository = repository; }
        private static VirtualAccountMasterResponse MapToResponse(VirtualAccountMaster e) => new() { VirtualAccountMasterID = e.VirtualAccountMasterID, Prefix = e.Prefix, BankName = e.BankName, IFSC = e.IFSC, };
        public async Task<List<VirtualAccountMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<VirtualAccountMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateVirtualAccountMasterRequest request) { var entity = new VirtualAccountMaster { Prefix = request.Prefix, BankName = request.BankName, IFSC = request.IFSC, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateVirtualAccountMasterRequest request) { var entity = await _repository.GetByID(request.VirtualAccountMasterID); if (entity == null) return Messages.VirtualAccountMasterMessages.NotFound; entity.Prefix = request.Prefix; entity.BankName = request.BankName; entity.IFSC = request.IFSC; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.VirtualAccountMasterMessages.NotFound; }
    }
}
