using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.KycDocumentMaster;
using FintechApp.Responses.KycDocumentMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class KycDocumentMasterService : IKycDocumentMasterService
    {
        private readonly IKycDocumentMasterRepository _repository;

        public KycDocumentMasterService(IKycDocumentMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<KycDocumentMasterResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<KycDocumentMasterResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateKycDocumentMasterRequest request)
        {
            var entity = new KycDocumentMaster
            {
                Label = request.Label,
                DocumentType = request.DocumentType,
                Remark = request.Remark,
                IsRequired = request.IsRequired,
                RegularRxpression = request.RegularRxpression,
                SampleImageURL = request.SampleImageURL
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateKycDocumentMasterRequest request)
        {
            var entity = await _repository.GetByID(request.KycDocumentMasterID);
            if (entity == null) return Messages.KycDocumentMasterMessages.NotFound;
            entity.Label = request.Label;
            entity.DocumentType = request.DocumentType;
            entity.Remark = request.Remark;
            entity.IsRequired = request.IsRequired;
            entity.RegularRxpression = request.RegularRxpression;
            entity.SampleImageURL = request.SampleImageURL;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.KycDocumentMasterMessages.NotFound;
        }

        private static KycDocumentMasterResponse MapToResponse(KycDocumentMaster e) => new()
        {
            KycDocumentMasterID = e.KycDocumentMasterID,
            Label = e.Label,
            DocumentType = e.DocumentType,
            Remark = e.Remark,
            IsRequired = e.IsRequired,
            RegularRxpression = e.RegularRxpression,
            SampleImageURL = e.SampleImageURL
        };
    }
}
