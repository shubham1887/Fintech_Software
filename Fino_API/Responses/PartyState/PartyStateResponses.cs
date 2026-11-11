namespace FintechApp.Responses.PartyState
{
    /// <summary>Where a party's place of supply came from. The source matters as much as the
    /// value: a code derived from a dropdown is worth less scrutiny than one nobody can find.</summary>
    public static class PartyStateSource
    {
        /// <summary>Set deliberately on the tax profile.</summary>
        public const string Profile = "Profile";

        /// <summary>The user's own GST state field, from onboarding.</summary>
        public const string UserField = "UserField";

        /// <summary>The first two digits of the party's GSTIN.</summary>
        public const string Gstin = "GSTIN";

        /// <summary>The state picked from the address dropdown at onboarding.</summary>
        public const string OnboardingState = "OnboardingState";

        /// <summary>Nothing on record. These are the rows that need a human.</summary>
        public const string None = "None";
    }

    public class PartyStateRow
    {
        public int UserID { get; set; }
        public int UserTypeID { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;
        public string RegistrationStatus { get; set; } = string.Empty;

        /// <summary>What is recorded on the tax profile right now, if anything.</summary>
        public string ProfileStateCode { get; set; } = string.Empty;

        /// <summary>What the resolver would use today, from whichever source ranks highest.</summary>
        public string ResolvedStateCode { get; set; } = string.Empty;
        public string ResolvedStateName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;

        /// <summary>True when a code can be derived but is not yet on the profile — the rows the
        /// backfill will write.</summary>
        public bool CanBackfill { get; set; }

        /// <summary>True when nothing anywhere knows this party's state.</summary>
        public bool NeedsAttention { get; set; }

        /// <summary>Documents already issued to this party. A party with documents cannot simply
        /// be corrected — the documents carry the old code until they are reissued.</summary>
        public int IssuedDocuments { get; set; }
    }

    public class PartyStateReport
    {
        public List<PartyStateRow> Parties { get; set; } = new();

        public int Total { get; set; }
        public int AlreadySet { get; set; }
        public int CanBackfill { get; set; }
        public int NeedsAttention { get; set; }

        /// <summary>Documents already carrying a place of supply the portal would reject. Zero is
        /// the happy case and means the fix can simply be applied before anything is generated.</summary>
        public int DocumentsWithBadPlaceOfSupply { get; set; }
    }

    public class DocumentRegenerationResult
    {
        public bool DryRun { get; set; }
        public string PeriodKey { get; set; } = string.Empty;
        public int DocumentsCancelled { get; set; }
        public decimal TaxableValueCancelled { get; set; }
        public List<string> DocumentNumbers { get; set; } = new();

        /// <summary>What to do next, in words — cancelling is only half the remedy.</summary>
        public string NextStep { get; set; } = string.Empty;

        public List<string> Messages { get; set; } = new();
    }

    public class PartyStateBackfillResult
    {
        public bool DryRun { get; set; }
        public int ProfilesCreated { get; set; }
        public int ProfilesUpdated { get; set; }
        public int StillUnknown { get; set; }
        public List<string> Messages { get; set; } = new();
    }
}
