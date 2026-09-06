using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CircleWiseCommission;
using FintechApp.Responses.CircleWiseCommission;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class CircleWiseCommissionService : ICircleWiseCommissionService
    {
        private readonly ICircleWiseCommissionRepository _repository;

        public CircleWiseCommissionService(ICircleWiseCommissionRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CircleWiseCommissionResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<CircleWiseCommissionResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<CircleWiseCommissionResponse>> GetBySlabMasterID(int slabMasterID)
        {
            var list = await _repository.GetBySlabMasterID(slabMasterID);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<string> Create(CreateCircleWiseCommissionRequest request)
        {
            var entity = new CircleWiseCommission
            {
                SlabMasterID = request.SlabMasterID,
                OperatorID = request.OperatorID,
                StateID = request.StateID,
                FromAmount = request.FromAmount,
                ToAmount = request.ToAmount,
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
                ClientCommissionCategory = request.ClientCommissionCategory
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateCircleWiseCommissionRequest request)
        {
            var entity = await _repository.GetByID(request.CircleWiseCommissionID);
            if (entity == null) return Messages.CircleWiseCommissionMessages.NotFound;
            entity.SlabMasterID = request.SlabMasterID;
            entity.OperatorID = request.OperatorID;
            entity.StateID = request.StateID;
            entity.FromAmount = request.FromAmount;
            entity.ToAmount = request.ToAmount;
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
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.CircleWiseCommissionMessages.NotFound;
        }

        private static CircleWiseCommissionResponse MapToResponse(CircleWiseCommission e) => new()
        {
            CircleWiseCommissionID = e.CircleWiseCommissionID,
            SlabMasterID = e.SlabMasterID,
            SlabName = e.SlabMaster?.SlabName,
            OperatorID = e.OperatorID,
            OperatorName = e.OperatorMaster?.OperatorName,
            StateID = e.StateID,
            FromAmount = e.FromAmount,
            ToAmount = e.ToAmount,
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
            ClientCommissionCategory = e.ClientCommissionCategory
        };
    }
}
