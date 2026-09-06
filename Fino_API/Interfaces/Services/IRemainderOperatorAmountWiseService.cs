using FintechApp.Requests.RemainderOperatorAmountWise;
using FintechApp.Responses.RemainderOperatorAmountWise;
namespace FintechApp.Interfaces.Services
{
    public interface IRemainderOperatorAmountWiseService
    {
        Task<List<RemainderOperatorAmountWiseResponse>> GetAll();
        Task<RemainderOperatorAmountWiseResponse?> GetByID(int id);

        Task<string> Create(CreateRemainderOperatorAmountWiseRequest request);
        Task<string> Update(UpdateRemainderOperatorAmountWiseRequest request);
        Task<string> Delete(int id);
    }
}
