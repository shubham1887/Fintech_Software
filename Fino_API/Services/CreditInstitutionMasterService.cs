using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CreditInstitutionMaster;
using FintechApp.Responses.CreditInstitutionMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class CreditInstitutionMasterService : ICreditInstitutionMasterService
    {
        private readonly ICreditInstitutionMasterRepository _repository;
        public CreditInstitutionMasterService(ICreditInstitutionMasterRepository repository) { _repository = repository; }
        private static CreditInstitutionMasterResponse MapToResponse(CreditInstitutionMaster e) => new() { CreditInstitutionMasterID = e.CreditInstitutionMasterID, Name = e.Name, MerchantID = e.MerchantID, BranchID = e.BranchID, AccountNumber = e.AccountNumber, Token = e.Token, EncryptionKey = e.EncryptionKey, IsActive = e.IsActive, IsActiveWeb = e.IsActiveWeb, };
        public async Task<List<CreditInstitutionMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<CreditInstitutionMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateCreditInstitutionMasterRequest request) { var entity = new CreditInstitutionMaster { Name = request.Name, MerchantID = request.MerchantID, BranchID = request.BranchID, AccountNumber = request.AccountNumber, Token = request.Token, EncryptionKey = request.EncryptionKey, IsActive = request.IsActive, IsActiveWeb = request.IsActiveWeb, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateCreditInstitutionMasterRequest request) { var entity = await _repository.GetByID(request.CreditInstitutionMasterID); if (entity == null) return Messages.CreditInstitutionMasterMessages.NotFound; entity.Name = request.Name; entity.MerchantID = request.MerchantID; entity.BranchID = request.BranchID; entity.AccountNumber = request.AccountNumber; entity.Token = request.Token; entity.EncryptionKey = request.EncryptionKey; entity.IsActive = request.IsActive; entity.IsActiveWeb = request.IsActiveWeb; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.CreditInstitutionMasterMessages.NotFound; }
    }
}
