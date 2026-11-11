namespace FintechApp.Responses.OperatorSequence
{
    // Provider cell of the sequence grid: every PAY-capable provider of the service,
    // IsActive marks membership in the operator's sequence, Count is its priority weight.
    public class SequenceProviderItemResponse
    {
        public int OperatorID { get; set; }
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public int Count { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// This operator is actually mapped to this provider (an active ProviderConfiguration
        /// route, carrying the provider's own operator code).
        ///
        /// A sequence entry alone routes nothing: dispatch walks the operator's ROUTES and only
        /// orders them by the sequence. Ticking a provider that has no route is therefore a
        /// silent no-op — the commonest way a switch is "configured" and never takes effect — so
        /// the grid says which cells are not wired up yet.
        /// </summary>
        public bool HasRoute { get; set; }

        /// <summary>
        /// This provider has at least one active TRANSACTION response rule, so the gateway can
        /// read what it answers.
        ///
        /// Without rules nothing it says can ever be matched, so the gateway skips it and moves to
        /// the next provider in the sequence - which means a provider ticked here does nothing at
        /// all. It used to be worse: the reply came back unmatched, the transaction was held as
        /// PENDING, and the rest of the sequence was never tried, so one half-configured provider
        /// at the front of a sequence silently parked every transaction behind it.
        /// </summary>
        public bool HasResponseRules { get; set; }
    }

    // One grid row per operator (mirrors the legacy OperatorSequenceResponse shape the UI expects).
    public class OperatorSequenceGridResponse
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public List<SequenceProviderItemResponse> ProviderList { get; set; } = new();
        public int RealProviderID { get; set; }
        public int ViewProviderID { get; set; }
        public int OperatorSequenceUserWiseID { get; set; }
    }

    public class SequenceProviderDropdownResponse
    {
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
    }

    // A service the Provider Routing picker may offer. Family is the rail it routes on
    // (RECHARGE / BBPS), so the picker can group billers apart from recharge, and
    // OperatorCount is why it is listed at all.
    public class SequenceServiceResponse
    {
        public int ServiceID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public int OperatorCount { get; set; }
    }
}
