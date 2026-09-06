namespace Fino.Pay1Api.Requests
{
    // Live bill enquiry before payment (operation_type 8).
    public class BillFetchRequest
    {
        /// <summary>Pay1 product id of the biller.</summary>
        public string Operator { get; set; } = string.Empty;

        /// <summary>Consumer / account number at the biller.</summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>Extra biller inputs, when the biller demands them (billing unit, cycle…).</summary>
        public string Param { get; set; } = string.Empty;
        public string Param1 { get; set; } = string.Empty;

        /// <summary>Unique id for this enquiry. Generated when left blank.</summary>
        public string RequestID { get; set; } = string.Empty;
    }

    // Bill payment (operation_type 9).
    public class BillPaymentRequest
    {
        public string Operator { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Param { get; set; } = string.Empty;
        public string Param1 { get; set; } = string.Empty;

        /// <summary>Our unique transaction reference. Pay1 rejects a repeat, which is what makes
        /// the payment idempotent — never reuse one for a retry.</summary>
        public string RequestID { get; set; } = string.Empty;
    }

    // Transaction enquiry (operation_type 3).
    public class StatusCheckRequest
    {
        /// <summary>Our request ids. Pay1 accepts up to 10, comma separated.</summary>
        public string RequestIDs { get; set; } = string.Empty;
    }

    // Raise a complaint on a transaction (operation_type 4).
    public class ComplaintRequest
    {
        /// <summary>Pay1's transaction id for the disputed payment.</summary>
        public string TransactionID { get; set; } = string.Empty;

        /// <summary>Complaint tag id from Pay1's tag list (928…933).</summary>
        public string TagID { get; set; } = Pay1ComplaintTag.SuccessfulNotUpdated;
    }

    // Pay1's complaint tag ids, as published in the Utility API document.
    public static class Pay1ComplaintTag
    {
        /// <summary>Transaction successful, account not updated.</summary>
        public const string SuccessfulNotUpdated = "928";

        /// <summary>Amount deducted, biller account credited but transaction id not received.</summary>
        public const string CreditedNoTxnID = "929";

        /// <summary>Amount deducted, biller account not credited, transaction id not received.</summary>
        public const string NotCreditedNoTxnID = "930";

        /// <summary>Amount deducted multiple times.</summary>
        public const string DeductedMultipleTimes = "931";

        /// <summary>Double payment updated.</summary>
        public const string DoublePayment = "932";

        /// <summary>Erroneously paid in wrong account.</summary>
        public const string WrongAccount = "933";
    }
}
