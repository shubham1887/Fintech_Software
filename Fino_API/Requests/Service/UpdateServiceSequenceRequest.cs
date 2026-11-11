namespace FintechApp.Requests.Service
{
    /// <summary>
    /// The admin's chosen display order for the service grid, sent as one list.
    ///
    /// Ordering is inherently a statement about the whole set — "this one is first, that one is
    /// second" — so it is saved as a whole. Sending it a row at a time through the field-level
    /// Service/Update made reordering N services N round-trips, each of which left the catalogue
    /// in a half-renumbered state that the retailer app could read.
    /// </summary>
    public class UpdateServiceSequenceRequest
    {
        public List<ServiceSequenceItem> Items { get; set; } = new();
    }

    public class ServiceSequenceItem
    {
        public int ServiceID { get; set; }

        /// <summary>Position in the grid; lower shows first. 1-based, as the admin sees it.</summary>
        public int UISequence { get; set; }
    }
}
