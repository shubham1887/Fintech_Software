namespace FintechApp.Responses.Operator
{
    public class OperatorResponse
    {
        public int OperatorID { get; set; }
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public decimal MinimumRechargeAmount { get; set; }
        public decimal MaximumRechargeAmount { get; set; }
        public int RepeatRechargeInMinute { get; set; }
        public bool? IsTimelyBlocked { get; set; }
        public DateTime BlockedTill { get; set; }
        public bool? IsRandom { get; set; }
        public string OperatorImagePath { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Opcode { get; set; } = string.Empty;
        public string BusinessModel { get; set; } = string.Empty;
        public int NumberLength { get; set; }
        public string HSNCode { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public string AccountStartWith { get; set; } = string.Empty;
        public string SequenceLogic { get; set; } = string.Empty;
        public bool? IsBBPS { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public string TollFreeNumber { get; set; } = string.Empty;
        public bool IsBillFetch { get; set; }
        public bool IsPartialBill { get; set; }
        public decimal UISequence { get; set; }
        public string BlockedAmount { get; set; } = string.Empty;
        public string AllowedFor { get; set; } = string.Empty;
        public bool IsViewRequired { get; set; }
        public bool IsDenominationApplicable { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
