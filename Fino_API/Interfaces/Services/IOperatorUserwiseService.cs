using FintechApp.Requests.OperatorUserwise;
using FintechApp.Responses.OperatorUserwise;
namespace FintechApp.Interfaces.Services
{
    public interface IOperatorUserwiseService
    {
        Task<List<OperatorUserwiseResponse>> GetAll();
        Task<List<OperatorUserwiseResponse>> GetByUserID(int userId);
        Task<OperatorUserwiseResponse?> GetByID(int id);
        Task<string> Create(CreateOperatorUserwiseRequest request);
        Task<string> Update(UpdateOperatorUserwiseRequest request);
        Task<string> Delete(int id);
    }
}
