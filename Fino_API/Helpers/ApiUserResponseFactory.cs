using FintechApp.Responses.ApiUser;
using FintechApp.StaticModels;

namespace FintechApp.Helpers
{
    /// <summary>
    /// Pure (dependency-free) shaping of the API-user envelope. Every failure reply in the
    /// B2B API is produced here so that an auth failure on /balance is byte-for-byte the same
    /// shape as an auth failure on /recharge — one contract, one code path, no drift.
    ///
    /// DB-backed payloads (transaction / balance / operators) are built by
    /// <c>ApiUserResponseBuilder</c>, which delegates the envelope part back to this class.
    /// </summary>
    public static class ApiUserResponseFactory
    {
        /// <summary>
        /// Maps an error code to its numeric class. The prefix IS the classification, so a new
        /// AUTH_*/VAL_* code is categorised correctly without touching this method.
        /// </summary>
        public static int CodeFor(string status, string? errorCode)
        {
            string ec = errorCode ?? string.Empty;
            if (ec.Length > 0)
            {
                if (ec.StartsWith("AUTH_", StringComparison.Ordinal)) return ApiUserStatusCode.Unauthorized;
                // A bad signature / stale timestamp / replayed nonce is a credential failure, not a
                // typo: it must read as 401-class so an integrator's retry logic never treats it as
                // a recoverable validation slip.
                if (ec.StartsWith("SEC_", StringComparison.Ordinal)) return ApiUserStatusCode.Unauthorized;
                if (ec.StartsWith("VAL_", StringComparison.Ordinal)) return ApiUserStatusCode.InvalidRequest;
                if (ec == ApiUserErrorCode.SenderNotFound || ec == ApiUserErrorCode.BeneficiaryNotFound)
                    return ApiUserStatusCode.NotFound;
                // Remaining DMT_* codes are business declines of a step that never became a
                // transaction — unverified sender/beneficiary, a rejected flow step.
                if (ec.StartsWith("DMT_", StringComparison.Ordinal)) return ApiUserStatusCode.InvalidRequest;
                // PAYOUT_* means the platform cannot serve the request at all (no payout service
                // configured) — our problem, not a bad parameter, so it reads as a system fault
                // and the integrator's retry logic treats it as transient rather than re-sending
                // a "corrected" request that would fail identically.
                if (ec.StartsWith("PAYOUT_", StringComparison.Ordinal)) return ApiUserStatusCode.SystemError;
                if (ec == ApiUserErrorCode.TransactionNotFound) return ApiUserStatusCode.NotFound;
                if (ec == ApiUserErrorCode.SystemError) return ApiUserStatusCode.SystemError;
                if (ec == ApiUserErrorCode.TransactionRejected) return ApiUserStatusCode.Failed;
            }
            return status switch
            {
                ApiUserStatus.Success => ApiUserStatusCode.Success,
                ApiUserStatus.Pending => ApiUserStatusCode.Pending,
                ApiUserStatus.Failed => ApiUserStatusCode.Failed,
                _ => ApiUserStatusCode.SystemError,
            };
        }

        /// <summary>
        /// The outward-facing status for an error code. A rejected transaction is reported as
        /// FAILED (it will never complete, nothing was charged); everything else — bad
        /// credentials, bad parameters, unknown requestid, an unhandled exception — is ERROR,
        /// meaning "no transaction exists".
        /// </summary>
        public static string StatusFor(string errorCode) =>
            errorCode == ApiUserErrorCode.TransactionRejected ? ApiUserStatus.Failed : ApiUserStatus.Error;

        /// <summary>Stamps the shared envelope fields onto any API-user response. Returns it for chaining.</summary>
        public static T Apply<T>(T envelope, string status, string errorCode, string message, string requestId)
            where T : ApiUserEnvelope
        {
            envelope.Status = status;
            envelope.ErrorCode = errorCode;
            envelope.StatusCode = CodeFor(status, errorCode);
            envelope.Message = message;
            envelope.RequestId = requestId ?? string.Empty;
            return envelope;
        }

        /// <summary>Successful envelope for a non-transaction endpoint (balance, operators).</summary>
        public static T Ok<T>(T envelope, string message, string requestId) where T : ApiUserEnvelope =>
            Apply(envelope, ApiUserStatus.Success, ApiUserErrorCode.None, message, requestId);

        /// <summary>Failure envelope for a non-transaction endpoint, from a machine error code.</summary>
        public static T Fail<T>(T envelope, string errorCode, string message, string requestId)
            where T : ApiUserEnvelope, new() =>
            Apply(envelope, StatusFor(errorCode), errorCode, message, requestId);

        /// <summary>Failure envelope for a transaction endpoint (/recharge, /status).</summary>
        public static ApiUserTransactionResponse FailTransaction(
            string errorCode, string message, string requestId, string eventType = ApiUserEventType.Initial)
        {
            string status = StatusFor(errorCode);
            var response = Apply(new ApiUserTransactionResponse(), status, errorCode, message, requestId);
            response.Event = eventType;
            response.Final = ApiUserStatus.IsFinal(status);
            return response;
        }
    }
}
