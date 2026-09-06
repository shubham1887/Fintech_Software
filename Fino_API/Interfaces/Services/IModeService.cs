using FintechApp.Requests.Mode;
using FintechApp.Responses.Mode;

namespace FintechApp.Interfaces.Services
{
    public interface IModeService
    {
        // All non-deleted modes (admin grid).
        Task<List<ModeResponse>> GetAll();
        // Active modes only, for the Fund Request "Mode" dropdown.
        Task<List<ModeResponse>> GetActive();
        Task<ModeResponse?> GetByID(int modeId);
        Task<string> Create(CreateModeRequest request, int userId);
        Task<string> Update(UpdateModeRequest request, int userId);
        Task<string> Delete(int modeId);
    }
}
