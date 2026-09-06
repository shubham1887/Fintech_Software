using FintechApp.Responses.Menu;

namespace FintechApp.Interfaces.Services
{
    // Returns the sidebar/navigation menu filtered for a given user-type and the
    // currently-active services (service-gated items disappear when their service is off).
    public interface IMenuService
    {
        Task<List<MenuItem>> GetMenu(int userTypeId, int userId = 0);
    }
}
