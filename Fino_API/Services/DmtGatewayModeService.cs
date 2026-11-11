using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IDmtGatewayModeService"/>
    public class DmtGatewayModeService : IDmtGatewayModeService
    {
        private readonly IDMTSettingRepository _dmtRepo;
        private readonly Logger _logger = new();

        public DmtGatewayModeService(IDMTSettingRepository dmtRepo) => _dmtRepo = dmtRepo;

        public async Task<bool> IsPayoutDmt()
        {
            try
            {
                // The lowest-Sequence active row is the gateway in force — the same rule
                // JaiJuiGatewayResolver uses to pick who the transfer is dispatched to. The two
                // must agree: if this said "payout, no OTP needed" while the dispatcher sent the
                // transfer to a pure rail, every transfer would be refused upstream for an
                // unregistered sender, after we had already told the integrator to skip
                // registration.
                var gateway = (await _dmtRepo.GetAll())
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Sequence)
                    .FirstOrDefault();

                return gateway?.IsPayoutDMT ?? false;
            }
            catch (Exception ex)
            {
                // A lookup failure must not switch the verification gates off. Report pure DMT —
                // the stricter of the two — and let the transfer be refused rather than let an
                // unverified one through because a database read hiccuped.
                _logger.Write(ex);
                return false;
            }
        }
    }
}
