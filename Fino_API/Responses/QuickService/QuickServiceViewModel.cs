namespace FintechApp.Responses.QuickService
{
    /// <summary>
    /// One shortcut tile, carrying everything the tile needs to render — so the phone does not
    /// have to fetch the whole catalogue just to find out what a pinned service is called.
    /// </summary>
    public class QuickServiceViewModel
    {
        public int ServiceID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ServiceCategory { get; set; } = string.Empty;
        public string ServiceImagePath { get; set; } = string.Empty;
        public int UISequence { get; set; }

        /// <summary>The service is pinned but currently unusable (KYC pending, or locked by the
        /// operator). The tile is still shown, greyed, rather than vanishing without explanation.</summary>
        public bool IsLocked { get; set; }

        public string LockReason { get; set; } = string.Empty;
    }
}
