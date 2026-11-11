namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Which wallet a service debits from, as the admin set it on the "Service Wallet Mapping"
    /// screen.
    ///
    /// The mapping existed and the screen saved it, but the transaction pipelines never read it —
    /// recharge and bill payment always debited the Recharge wallet and a transfer always debited
    /// the DMT wallet, whatever the mapping said. Mapping Electricity Bill Payment to the DMT
    /// wallet therefore changed nothing: the bill still came out of Recharge. One resolver, used
    /// by every pipeline, is what makes the screen mean something.
    /// </summary>
    public interface IServiceWalletResolver
    {
        /// <summary>
        /// The wallet this service should debit. Returns <paramref name="fallbackWalletID"/> when
        /// the admin has mapped nothing for the service, or when the mapped wallet has since been
        /// switched off — a transaction must never be routed into a disabled wallet.
        /// </summary>
        Task<int> Resolve(int serviceID, int fallbackWalletID);

        /// <summary>
        /// The wallet for a whole KIND of service (<see cref="StaticModels.ServiceKinds"/>) — for
        /// the pipelines that know what they are doing ("a payout", "a micro-ATM withdrawal") but
        /// not which row in this install's Service Catalog represents it. The service ids differ
        /// per install, so asking by kind is the only way these can read the mapping at all: the
        /// MicroATM pipeline, for one, had service id 5 compiled in, which is DTH Recharge here.
        /// Falls back to <paramref name="fallbackWalletID"/> when no such service is configured.
        /// </summary>
        Task<int> ResolveForKind(string serviceKind, int fallbackWalletID);
    }
}
