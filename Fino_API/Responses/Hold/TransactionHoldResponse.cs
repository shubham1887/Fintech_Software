namespace FintechApp.Responses.Hold
{
    /// <summary>State of the global money-transfer hold, plus what is currently parked by it.</summary>
    public class TransactionHoldResponse
    {
        /// <summary>true = transfers are being parked instead of sent upstream.</summary>
        public bool IsOn { get; set; }

        /// <summary>Last remark recorded when the switch was changed.</summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>When the switch was last changed.</summary>
        public DateTime? UpdatedOn { get; set; }

        /// <summary>How many transfers are parked right now.</summary>
        public int HeldCount { get; set; }

        /// <summary>Total value of the parked transfers.</summary>
        public decimal HeldAmount { get; set; }
    }
}
