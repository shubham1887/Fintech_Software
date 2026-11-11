namespace FintechApp.Requests.User
{
    public class UserSelectRequest
    {
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public int UserID { get; set; }
        public string Search { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int SkipCount { get; set; }
        public int TakeCount { get; set; } = 20;
        public bool IsExcelDownload { get; set; }
        public string SortColumnName { get; set; } = string.Empty;
        public string SortDirection { get; set; } = string.Empty;
        public bool ShowDeleted { get; set; }

        /// <summary>
        /// Return ONLY soft-deleted rows (the Deleted Users screen). Implies ShowDeleted, and it
        /// wins over it: with this on, a live user is never in the result.
        /// </summary>
        public bool OnlyDeleted { get; set; }

        /// <summary>
        /// Tri-state activity filter: true = active only, false = inactive only (the Inactive
        /// Users screen), null = both. Deleted rows are governed by ShowDeleted/OnlyDeleted, not
        /// by this — a deleted user is neither "active" nor "inactive" here.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// KYC filter: APPROVED / PENDING / REJECTED / NOTSUBMITTED. Empty = no filter.
        /// </summary>
        public string KYCStatus { get; set; } = string.Empty;
    }

    public class UserSelectByIDRequest
    {
        public int TargetUserID { get; set; }
    }

    public class UserDDLRequest
    {
        public int UserTypeID { get; set; }
        public int ParentID { get; set; }
        public string Search { get; set; } = string.Empty;
        public int WalletID { get; set; }
    }

    /// <summary>
    /// Apply one action to many users at once.
    ///
    /// Targets come either as an explicit list, or as "everyone under this user" — moving a whole
    /// branch of the network is the case this exists for, and picking forty retailers one by one is
    /// not a workflow. Both can be combined; the two sets are merged.
    /// </summary>
    public class UserBulkUpdateRequest
    {
        /// <summary>Explicitly chosen users.</summary>
        public List<int> UserIDs { get; set; } = new();

        /// <summary>Take everyone below this user in the network as well (any depth).</summary>
        public int SourceUserID { get; set; }

        /// <summary>Include <see cref="SourceUserID"/> itself, not just its downline.</summary>
        public bool IncludeSourceUser { get; set; }

        /// <summary>Narrow the downline to one role — "all retailers under this distributor".</summary>
        public int UserTypeID { get; set; }

        /// <summary>Activate, Deactivate, HoldCommission, ReleaseCommission or Shift.</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>For Shift: the user the targets are moved under.</summary>
        public int NewParentID { get; set; }

        public string Remark { get; set; } = string.Empty;

        // Older callers set these two directly instead of naming an Action.
        public bool IsActive { get; set; }
        public bool IsCommissionHold { get; set; }
    }

    public class UserSendPasswordRequest
    {
        public int TargetUserID { get; set; }
    }

    public class UserAddHoldBalanceRequest
    {
        public int TargetUserID { get; set; }

        /// <summary>How much to freeze (or, with <see cref="IsRelease"/>, to unfreeze).</summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Which wallet to hold against. A hold freezes spendable balance, and balance lives per
        /// wallet — a user with AEPS and Recharge wallets can legitimately need one held and the
        /// other free — so the caller names the wallet. 0 = the user's primary wallet.
        /// </summary>
        public int WalletID { get; set; }

        /// <summary>true = release this much of the existing hold instead of adding to it.</summary>
        public bool IsRelease { get; set; }

        /// <summary>Why. Recorded on the hold history, which is what the hold report reads.</summary>
        public string Remark { get; set; } = string.Empty;
    }

    /// <summary>Filter for the wallet-hold history report.</summary>
    public class WalletHoldHistoryRequest
    {
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        /// <summary>HOLD, RELEASE, or empty/ALL for both.</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// The shared report screen sends its dropdown as "Status". Accepting it here means the hold
        /// report needs no special case on the frontend — it is just another report.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Whichever of the two the caller filled in.</summary>
        public string EffectiveAction =>
            !string.IsNullOrWhiteSpace(Action) ? Action : Status;

        public int SkipCount { get; set; }
        public int TakeCount { get; set; }
    }

    public class UserDeactivate2FARequest
    {
        public int TargetUserID { get; set; }
        public string OTP { get; set; } = string.Empty;
    }

    public class UserFetchPincodeRequest
    {
        public string Pincode { get; set; } = string.Empty;
    }

    public class UserLoginAsRequest
    {
        public int TargetUserID { get; set; }
    }

    public class UserGSTRequest
    {
        public int TargetUserID { get; set; }
    }
}
