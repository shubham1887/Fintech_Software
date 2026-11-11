namespace FintechApp.Requests.QuickService
{
    /// <summary>
    /// The caller's chosen shortcut tiles, in the order they want them.
    ///
    /// There is deliberately no UserID here: a user edits their OWN shortcuts and nobody else's,
    /// so the id comes from the signed token. An empty list is a valid save — it means "clear my
    /// shortcuts" — which is why this is a replace rather than an add/remove pair.
    /// </summary>
    public class SaveQuickServiceRequest
    {
        public List<int> ServiceIDs { get; set; } = new();
    }
}
