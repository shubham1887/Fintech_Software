namespace FintechApp.Requests.Live
{
    /// <summary>
    /// The only thing a live-summary poll carries.
    ///
    /// Deliberately no user id: the caller is whoever the signed token says they are. A request
    /// that could name a user would turn the one endpoint every screen polls into the easiest way
    /// in the product to read somebody else's balance.
    /// </summary>
    public class LiveSummaryRequest
    {
        /// <summary>
        /// The <c>Version</c> from the client's last answer, or empty on the first poll. When it
        /// still matches, the server replies "not modified" and sends no payload.
        /// </summary>
        public string Version { get; set; } = string.Empty;
    }
}
