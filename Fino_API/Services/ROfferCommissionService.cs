using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ROfferCommission;
using FintechApp.Responses.ROfferCommission;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ROfferCommissionService : IROfferCommissionService
    {
        private readonly IROfferCommissionRepository _repository;
        public ROfferCommissionService(IROfferCommissionRepository repository) { _repository = repository; }

        private static ROfferCommissionResponse MapToResponse(ROfferCommission e) => new()
        {
            ROfferCommissionID = e.ROfferCommissionID,
            SlabMasterID = e.SlabMasterID,
            ServiceID = e.ServiceID,
            OperatorID = e.OperatorID,
            Amount = e.Amount,
            RetailerComm = e.RetailerComm,
            RetailerCommType = e.RetailerCommType,
            RetailerCommissionCategory = e.RetailerCommissionCategory,
            DistributorComm = e.DistributorComm,
            DistributorCommType = e.DistributorCommType,
            DistributorCommissionCategory = e.DistributorCommissionCategory,
            SuperDistributorComm = e.SuperDistributorComm,
            SuperDistributorCommType = e.SuperDistributorCommType,
            SuperDistributorCommissionCategory = e.SuperDistributorCommissionCategory,
            ClientComm = e.ClientComm,
            ClientCommType = e.ClientCommType,
            ClientCommissionCategory = e.ClientCommissionCategory,
            IsActive = e.IsActive,
            IsDelete = e.IsDelete,
        };

        public async Task<List<ROfferCommissionResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ROfferCommissionResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<ROfferCommissionResponse>> GetByOperatorID(int operatorid) { var list = await _repository.GetByOperatorID(operatorid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateROfferCommissionRequest request)
        {
            var entity = new ROfferCommission
            {
                SlabMasterID = request.SlabMasterID,
                ServiceID = request.ServiceID,
                OperatorID = request.OperatorID,
                Amount = request.Amount,
                RetailerComm = request.RetailerComm,
                RetailerCommType = request.RetailerCommType,
                RetailerCommissionCategory = request.RetailerCommissionCategory,
                DistributorComm = request.DistributorComm,
                DistributorCommType = request.DistributorCommType,
                DistributorCommissionCategory = request.DistributorCommissionCategory,
                SuperDistributorComm = request.SuperDistributorComm,
                SuperDistributorCommType = request.SuperDistributorCommType,
                SuperDistributorCommissionCategory = request.SuperDistributorCommissionCategory,
                ClientComm = request.ClientComm,
                ClientCommType = request.ClientCommType,
                ClientCommissionCategory = request.ClientCommissionCategory,
                IsActive = request.IsActive,
                IsDelete = request.IsDelete,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateROfferCommissionRequest request)
        {
            var entity = await _repository.GetByID(request.ROfferCommissionID);
            if (entity == null) return Messages.ROfferCommissionMessages.NotFound;
            entity.SlabMasterID = request.SlabMasterID;
            entity.ServiceID = request.ServiceID;
            entity.OperatorID = request.OperatorID;
            entity.Amount = request.Amount;
            entity.RetailerComm = request.RetailerComm;
            entity.RetailerCommType = request.RetailerCommType;
            entity.RetailerCommissionCategory = request.RetailerCommissionCategory;
            entity.DistributorComm = request.DistributorComm;
            entity.DistributorCommType = request.DistributorCommType;
            entity.DistributorCommissionCategory = request.DistributorCommissionCategory;
            entity.SuperDistributorComm = request.SuperDistributorComm;
            entity.SuperDistributorCommType = request.SuperDistributorCommType;
            entity.SuperDistributorCommissionCategory = request.SuperDistributorCommissionCategory;
            entity.ClientComm = request.ClientComm;
            entity.ClientCommType = request.ClientCommType;
            entity.ClientCommissionCategory = request.ClientCommissionCategory;
            entity.IsActive = request.IsActive;
            entity.IsDelete = request.IsDelete;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ROfferCommissionMessages.NotFound;
        }
    }
}
