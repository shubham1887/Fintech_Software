namespace FintechApp.Interfaces.Services
{
    // Resolves which ServiceMaster rows represent AEPS / MicroATM, by name and category, instead
    // of hardcoded ServiceIDs. Service ids are data (created from the admin panel), so fixed
    // numbers silently break money flows when a deployment seeds services in a different order —
    // e.g. a failed recharge whose ServiceID collides with the assumed AEPS id would never be
    // refunded. Mirrors how UserTypeCode centralised the role ids.
    public interface IServiceKindResolver
    {
        // AEPS / MicroATM are cash-in services: a failed transaction must NOT credit the wallet.
        Task<bool> IsCashInService(int serviceID);

        // DMT / money-transfer services (their refunds run through the DMT flow, not recon).
        Task<bool> IsDMTService(int serviceID);

        // Recharge-family services (mobile/DTH/prepaid/postpaid). These use the admin-funded
        // commission accounting (admin debited for each downline commission, surcharge credited to
        // admin); other services keep the legacy net-commission posting.
        Task<bool> IsRechargeService(int serviceID);

        // Services whose commissions settle through the balanced chain double-entry: every downline
        // commission is credited to the downline AND debited from the admin that funds it. Recharge
        // and DMT. Asked by the money engine when it posts those legs and by LedgerIntegrityService
        // when it checks that none is missing — two places that must agree on the same set, or the
        // integrity check reports a break on a service that was never meant to have the leg.
        Task<bool> IsChainFundedService(int serviceID);

        Task<HashSet<int>> GetAEPSServiceIDs();

        Task<HashSet<int>> GetMicroATMServiceIDs();
    }
}
