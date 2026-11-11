using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ApiUser;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The public B2B PAYOUT API for API-user integrations (contract v1).
    ///
    /// Sibling of <see cref="ApiDmtController"/>: same credentials, same HMAC request signing,
    /// same envelope, POST-only except the balance read. The difference is the one that matters
    /// to an integrator:
    ///
    ///   A PAYOUT IS ONE CALL. /payout/transfer carries the destination account, IFSC and holder
    ///   name inline. There is no sender to register, no beneficiary to register, no OTP, and so
    ///   NO <c>beneficiaryid</c> — that parameter belongs to /api/v1/dmt, where the regulator
    ///   requires a verified remitter because the money is a walk-in customer's cash. A payout
    ///   moves the API user's OWN wallet balance to an account they name, so there is nothing to
    ///   pre-register and nothing to look up in a database first.
    ///
    /// Every endpoint replies HTTP 200 and carries the real outcome in the body's
    /// <c>status</c> / <c>statuscode</c> / <c>errorcode</c> fields — integrators on simple HTTP
    /// stacks must never have to distinguish a transport failure from a business decline.
    /// </summary>
    [ApiController]
    [Route("api/v1/payout")]
    public class ApiPayoutController : ApiUserSignedControllerBase
    {
        private readonly IApiUserPayoutGatewayService _gateway;

        public ApiPayoutController(IApiUserPayoutGatewayService gateway) => _gateway = gateway;

        // ─────────────────────────────────────────────────────────────────────
        //  Money movement
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// POST /api/v1/payout/transfer — send money to a bank account in a single call.
        /// <c>requestid</c> is mandatory and is the idempotency key.
        /// </summary>
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer()
        {
            await Load();
            var request = Bind(new ApiUserPayoutTransferRequest());
            request.AccountNo = P("accountno");
            request.Ifsc = P("ifsc");
            request.BeneficiaryName = P("beneficiaryname");
            request.BankName = P("bankname");
            request.Amount = P("amount");
            request.PaymentType = P("paymenttype");
            request.Remark = P("remark");
            request.Mobile = P("mobile");
            request.Latitude = P("latitude");
            request.Longitude = P("longitude");
            return Ok(await _gateway.Transfer(request));
        }

        /// <summary>
        /// POST /api/v1/payout/status — current state of a payout, by your requestid or our txnid.
        /// Returns exactly the same payload shape as /transfer and as the callback.
        /// </summary>
        [HttpPost("status")]
        public async Task<IActionResult> Status()
        {
            await Load();
            var request = Bind(new ApiUserPayoutStatusRequest());
            request.TxnId = P("txnid");
            return Ok(await _gateway.Status(request));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Reference data
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/v1/payout/balance — the API user's wallet balances.
        /// The only GET in the payout contract: it exposes no customer data and changes nothing.
        /// </summary>
        [HttpGet("balance")]
        public async Task<IActionResult> Balance()
        {
            await Load();
            return Ok(await _gateway.Balance(Bind(new ApiUserPayoutQueryRequest())));
        }

        /// <summary>POST /api/v1/payout/ifsc — branch details behind an IFSC.</summary>
        [HttpPost("ifsc")]
        public async Task<IActionResult> Ifsc()
        {
            await Load();
            var request = Bind(new ApiUserPayoutIfscRequest());
            request.Ifsc = P("ifsc");
            return Ok(await _gateway.Ifsc(request));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET guards
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Catches a GET aimed at a payout endpoint other than /balance and answers with the
        /// contract's own error shape, so an integrator who built against a GET-based API
        /// elsewhere gets told exactly what to change instead of a bare framework 405. It is a
        /// refusal, never a fallback: the request is not executed.
        /// </summary>
        [HttpGet("transfer")]
        [HttpGet("status")]
        [HttpGet("ifsc")]
        public IActionResult MethodNotAllowed() => Ok(ApiUserResponseFactory.Fail(
            new Responses.ApiUser.ApiUserPayoutAckResponse(),
            ApiUserErrorCode.MethodNotAllowed,
            $"{Request.Path} accepts POST only. Only /api/v1/payout/balance may be called with GET. " +
            "Money-moving endpoints must not put parameters in a URL.",
            QueryRequestId()));
    }
}
