using FintechApp.Requests.KycDocumentMaster;
using FintechApp.Responses.KycDocumentMaster;

namespace FintechApp.Interfaces.Services
{
    public interface IKycDocumentMasterService
    {
        Task<List<KycDocumentMasterResponse>> GetAll();
        Task<KycDocumentMasterResponse?> GetByID(int id);
        Task<string> Create(CreateKycDocumentMasterRequest request);
        Task<string> Update(UpdateKycDocumentMasterRequest request);
        Task<string> Delete(int id);
    }
}
