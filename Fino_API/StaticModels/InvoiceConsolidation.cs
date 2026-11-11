namespace FintechApp.StaticModels
{
    /// <summary>
    /// How many sale documents a period's wallet top-ups become.
    ///
    /// This is a real business decision and it was previously a literal: a registered buyer got
    /// one invoice per top-up, an unregistered one got one per day, and nobody was ever asked.
    /// On a month where a single distributor funds their wallet most days, that is thirty
    /// invoices for one customer — correct under the old rule, and not what the business wanted
    /// to send or what the customer wanted to receive.
    ///
    /// The options are not interchangeable in law, which is why the choice is recorded rather
    /// than inferred:
    ///
    ///   TRANSACTION — one document per top-up. The most defensible for a registered buyer:
    ///                 they can match it line by line against their own purchase register.
    ///   DAILY       — one document per party per day. What GST contemplates for B2C supplies,
    ///                 and what stops a busy counter producing forty documents a day.
    ///   MONTHLY     — one document per party for the whole period. Clean for the customer and
    ///                 the one most people mean by "monthly billing". For an UNREGISTERED party
    ///                 this is straightforwardly fine — those supplies are reported consolidated
    ///                 in GSTR-1 table 7 anyway. For a REGISTERED one it rests on treating the
    ///                 supply as continuous (s.31(5)), so the generator says so rather than
    ///                 letting it pass silently.
    ///   AUTO        — the shipped behaviour: TRANSACTION for registered, DAILY for unregistered.
    /// </summary>
    public static class InvoiceConsolidation
    {
        public const string Auto = "AUTO";
        public const string Transaction = "TRANSACTION";
        public const string Daily = "DAILY";
        public const string Monthly = "MONTHLY";

        /// <summary>The Key Setting that holds this install's answer.</summary>
        public const string SettingKey = "InvoiceConsolidation";

        public static readonly string[] All = { Auto, Transaction, Daily, Monthly };

        /// <summary>
        /// Normalises whatever arrived — a blank, a stray case, an unknown word — to one of the
        /// four. An unrecognised value becomes AUTO rather than an error: the shipped behaviour
        /// is the safe answer, and refusing to invoice a month because a setting was mistyped
        /// would be a worse failure than invoicing it the way it has always been invoiced.
        /// </summary>
        public static string Normalise(string? value)
        {
            string v = (value ?? string.Empty).Trim().ToUpperInvariant();
            return All.Contains(v) ? v : Auto;
        }
    }
}
