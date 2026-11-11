namespace FintechApp.StaticModels
{
    /// <summary>
    /// The global "Hold DMT" switch (Admin → DMT Settings). While it is ON, a money-transfer is
    /// still validated, debited and commissioned exactly as usual, but it is NOT dispatched to the
    /// upstream gateway — it is parked as PENDING with <see cref="Marker"/> in its description and
    /// ProviderID left at 0. Switching the hold OFF releases every parked transfer through the
    /// normal DMT provider sequence, so nothing is lost and nothing is charged twice.
    /// </summary>
    public static class TransactionHold
    {
        /// <summary>SettingMaster key carrying the switch (IsOn = held).</summary>
        public const string DMTHoldSettingKey = "DMTHold";

        /// <summary>Written into TransactionMaster.Description while a transfer is parked.</summary>
        public const string Marker = "[HELD]";

        /// <summary>Description written when a transfer is parked by the hold.</summary>
        public const string HeldDescription = Marker + " DMT on hold — awaiting release by admin";

        /// <summary>True when this transaction is parked by the hold (not yet dispatched).</summary>
        public static bool IsHeld(Entities.TransactionMaster txn) =>
            txn != null && txn.ProviderID == 0 && (txn.Description ?? string.Empty).Contains(Marker);
    }
}
