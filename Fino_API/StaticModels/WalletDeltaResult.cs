namespace FintechApp.StaticModels
{
    /// <summary>
    /// Outcome of an atomic wallet-balance mutation (see IUserWalletMasterRepository.TryApplyDelta).
    /// The balance is changed by a single conditional UPDATE in the database, so the opening/closing
    /// values are exactly the pre- and post-image the DB row-lock saw — no read-modify-write race.
    /// </summary>
    public readonly struct WalletDeltaResult
    {
        /// <summary>True when the delta was applied. False means the row is missing OR the debit
        /// would have breached the spendable-balance guard (insufficient balance / hold).</summary>
        public bool Applied { get; init; }

        /// <summary>Balance before the delta (only meaningful when <see cref="Applied"/> is true).</summary>
        public decimal OpeningBalance { get; init; }

        /// <summary>Balance after the delta (only meaningful when <see cref="Applied"/> is true).</summary>
        public decimal ClosingBalance { get; init; }

        public static WalletDeltaResult NotApplied => new() { Applied = false };

        public static WalletDeltaResult Ok(decimal opening, decimal closing)
            => new() { Applied = true, OpeningBalance = opening, ClosingBalance = closing };
    }
}
