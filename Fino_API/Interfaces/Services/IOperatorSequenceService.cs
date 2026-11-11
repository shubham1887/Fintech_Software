using FintechApp.Requests.OperatorSequence;
using FintechApp.Responses.OperatorSequence;
namespace FintechApp.Interfaces.Services
{
    public interface IOperatorSequenceService
    {
        Task<List<OperatorSequenceResponse>> GetAll();
        Task<OperatorSequenceResponse?> GetByID(int id);
        Task<List<OperatorSequenceResponse>> GetByOperatorID(int operatorID);
        Task<string> Create(CreateOperatorSequenceRequest request);
        Task<string> Update(UpdateOperatorSequenceRequest request);
        Task<string> Delete(int id);

        // Legacy provider-sequence grid contract (old OperatorSequenceController).
        Task<List<OperatorSequenceGridResponse>> SelectByService(int serviceID);
        Task<string> InsertSequence(InsertOperatorSequenceRequest request);
        Task<string> BulkUpdate(List<BulkUpdateOperatorSequenceRequest> requests);
        Task<List<SequenceProviderItemResponse>> SelectProviderForBulkSequence(int serviceID);
        Task<List<SequenceServiceResponse>> SelectRoutableServices();
        Task<List<SequenceProviderDropdownResponse>> SelectForViewApiDropdown();
    }
}
