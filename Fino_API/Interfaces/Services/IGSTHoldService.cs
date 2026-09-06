using FintechApp.Requests.GSTHold;
using FintechApp.Responses.GSTHold;
namespace FintechApp.Interfaces.Services
{
    public interface IGSTHoldService
    {
        Task<List<GSTHoldResponse>> GetAll();
        Task<GSTHoldResponse?> GetByID(int id);
        Task<List<GSTHoldResponse>> GetByUserID(int userID);
        Task<string> Create(CreateGSTHoldRequest request);
        Task<string> Update(UpdateGSTHoldRequest request);
        Task<string> Delete(int id);
    }
}
