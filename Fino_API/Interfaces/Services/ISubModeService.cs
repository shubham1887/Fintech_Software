using FintechApp.Requests.SubMode;
using FintechApp.Responses.SubMode;

namespace FintechApp.Interfaces.Services
{
    public interface ISubModeService
    {
        // All non-deleted sub modes (admin grid).
        Task<List<SubModeResponse>> GetAll();
        // Active sub modes for a mode, for the Fund Request dependent "Sub Mode" dropdown.
        Task<List<SubModeResponse>> GetByModeID(int modeId);
        Task<SubModeResponse?> GetByID(int subModeId);
        Task<string> Create(CreateSubModeRequest request, int userId);
        Task<string> Update(UpdateSubModeRequest request, int userId);
        Task<string> Delete(int subModeId);
    }
}
