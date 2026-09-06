using Newtonsoft.Json;

namespace FintechApp.Responses.ApiUser
{
    /// <summary>One wallet row inside a balance enquiry.</summary>
    public class ApiUserWalletBalance
    {
        /// <summary>Numeric wallet id — stable across renames, use this as the key.</summary>
        [JsonProperty("walletid", Order = 1)]
        public int WalletId { get; set; }

        /// <summary>Wallet display name, e.g. "Recharge Wallet".</summary>
        [JsonProperty("wallet", Order = 2)]
        public string Wallet { get; set; } = string.Empty;

        /// <summary>Ledger balance, including any held portion.</summary>
        [JsonProperty("balance", Order = 3)]
        public decimal Balance { get; set; }

        /// <summary>Frozen portion that cannot be spent.</summary>
        [JsonProperty("hold", Order = 4)]
        public decimal Hold { get; set; }

        /// <summary>Spendable balance = balance - hold. This is what transactions draw on.</summary>
        [JsonProperty("available", Order = 5)]
        public decimal Available { get; set; }
    }

    /// <summary>
    /// Balance enquiry payload — the shared envelope plus the wallet breakdown.
    /// <see cref="Balance"/> is the sum of every wallet's available balance, so a caller that
    /// only needs "how much can I spend" never has to walk the array.
    /// </summary>
    public class ApiUserBalanceResponse : ApiUserEnvelope
    {
        /// <summary>Total spendable balance across all wallets.</summary>
        [JsonProperty("balance", Order = 6)]
        public decimal Balance { get; set; }

        /// <summary>Per-wallet breakdown.</summary>
        [JsonProperty("wallets", Order = 7)]
        public List<ApiUserWalletBalance> Wallets { get; set; } = new();
    }
}
