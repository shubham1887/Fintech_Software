using Newtonsoft.Json;

namespace FintechApp.Responses.ApiUser
{
    /// <summary>
    /// A step that succeeded but has no payload of its own — OTP verified, OTP resent,
    /// beneficiary deleted. <see cref="OtpRequired"/> is what an integrator branches on after a
    /// registration call: it says whether the flow still owes us a verification step.
    /// </summary>
    public class ApiUserDmtAckResponse : ApiUserEnvelope
    {
        /// <summary>True when the platform has sent an OTP and the next call must verify it.</summary>
        [JsonProperty("otprequired", Order = 6)]
        public bool OtpRequired { get; set; }

        /// <summary>
        /// The reference the follow-up call needs — a beneficiary id after a beneficiary
        /// registration. Empty where the step needs no reference.
        /// </summary>
        [JsonProperty("reference", Order = 7)]
        public string Reference { get; set; } = string.Empty;
    }

    /// <summary>Remitter profile and the headroom left on their regulatory limit.</summary>
    public class ApiUserDmtSenderResponse : ApiUserEnvelope
    {
        [JsonProperty("sendermobile", Order = 6)]
        public string SenderMobile { get; set; } = string.Empty;

        [JsonProperty("sendername", Order = 7)]
        public string SenderName { get; set; } = string.Empty;

        [JsonProperty("address", Order = 8)]
        public string Address { get; set; } = string.Empty;

        /// <summary>True once the remitter has cleared OTP verification and may transact.</summary>
        [JsonProperty("verified", Order = 9)]
        public bool Verified { get; set; }

        /// <summary>Amount already sent inside the current limit period.</summary>
        [JsonProperty("consumedlimit", Order = 10)]
        public decimal ConsumedLimit { get; set; }

        /// <summary>Amount the remitter may still send inside the current limit period.</summary>
        [JsonProperty("remaininglimit", Order = 11)]
        public decimal RemainingLimit { get; set; }

        /// <summary>Per-transfer ceiling in force for this remitter. 0 ⇒ no additional cap.</summary>
        [JsonProperty("pertransferlimit", Order = 12)]
        public decimal PerTransferLimit { get; set; }
    }

    /// <summary>One beneficiary row.</summary>
    public class ApiUserDmtBeneficiary
    {
        /// <summary>The reference to quote on /dmt/transfer. Opaque — never parse it.</summary>
        [JsonProperty("beneficiaryid", Order = 1)]
        public string BeneficiaryId { get; set; } = string.Empty;

        [JsonProperty("beneficiaryname", Order = 2)]
        public string BeneficiaryName { get; set; } = string.Empty;

        [JsonProperty("accountno", Order = 3)]
        public string AccountNo { get; set; } = string.Empty;

        [JsonProperty("ifsc", Order = 4)]
        public string Ifsc { get; set; } = string.Empty;

        [JsonProperty("bankname", Order = 5)]
        public string BankName { get; set; } = string.Empty;

        /// <summary>True once OTP-verified. A beneficiary that is not verified cannot receive a transfer.</summary>
        [JsonProperty("verified", Order = 6)]
        public bool Verified { get; set; }

        /// <summary>True when the account holder's name was confirmed by a penny-drop enquiry.</summary>
        [JsonProperty("nameverified", Order = 7)]
        public bool NameVerified { get; set; }
    }

    /// <summary>Result of registering one beneficiary.</summary>
    public class ApiUserDmtBeneficiaryResponse : ApiUserEnvelope
    {
        /// <summary>True when an OTP was sent and /dmt/beneficiary/otp/verify must follow.</summary>
        [JsonProperty("otprequired", Order = 6)]
        public bool OtpRequired { get; set; }

        [JsonProperty("beneficiary", Order = 7)]
        public ApiUserDmtBeneficiary? Beneficiary { get; set; }
    }

    /// <summary>Every beneficiary registered under one remitter.</summary>
    public class ApiUserDmtBeneficiaryListResponse : ApiUserEnvelope
    {
        [JsonProperty("count", Order = 6)]
        public int Count { get; set; }

        [JsonProperty("beneficiaries", Order = 7)]
        public List<ApiUserDmtBeneficiary> Beneficiaries { get; set; } = new();
    }

    /// <summary>Outcome of a penny-drop account verification.</summary>
    public class ApiUserDmtVerifyAccountResponse : ApiUserEnvelope
    {
        /// <summary>The account-holder name the bank returned. Empty when the enquiry failed.</summary>
        [JsonProperty("verifiedname", Order = 6)]
        public string VerifiedName { get; set; } = string.Empty;

        [JsonProperty("bankname", Order = 7)]
        public string BankName { get; set; } = string.Empty;

        /// <summary>Fee debited for this enquiry. Charged on failure too where the bank was reached.</summary>
        [JsonProperty("charge", Order = 8)]
        public decimal Charge { get; set; }

        /// <summary>DMT wallet balance after the fee.</summary>
        [JsonProperty("balance", Order = 9)]
        public decimal Balance { get; set; }
    }

    /// <summary>One bank in the DMT catalogue.</summary>
    public class ApiUserDmtBank
    {
        [JsonProperty("bankid", Order = 1)]
        public int BankId { get; set; }

        [JsonProperty("bankname", Order = 2)]
        public string BankName { get; set; } = string.Empty;

        /// <summary>A representative IFSC for the bank, useful for pre-filling a branch lookup.</summary>
        [JsonProperty("defaultifsc", Order = 3)]
        public string DefaultIfsc { get; set; } = string.Empty;

        /// <summary>True when the bank accepts IMPS. A false here means NEFT only.</summary>
        [JsonProperty("imps", Order = 4)]
        public bool Imps { get; set; }
    }

    /// <summary>The DMT bank catalogue.</summary>
    public class ApiUserDmtBankListResponse : ApiUserEnvelope
    {
        [JsonProperty("count", Order = 6)]
        public int Count { get; set; }

        [JsonProperty("banks", Order = 7)]
        public List<ApiUserDmtBank> Banks { get; set; } = new();
    }

    /// <summary>Branch details behind one IFSC.</summary>
    public class ApiUserDmtIfscResponse : ApiUserEnvelope
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
    /// THE DMT transaction payload. Returned verbatim by:
    ///   • POST /api/v1/dmt/transfer  (event = INITIAL)
    ///   • POST /api/v1/dmt/status    (event = STATUS)
    ///   • the callback we POST to the integrator's URL (event = CALLBACK)
    ///
    /// It extends the recharge transaction contract rather than replacing it, so an integrator
    /// who already consumes /api/v1/recharge parses this with the same code and only reads the
    /// extra beneficiary block below. Every inherited field keeps its exact meaning —
    /// <c>number</c> is the destination account, <c>amount</c> the principal,
    /// <c>debitamount</c> the principal plus charges, <c>balance</c> the DMT wallet afterwards.
    /// </summary>
    public class ApiUserDmtTransactionResponse : ApiUserTransactionResponse
    {
        /// <summary>The remitter's mobile.</summary>
        [JsonProperty("sendermobile", Order = 20)]
        public string SenderMobile { get; set; } = string.Empty;

        /// <summary>The beneficiary reference the transfer was sent to.</summary>
        [JsonProperty("beneficiaryid", Order = 21)]
        public string BeneficiaryId { get; set; } = string.Empty;

        [JsonProperty("beneficiaryname", Order = 22)]
        public string BeneficiaryName { get; set; } = string.Empty;

        /// <summary>Destination account number. Same value as the inherited <c>number</c>.</summary>
        [JsonProperty("accountno", Order = 23)]
        public string AccountNo { get; set; } = string.Empty;

        [JsonProperty("ifsc", Order = 24)]
        public string Ifsc { get; set; } = string.Empty;

        [JsonProperty("bankname", Order = 25)]
        public string BankName { get; set; } = string.Empty;

        /// <summary>IMPS or NEFT, as actually dispatched.</summary>
        [JsonProperty("paymenttype", Order = 26)]
        public string PaymentType { get; set; } = string.Empty;

        /// <summary>
        /// The bank UTR / RRN. This is the reference the beneficiary's bank recognises, so it is
        /// what a customer dispute is settled with. Populated once the transfer reaches SUCCESS;
        /// empty while PENDING.
        /// </summary>
        [JsonProperty("utr", Order = 27)]
        public string Utr { get; set; } = string.Empty;

        /// <summary>Total charge levied on this transfer (already included in <c>debitamount</c>).</summary>
        [JsonProperty("charge", Order = 28)]
        public decimal Charge { get; set; }
    }
}
