namespace FintechApp.StaticModels
{
    /// <summary>
    /// The single, versioned contract every API-user (B2B) endpoint speaks.
    ///
    /// One shape is returned by the three transaction touch-points — the INITIAL request
    /// (/api/v1/recharge), the STATUS CHECK (/api/v1/status) and the CALLBACK we push back —
    /// so an integrator writes one parser and reuses it everywhere. Balance and operator
    /// listings share the same outer envelope (status / statuscode / errorcode / message)
    /// and only differ in their payload block.
    ///
    /// Nothing here is derived at runtime: the constants below ARE the published contract,
    /// so changing a value is a breaking change and must be versioned (/api/v2).
    /// </summary>
    public static class ApiUserContract
    {
        /// <summary>Contract version echoed on every response, so integrators can pin/branch.</summary>
        public const string Version = "1.0";
    }

    /// <summary>
    /// The only four values <c>status</c> can ever hold. SUCCESS/PENDING/FAILED describe the
    /// TRANSACTION; ERROR means the request never became a transaction (auth/validation/system).
    /// </summary>
    public static class ApiUserStatus
    {
        /// <summary>Final. Consumer served, wallet debited.</summary>
        public const string Success = "SUCCESS";

        /// <summary>Not final. Accepted and debited; settles later via status-check or callback.</summary>
        public const string Pending = "PENDING";

        /// <summary>Final. Declined by the operator — amount refunded to the API user's wallet.</summary>
        public const string Failed = "FAILED";

        /// <summary>No transaction exists. Authentication, validation or system failure.</summary>
        public const string Error = "ERROR";

        /// <summary>Normalises any internal status spelling (FAILURE/PROCESS/REFUND/…) to the contract.</summary>
        public static string Normalize(string? status)
        {
            string s = (status ?? string.Empty).Trim().ToUpperInvariant();
            return s switch
            {
                "SUCCESS" or "SUCCESSFUL" => Success,
                "PENDING" or "PROCESS" or "PROCESSING" or "INITIATED" => Pending,
                "FAILED" or "FAILURE" or "FAIL" or "REFUND" or "REFUNDED" or "REVERSED" => Failed,
                "" => Pending,
                _ => Pending,
            };
        }

        /// <summary>True once the status can no longer change (SUCCESS or FAILED).</summary>
        public static bool IsFinal(string? status)
        {
            string s = Normalize(status);
            return s == Success || s == Failed;
        }
    }

    /// <summary>
    /// Numeric mirror of <see cref="ApiUserStatus"/> plus the reason class, for integrators whose
    /// stack prefers switching on an int. Always travels alongside the string <c>status</c>.
    /// </summary>
    public static class ApiUserStatusCode
    {
        public const int Success = 0;
        public const int Pending = 1;
        public const int Failed = 2;
        public const int InvalidRequest = 3;
        public const int Unauthorized = 4;
        public const int NotFound = 5;
        public const int SystemError = 9;
    }

    /// <summary>
    /// Stable machine-readable reason codes. Empty string on success/pending/failed transactions
    /// that carry no error; never localised, never reworded — <c>message</c> is the human text.
    /// </summary>
    public static class ApiUserErrorCode
    {
        public const string None = "";

        // ── Authentication / authorisation ───────────────────────────────
        public const string UserIdRequired = "AUTH_USERID_REQUIRED";
        public const string TokenRequired = "AUTH_TOKEN_REQUIRED";
        public const string UserNotFound = "AUTH_USER_NOT_FOUND";
        public const string NotApiUser = "AUTH_NOT_API_USER";
        public const string AccountInactive = "AUTH_ACCOUNT_INACTIVE";
        public const string InvalidToken = "AUTH_INVALID_TOKEN";
        public const string IpNotWhitelisted = "AUTH_IP_NOT_WHITELISTED";

        // ── Request validation ───────────────────────────────────────────
        public const string RequestIdRequired = "VAL_REQUESTID_REQUIRED";
        public const string NumberRequired = "VAL_NUMBER_REQUIRED";
        public const string AmountInvalid = "VAL_AMOUNT_INVALID";
        public const string OperatorRequired = "VAL_OPERATOR_REQUIRED";
        public const string OperatorInvalid = "VAL_OPERATOR_INVALID";

        // ── Transaction outcome ──────────────────────────────────────────
        /// <summary>Rejected before a transaction was created (balance, limits, PIN, operator off…).</summary>
        public const string TransactionRejected = "TXN_REJECTED";
        /// <summary>Status check found no transaction for the given requestid / txnid.</summary>
        public const string TransactionNotFound = "TXN_NOT_FOUND";

        // ── System ───────────────────────────────────────────────────────
        public const string SystemError = "SYS_ERROR";
    }
}
