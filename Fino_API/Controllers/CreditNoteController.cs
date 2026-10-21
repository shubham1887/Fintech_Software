using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.TaxEngine;
using FintechApp.Requests.TaxInvoice;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Credit notes — the only correct way to undo an issued document.
    ///
    /// There is no endpoint here to edit or delete an invoice, and that is deliberate. Its number
    /// has gone into GSTR-1; changing what it stands for makes the return and the books disagree
    /// from then on, and deleting it leaves a hole in a sequence GST requires to be consecutive.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class CreditNoteController : TaxControllerBase
    {
        private readonly ICreditNoteService _notes;

        public CreditNoteController(ICreditNoteService notes, IConfiguration configuration)
            : base(configuration) => _notes = notes;

        /// <summary>
        /// Reverse an issued document, in full or in part.
        ///
        /// A warning comes back rather than a refusal when the 30-November window has closed: the
        /// note is still issued, but without tax, because the GST on that supply can no longer be
        /// recovered. The caller must show that warning — it is the difference between a reversal
        /// that costs nothing and one that costs the whole tax.
        /// </summary>
        [HttpPost("Issue")]
        public Task<APIResponse> Issue([FromForm] APIRequest apiRequest) =>
            Handle<CreditNoteRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (doc, message) = await _notes.Issue(request);
                return doc == null ? Refuse(message) : Ok(new { Document = doc, Warning = message });
            });

        /// <summary>
        /// Documents whose credit-note window is closing that still cover a failed or refunded
        /// transaction — reversals somebody has to decide about while it is still free to.
        /// </summary>
        [HttpPost("WindowAlerts")]
        public Task<APIResponse> WindowAlerts([FromForm] APIRequest apiRequest) =>
            Handle<PeriodRequest>(apiRequest, async (request, _) =>
                Ok(await _notes.GetWindowAlerts(DateTime.Today, request.DaysAhead)));
    }
}
