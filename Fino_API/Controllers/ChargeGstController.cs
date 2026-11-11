using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.ChargeGst;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The GST on a charge, previewed before it is charged.
    ///
    /// Every band of a provider's rate card and of a retailer's slab now carries its own GST
    /// decision — inclusive, exclusive, or none — and falls through to the commission tax profile
    /// when it does not say. Four layers is the right amount of flexibility for contracts that
    /// genuinely differ per band, and far too much to hold in anybody's head, so this endpoint
    /// answers the only question that matters: for a transaction of THIS size by THIS user,
    /// what gets charged, what of it is tax, and which layer decided.
    ///
    /// It writes nothing, so the accounting desk reads it too. Editing the bands themselves stays
    /// with Admin on the two configuration controllers.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class ChargeGstController : TaxControllerBase
    {
        private readonly IChargeGstResolver _chargeGst;

        public ChargeGstController(IChargeGstResolver chargeGst, IConfiguration configuration)
            : base(configuration) => _chargeGst = chargeGst;

        /// <summary>
        /// Both sides of one hypothetical transaction: the provider leg and all three downline
        /// tiers, each split into base + GST, plus the steps in words.
        /// </summary>
        [HttpPost("Preview")]
        public Task<APIResponse> Preview([FromForm] APIRequest apiRequest) =>
            Handle<ChargeGstPreviewRequest>(apiRequest, async (request, keyData) =>
            {
                if (request.Amount <= 0m)
                    return Refuse("Enter the transaction amount to preview. The amount is what picks the band on each side.");
                if (request.ServiceID <= 0)
                    return Refuse("Pick a service. A rate card band is always scoped to one.");

                // Previewing somebody else's slab is an admin act, and this controller is granted
                // to the accounting desk only — but a 0 should still mean "me" rather than
                // silently previewing the house with no slab at all.
                int userID = request.UserID > 0 ? request.UserID : keyData.UserID;

                var preview = await _chargeGst.Preview(
                    userID, request.ProviderID, request.ServiceID, request.OperatorID,
                    request.Amount, (request.AsOf ?? DateTime.Now).Date);

                return Ok(preview);
            });
    }
}
