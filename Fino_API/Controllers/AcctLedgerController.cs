using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Phase4;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The double-entry ledger: the chart's vouchers, the posting outbox, and the acts that write
    /// to the books.
    ///
    /// Reads — vouchers and the outbox — are open to the accounting desk. Enqueuing an event and
    /// processing the outbox are operational (Admin + Support): recording that something happened,
    /// and turning the queue into vouchers, are re-runnable and idempotent. Posting a manual journal
    /// and reversing a voucher CHANGE the books directly, so they are Admin only — the same line the
    /// GST side draws for issuing a document.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class AcctLedgerController : TaxControllerBase
    {
        private readonly ILedgerPostingService _ledger;

        public AcctLedgerController(ILedgerPostingService ledger, IConfiguration configuration)
            : base(configuration) => _ledger = ledger;

        [HttpPost("Vouchers")]
        public Task<APIResponse> Vouchers([FromForm] APIRequest apiRequest) =>
            Handle<LedgerQueryRequest>(apiRequest, async (request, _) => Ok(await _ledger.GetVouchers(request)));

        [HttpPost("GetById")]
        public Task<APIResponse> GetById([FromForm] APIRequest apiRequest) =>
            Handle<VoucherIdRequest>(apiRequest, async (request, _) =>
            {
                var v = await _ledger.GetVoucher(request.AcctVoucherID);
                return v == null ? Refuse($"Voucher {request.AcctVoucherID} not found.") : Ok(v);
            });

        [HttpPost("Outbox")]
        public Task<APIResponse> Outbox([FromForm] APIRequest apiRequest) =>
            Handle<LedgerQueryRequest>(apiRequest, async (request, _) => Ok(await _ledger.GetOutbox(request.Status)));

        /// <summary>Record a business event for posting. Idempotent on its event key.</summary>
        [HttpPost("Enqueue")]
        public Task<APIResponse> Enqueue([FromForm] APIRequest apiRequest) =>
            Handle<EnqueuePostingRequest>(apiRequest, async (request, _) =>
            {
                if (string.IsNullOrWhiteSpace(request.EventType)) return Refuse("An event type is required.");
                return Ok(await _ledger.Enqueue(request));
            });

        /// <summary>Turn the pending (and retryable failed) outbox into vouchers.</summary>
        [HttpPost("ProcessOutbox")]
        public Task<APIResponse> ProcessOutbox([FromForm] APIRequest apiRequest) =>
            Handle<ProcessOutboxRequest>(apiRequest, async (request, _) => Ok(await _ledger.ProcessOutbox(request.MaxItems)));

        /// <summary>Post a hand-written balanced journal. Admin only.</summary>
        [HttpPost("PostJournal")]
        public Task<APIResponse> PostJournal([FromForm] APIRequest apiRequest) =>
            Handle<ManualJournalRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (result, failure) = await _ledger.PostManualJournal(request);
                return result == null ? Refuse(failure) : Ok(result);
            });

        /// <summary>Reverse a posted voucher with an equal-and-opposite one. Admin only.</summary>
        [HttpPost("Reverse")]
        public Task<APIResponse> Reverse([FromForm] APIRequest apiRequest) =>
            Handle<ReverseVoucherRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                var (result, failure) = await _ledger.ReverseVoucher(request);
                return result == null ? Refuse(failure) : Ok(result);
            });
    }

    public class VoucherIdRequest
    {
        public int AcctVoucherID { get; set; }
    }
}
