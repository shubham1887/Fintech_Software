namespace FintechApp.Requests.PartyState
{
    /// <summary>Write the derived place of supply onto the parties that can have one.</summary>
    public class PartyStateBackfillRequest
    {
        /// <summary>Work it out and report it, but write nothing. The first run over a real
        /// install is the moment somebody finds out how many parties cannot be derived, and that
        /// number should be readable before any row is written.</summary>
        public bool DryRun { get; set; }

        /// <summary>Limit to one party, for re-running a single case. 0 is everybody.</summary>
        public int OnlyUserID { get; set; }

        public int CreatedByUserID { get; set; }
    }

    /// <summary>Cancel the documents carrying an invalid place of supply, so they can be made again.</summary>
    public class DocumentRegenerationRequest
    {
        public string CompanyGSTIN { get; set; } = string.Empty;

        /// <summary>yyyyMM. One period at a time, deliberately: this cancels real documents, and a
        /// blanket run over every period is not something anyone should be able to do by accident.</summary>
        public string PeriodKey { get; set; } = string.Empty;

        /// <summary>Report what would be cancelled and change nothing.</summary>
        public bool DryRun { get; set; }

        public int CreatedByUserID { get; set; }
    }

    /// <summary>Set one party's place of supply by hand.</summary>
    public class SetPartyStateRequest
    {
        public int UserID { get; set; }

        /// <summary>The two-digit GST state code. Empty CLEARS it, which is honest when nobody
        /// knows the state — it is never written as "00".</summary>
        public string PlaceOfSupplyStateCode { get; set; } = string.Empty;

        public int CreatedByUserID { get; set; }
    }
}
