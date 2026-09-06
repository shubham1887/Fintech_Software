using FintechApp.Requests.DateFormatMaster;
using FintechApp.Responses.DateFormatMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IDateFormatMasterService
    {
        Task<List<DateFormatMasterResponse>> GetAll();
        Task<DateFormatMasterResponse?> GetByID(int id);
        Task<string> Create(CreateDateFormatMasterRequest request);
        Task<string> Update(UpdateDateFormatMasterRequest request);
        Task<string> Delete(int id);
    }
}
