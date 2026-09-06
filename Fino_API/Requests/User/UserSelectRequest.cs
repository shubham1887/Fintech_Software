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

    public class UserBulkUpdateRequest
    {
        public List<int> UserIDs { get; set; } = new();
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
        public decimal Amount { get; set; }
        /// <summary>Optional — which wallet to hold against; 0 = the user's primary wallet.</summary>
        public int WalletID { get; set; }
        public string Remark { get; set; } = string.Empty;
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
