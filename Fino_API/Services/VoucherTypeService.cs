using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.VoucherType;
using FintechApp.Responses.VoucherType;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class VoucherTypeService : IVoucherTypeService
    {
        private readonly IVoucherTypeRepository _repository;

        public VoucherTypeService(IVoucherTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<VoucherTypeResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<VoucherTypeResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateVoucherTypeRequest request)
        {
            var entity = new VoucherType
            {
                VoucherTypeName = request.VoucherTypeName,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateVoucherTypeRequest request)
        {
            var entity = await _repository.GetByID(request.VoucherTypeID);
            if (entity == null) return VoucherTypeMessages.NotFound;
            entity.VoucherTypeName = request.VoucherTypeName;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : VoucherTypeMessages.NotFound;
        }

        private static VoucherTypeResponse MapToResponse(VoucherType e) => new()
        {
            VoucherTypeID = e.VoucherTypeID,
            VoucherTypeName = e.VoucherTypeName,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
