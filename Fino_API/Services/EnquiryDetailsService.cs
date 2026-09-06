using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.EnquiryDetails;
using FintechApp.Responses.EnquiryDetails;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class EnquiryDetailsService : IEnquiryDetailsService
    {
        private readonly IEnquiryDetailsRepository _repository;
        public EnquiryDetailsService(IEnquiryDetailsRepository repository) { _repository = repository; }

        private static EnquiryDetailsResponse MapToResponse(EnquiryDetails e) => new()
        {
            EnquiryDetailsID = e.EnquiryDetailsID,
            PersonName = e.PersonName,
            MobileNumber = e.MobileNumber,
            EmailID = e.EmailID,
            Address = e.Address,
            EnquiryType = e.EnquiryType,
            Remark = e.Remark,
            AdminStatus = e.AdminStatus,
            AdminRemark = e.AdminRemark,
        };

        public async Task<List<EnquiryDetailsResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<EnquiryDetailsResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateEnquiryDetailsRequest request)
        {
            var entity = new EnquiryDetails
            {
                PersonName = request.PersonName,
                MobileNumber = request.MobileNumber,
                EmailID = request.EmailID,
                Address = request.Address,
                EnquiryType = request.EnquiryType,
                Remark = request.Remark,
                AdminStatus = request.AdminStatus,
                AdminRemark = request.AdminRemark,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateEnquiryDetailsRequest request)
        {
            var entity = await _repository.GetByID(request.EnquiryDetailsID);
            if (entity == null) return Messages.EnquiryDetailsMessages.NotFound;
            entity.PersonName = request.PersonName;
            entity.MobileNumber = request.MobileNumber;
            entity.EmailID = request.EmailID;
            entity.Address = request.Address;
            entity.EnquiryType = request.EnquiryType;
            entity.Remark = request.Remark;
            entity.AdminStatus = request.AdminStatus;
            entity.AdminRemark = request.AdminRemark;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.EnquiryDetailsMessages.NotFound;
        }
    }
}
