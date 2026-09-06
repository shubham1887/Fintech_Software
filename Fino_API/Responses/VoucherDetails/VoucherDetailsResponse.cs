namespace FintechApp.Responses.VoucherDetails
{
    public class VoucherDetailsResponse
    {
        public int VoucherDetailsID { get; set; }
        public int UserID { get; set; }
        public string? Mode { get; set; }
        public decimal OutstandingOpeningBal { get; set; }
        public decimal Amount { get; set; }
        public decimal OutstandingClossingBal { get; set; }
        public string? Remark { get; set; }
        public string? Credit { get; set; }
        public string? Debit { get; set; }
        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

    }
}

