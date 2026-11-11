using Newtonsoft.Json;

namespace FintechApp.Responses.ApiUser
{
    /// <summary>
    /// A payout step that succeeded but carries no payload of its own. Exists so a refusal on a
    /// payout route (a GET where POST is required) comes back in the contract's own shape rather
    /// than as a bare framework 405 with an empty body.
    /// </summary>
    public class ApiUserPayoutAckResponse : ApiUserEnvelope
    {
    }

    /// <summary>Branch details behind one IFSC.</summary>
    public class ApiUserPayoutIfscResponse : ApiUserEnvelope
    {
        [JsonProperty("ifsc", Order = 6)]
        public string Ifsc { get; set; } = string.Empty;

        [JsonProperty("bankname", Order = 7)]
        public string BankName { get; set; } = string.Empty;

        [JsonProperty("branch", Order = 8)]
        public string Branch { get; set; } = string.Empty;

        [JsonProperty("address", Order = 9)]
        public string Address { get; set; } = string.Empty;
    }

    /// <summary>
    /// THE payout transaction payload. Returned verbatim by:
    ///   • POST /api/v1/payout/transfer  (event = INITIAL)
    ///   • POST /api/v1/payout/status    (event = STATUS)
    ///   • the callback we POST to the integrator's URL (event = CALLBACK)
    ///
    /// It extends the recharge transaction contract rather than replacing it, so an integrator
    /// who already consumes /api/v1/recharge or /api/v1/dmt parses this with the same code.
    /// Every inherited field keeps its exact meaning — <c>number</c> is the destination account,
    /// <c>amount</c> the principal, <c>debitamount</c> the principal plus charges,
    /// <c>balance</c> the wallet afterwards.
    ///
    /// Note what is NOT here: no <c>sendermobile</c> and no <c>beneficiaryid</c>. A payout has no
    /// remitter and nothing pre-registered — the destination account travelled on the request and
    /// is echoed back below.
    /// </summary>
    public class ApiUserPayoutTransactionResponse : ApiUserTransactionResponse
    {
        /// <summary>Destination account number. Same value as the inherited <c>number</c>.</summary>
        [JsonProperty("accountno", Order = 20)]
        public string AccountNo { get; set; } = string.Empty;

        [JsonProperty("ifsc", Order = 21)]
        public string Ifsc { get; set; } = string.Empty;

        [JsonProperty("beneficiaryname", Order = 22)]
        public string BeneficiaryName { get; set; } = string.Empty;

        [JsonProperty("bankname", Order = 23)]
        public string BankName { get; set; } = string.Empty;

        /// <summary>IMPS, NEFT or RTGS, as actually dispatched.</summary>
        [JsonProperty("paymenttype", Order = 24)]
        public string PaymentType { get; set; } = string.Empty;

        /// <summary>
        /// The bank UTR / RRN. This is the reference the beneficiary's bank recognises, so it is
        /// what a dispute is settled with. Populated once the payout reaches SUCCESS; empty while
        /// PENDING.
        /// </summary>
        [JsonProperty("utr", Order = 25)]
        public string Utr { get; set; } = string.Empty;

        /// <summary>Total charge levied on this payout (already included in <c>debitamount</c>).</summary>
        [JsonProperty("charge", Order = 26)]
        public decimal Charge { get; set; }
    }
}
