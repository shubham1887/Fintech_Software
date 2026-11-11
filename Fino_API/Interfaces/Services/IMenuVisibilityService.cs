using FintechApp.Responses.Menu;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Which menu entries this install has chosen to hide.
    ///
    /// Presentation only: it decides what the sidebar lists, never what an endpoint will serve.
    /// That remains <c>ApiAccessPolicy</c>'s job, and keeping the two apart is deliberate — a
    /// menu switch that quietly doubled as a permission would be a permission nobody audits.
    /// </summary>
    public interface IMenuVisibilityService
    {
        /// <summary>
        /// True when this entry should be drawn for this role.
        ///
        /// A role-specific decision beats the all-roles one, so an entry can be hidden everywhere
        /// and kept for a single role. No decision at all means visible — the shipped definition
        /// stands until somebody overrides it.
        /// </summary>
        Task<bool> IsVisible(string menuKey, int userTypeId);

        /// <summary>Every stored decision, for the admin screen.</summary>
        Task<List<MenuVisibilityItem>> GetAll();

        /// <summary>
        /// The whole menu as the ADMIN definition has it, each entry carrying its current
        /// decision — the list the admin screen is built from. Role 0 shows the all-roles state.
        /// </summary>
        Task<List<MenuVisibilityItem>> GetCatalog(int userTypeId);

        /// <summary>
        /// Records decisions. An entry set back to visible for the same scope removes the
        /// exception rather than storing a second one, so the table never grows into a log of
        /// toggles and "no row" always means "as shipped".
        /// </summary>
        Task<(string failureMessage, int changed)> Save(List<MenuVisibilityItem> items, int updatedByUserID);
    }
}
