using FintechApp.Requests.OperatorSequenceUserWise;
using FintechApp.Responses.OperatorSequence;
using FintechApp.Responses.OperatorSequenceUserWise;
namespace FintechApp.Interfaces.Services
{
    public interface IOperatorSequenceUserWiseService
    {
        Task<List<OperatorSequenceUserWiseResponse>> GetAll();
        Task<List<OperatorSequenceUserWiseResponse>> GetByUserID(int userId);
        Task<OperatorSequenceUserWiseResponse?> GetByID(int id);
        Task<string> Create(CreateOperatorSequenceUserWiseRequest request);
        Task<string> Update(UpdateOperatorSequenceUserWiseRequest request);
        Task<string> Delete(int id);

        // Legacy user-wise switching contract (old OperatorSequenceUserWiseController).
        Task<List<OperatorSequenceGridResponse>> Select(SelectUserWiseSequenceRequest request);
        Task<OperatorSequenceGridResponse?> SelectByOperator(SelectUserWiseByOperatorRequest request);
        Task<string> InsertSequence(InsertUserWiseSequenceRequest request);
        Task<string> DeleteByUser(DeleteUserWiseByUserRequest request);
        Task<string> DeleteAllSequences();
        Task<List<OperatorSequenceGridResponse>> SelectAllUserwiseSequenceList();
    }
}
