namespace FintechApp.Responses.WorkQueue
{
    /// <summary>
    /// One queue in the counts strip: how many items are waiting, and where to go to work them.
    ///
    /// The client renders an icon and the number — nothing else — so everything it needs to draw
    /// and route the chip is on this row. The <see cref="Key"/> is what the client maps to an
    /// icon; adding a queue is a new row here plus one icon in the client's map.
    /// </summary>
    public class WorkQueueCount
    {
        /// <summary>Stable identifier, e.g. "pending-recharge". Never localised, never renumbered.</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>Tooltip / accessible name, e.g. "Pending recharges".</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>How many items are waiting for this user right now.</summary>
        public int Count { get; set; }

        /// <summary>The menu URL that works this queue, e.g. "/Pending". The client routes it.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// "danger" | "warning" | "info" | "success" — how urgent the chip should look when the
        /// count is above zero. Money waiting is not the same as an unread announcement.
        /// </summary>
        public string Tone { get; set; } = "info";
    }

    /// <summary>Every queue this caller is entitled to see, already role-scoped by the server.</summary>
    public class WorkQueueResponse
    {
        public List<WorkQueueCount> Queues { get; set; } = new();

        /// <summary>Sum across the queues — the one number worth showing on a collapsed strip.</summary>
        public int Total { get; set; }
    }
}
