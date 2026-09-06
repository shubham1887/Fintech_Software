using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TransactionProcessing;
using FintechApp.StaticModels;

namespace FintechApp.Services.Callbacks
{
    /// <summary>
    /// The catch-all parser: Payin and every other upstream that pushes a plain
    /// "here is what happened to your request id" notification, signed with nothing.
    ///
    /// Registered LAST — <see cref="CanHandle"/> claims everything, so a provider with its own
    /// parser must be registered ahead of it. This is what makes a new integration free: a
    /// provider that speaks any of the aliases below needs a row and a key, no code.
    ///
    /// It cannot prove authenticity, so its safety rests on two things the router applies around
    /// it: the callback key in the URL (unguessable, per provider) and the IP allowlist on the
    /// provider row. Set that IP for any provider that lands here.
    /// </summary>
    public class GenericProviderCallbackParser : IProviderCallbackParser
    {
        public bool CanHandle(ProviderMaster provider) => true;

        public ProviderCallbackParseResult Parse(ProviderMaster provider, IDictionary<string, string> values, string rawBody)
        {
            string requestId = CallbackValues.First(values,
                "RequestID", "request_id", "requestid", "client_req_id", "clientrequestid",
                "orderid", "order_id", "apirequestid", "agentrefno", "clientid");

            string status = CallbackValues.First(values,
                "Status", "status", "txnstatus", "transactionstatus", "txn_status", "trans_status");

            if (requestId.Length == 0 || status.Length == 0)
                return ProviderCallbackParseResult.Reject(
                    "Callback rejected: could not find a request id and a status in the push.");

            return ProviderCallbackParseResult.Accept(new RechargeCallbackRequest
            {
                RequestID = requestId,
                Status = NormaliseStatus(status),
                ReferenceNo = CallbackValues.First(values,
                    "ReferenceNo", "reference_no", "referenceno", "operator_id", "operatorid",
                    "opid", "oprid", "operatorref", "utr", "rrn", "trans_id", "txnid"),
                Number = CallbackValues.First(values,
                    "Number", "number", "mobile", "mobileno", "mobile_no", "consumernumber", "consumer_number", "account"),
                Amount = CallbackValues.First(values, "Amount", "amount", "amt"),
                Balance = CallbackValues.First(values, "Balance", "balance", "close_bal", "closingbalance", "closing_balance"),
                Description = CallbackValues.First(values,
                    "Description", "description", "message", "msg", "remark", "reason", "error_desc", "errordesc"),
                IsReversal = CallbackValues.First(values, "reversal_status", "reversal", "isreversal", "is_reversal")
                    is "1" or "true" or "TRUE" or "True" or "Y" or "y",
            }, "Accepted (unsigned provider push).");
        }

        /// <summary>
        /// Providers spell the same three outcomes a dozen ways. Anything not recognised is passed
        /// through untouched — the settlement layer treats an unknown status as "nothing definitive
        /// yet" and leaves the transaction pending, which is the safe direction to be wrong in.
        /// </summary>
        private static string NormaliseStatus(string status)
        {
            string value = status.Trim().ToUpperInvariant();

            // Words only. A bare code ("1", "2") means nothing without the provider's own code
            // table — that mapping is what ProviderResponseMaster / ResponseGroupID exist for, so
            // guessing it here would be settling a transaction on a coin flip.
            if (value is "SUCCESS" or "SUCCESSFUL" or "SUCESS" or "COMPLETED" or "COMPLETE"
                or "DELIVERED" or "ACCEPTED" or "S")
                return "SUCCESS";

            if (value.StartsWith("FAIL") || value is "FAILURE" or "REJECTED" or "REJECT" or "REFUND"
                or "REFUNDED" or "REVERSED" or "CANCELLED" or "CANCELED" or "F")
                return "FAILURE";

            if (value is "PENDING" or "PROCESS" or "PROCESSING" or "INPROGRESS" or "IN_PROGRESS"
                or "WAITING" or "HOLD" or "P")
                return "PROCESS";

            return value;
        }
    }
}
