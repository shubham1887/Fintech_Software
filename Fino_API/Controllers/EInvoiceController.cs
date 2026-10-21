using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.TaxEngine;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The e-invoice (IRN) queue.
    ///
    /// At this turnover e-invoicing is mandatory: a notified invoice without a valid IRN is not a
    /// valid tax invoice, and the buyer's credit on it is at risk. Above Rs 10 crore AATO a
    /// document must reach the IRP within 30 days of its date, after which no IRN can be obtained
    /// at all and the only route left is a credit note and a fresh invoice.
    ///
    /// DeadlineAlerts is therefore not a nice-to-have. It is the endpoint that catches a document
    /// before that happens, and it should be on a dashboard tile rather than waiting to be asked.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class EInvoiceController : TaxControllerBase
    {
        private readonly IEInvoiceService _einvoice;
        private readonly IEInvoiceGateway _gateway;

        public EInvoiceController(IEInvoiceService einvoice, IEInvoiceGateway gateway, IConfiguration configuration)
            : base(configuration)
        {
            _einvoice = einvoice;
            _gateway = gateway;
        }

        /// <summary>Whether an IRP is configured at all. The screen should say so plainly if not —
        /// documents queue either way, and their deadlines keep counting down.</summary>
        [HttpPost("Status")]
        public Task<APIResponse> Status([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, _ => Task.FromResult(Ok(new
            {
                GatewayConfigured = _gateway.IsConfigured,
                Message = _gateway.IsConfigured
                    ? "An Invoice Registration Portal is configured."
                    : "No IRP is configured. Documents are queued and their 30-day deadlines are running; "
                    + "none of them can obtain an IRN until a gateway is set up.",
            })));

        [HttpPost("Enqueue")]
        public Task<APIResponse> Enqueue([FromForm] APIRequest apiRequest) =>
            Handle<DocumentIdRequest>(apiRequest, async (request, _) =>
            {
                var (queued, message) = await _einvoice.Enqueue(request.TaxInvoiceHeaderID);
                return Ok(new { Queued = queued, Message = message });
            });

        /// <summary>Queue every issued sale document in a period that is not queued yet.</summary>
        [HttpPost("EnqueuePeriod")]
        public Task<APIResponse> EnqueuePeriod([FromForm] APIRequest apiRequest) =>
            Handle<PeriodRequest>(apiRequest, async (request, _) =>
            {
                if (request.PeriodFrom == default || request.PeriodTo == default)
                    return Refuse("A period is required.");
                return Ok(new { Queued = await _einvoice.EnqueuePeriod(request.PeriodFrom, request.PeriodTo) });
            });

        /// <summary>Attempt the pending registrations, soonest deadline first.</summary>
        [HttpPost("ProcessQueue")]
        public Task<APIResponse> ProcessQueue([FromForm] APIRequest apiRequest) =>
            Handle<PeriodRequest>(apiRequest, async (request, _) =>
                Ok(await _einvoice.ProcessQueue(request.MaxDocuments)));

        /// <summary>What is running out of time, or has already run out.</summary>
        [HttpPost("DeadlineAlerts")]
        public Task<APIResponse> DeadlineAlerts([FromForm] APIRequest apiRequest) =>
            Handle<PeriodRequest>(apiRequest, async (request, _) =>
                Ok(await _einvoice.GetDeadlineAlerts(DateTime.Today, request.DaysAhead)));
    }
}
