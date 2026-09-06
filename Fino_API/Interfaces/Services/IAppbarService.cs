using FintechApp.Responses.Appbar;

namespace FintechApp.Interfaces.Services
{
    public interface IAppbarService
    {
        Task<AppbarResponse> Refresh(int userId);
        Task<AppbarResponse> RefreshSD(int userId);
    }
}
