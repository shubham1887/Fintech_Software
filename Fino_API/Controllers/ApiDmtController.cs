using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ApiUser;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The public B2B DMT API for API-user integrations (contract v1).
    ///
    /// Sibling of <see cref="ApiRechargeController"/> and deliberately shaped the same way: no AES
    /// envelope and no JWT — the caller passes their userid and API token, and receives plain
    /// JSON. This class binds parameters and returns JSON; authentication, signature verification,
    /// validation, idempotency, execution, audit logging and callbacks all live in
    /// <see cref="IApiUserDmtGatewayService"/>, so every endpoint behaves identically.
    ///
    /// TWO RULES SET THIS CONTROLLER APART FROM THE RECHARGE ONE, both because DMT tickets are
    /// large enough that a leaked or replayed request is a material loss:
    ///
    ///   1. POST ONLY. Every endpoint that reads customer data, changes state or moves money is
    ///      POST. Query strings are logged by proxies, load balancers, browser history and server
    ///      access logs as a matter of course, so a GET would scatter account numbers and tokens
    ///      across infrastructure nobody audits. The single exception is
    ///      <see cref="Balance"/> — it carries no customer data, changes nothing, and integrators
    ///      poll it from simple stacks. A GET to any other endpoint is refused with
    ///      SEC_METHOD_NOT_ALLOWED rather than quietly honoured.
    ///
    ///   2. EVERY REQUEST IS SIGNED. The body is HMAC-SHA256'd with the caller's API token and
    ///      bound to a timestamp and a one-shot nonce — see <see cref="ApiUserSignature"/>. The
    ///      controller's job is only to lift that material off the wire faithfully, including the
    ///      raw body BYTES, because the signature is computed over exactly what was sent.
    ///
    /// Every endpoint replies HTTP 200 and carries the real outcome in the body's
    /// <c>status</c> / <c>statuscode</c> / <c>errorcode</c> fields — integrators on simple HTTP
    /// stacks must never have to distinguish a transport failure from a business decline.
    /// </summary>
    [ApiController]
    [Route("api/v1/dmt")]
    public class ApiDmtController : ApiUserSignedControllerBase
    {
        private readonly IApiUserDmtGatewayService _gateway;

        public ApiDmtController(IApiUserDmtGatewayService gateway) => _gateway = gateway;

        // ─────────────────────────────────────────────────────────────────────
        //  Reference data
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/v1/dmt/balance — the API user's wallet balances.
        /// The only GET in the DMT contract: it exposes no customer data and changes nothing.
        /// </summary>
        [HttpGet("balance")]
        public async Task<IActionResult> Balance()
        {
            await Load();
            return Ok(await _gateway.Balance(Bind(new ApiUserDmtQueryRequest())));
        }

        /// <summary>POST /api/v1/dmt/banks — the DMT bank catalogue.</summary>
        [HttpPost("banks")]
        public async Task<IActionResult> Banks()
        {
            await Load();
            return Ok(await _gateway.Banks(Bind(new ApiUserDmtQueryRequest())));
        }

        /// <summary>POST /api/v1/dmt/ifsc — branch details behind an IFSC.</summary>
        [HttpPost("ifsc")]
        public async Task<IActionResult> Ifsc()
        {
            await Load();
            var request = Bind(new ApiUserDmtIfscRequest());
            request.Ifsc = P("ifsc");
            return Ok(await _gateway.Ifsc(request));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Remitter onboarding
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>POST /api/v1/dmt/sender/register — onboard a remitter.</summary>
        [HttpPost("sender/register")]
        public async Task<IActionResult> RegisterSender()
        {
            await Load();
            var request = Bind(new ApiUserDmtSenderRegisterRequest());
            request.SenderMobile = P("sendermobile");
            request.SenderName = P("sendername");
            request.Address = P("address");
            request.Pincode = P("pincode");
            return Ok(await _gateway.RegisterSender(request));
        }

        /// <summary>POST /api/v1/dmt/sender/otp/verify — activate a remitter.</summary>
        [HttpPost("sender/otp/verify")]
        public async Task<IActionResult> VerifySenderOtp()
        {
            await Load();
            return Ok(await _gateway.VerifySenderOtp(BindSenderOtp()));
        }

        /// <summary>POST /api/v1/dmt/sender/otp/resend — send the remitter a fresh OTP.</summary>
        [HttpPost("sender/otp/resend")]
        public async Task<IActionResult> ResendSenderOtp()
        {
            await Load();
            return Ok(await _gateway.ResendSenderOtp(BindSenderOtp()));
        }

        /// <summary>POST /api/v1/dmt/sender/fetch — remitter profile and remaining limit.</summary>
        [HttpPost("sender/fetch")]
        public async Task<IActionResult> FetchSender()
        {
            await Load();
            var request = Bind(new ApiUserDmtSenderFetchRequest());
            request.SenderMobile = P("sendermobile");
            return Ok(await _gateway.FetchSender(request));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Beneficiary management
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>POST /api/v1/dmt/beneficiary/register — attach a bank account to a remitter.</summary>
        [HttpPost("beneficiary/register")]
        public async Task<IActionResult> RegisterBeneficiary()
        {
            await Load();
            var request = Bind(new ApiUserDmtBeneficiaryRegisterRequest());
            request.SenderMobile = P("sendermobile");
            request.BeneficiaryName = P("beneficiaryname");
            request.AccountNo = P("accountno");
            request.Ifsc = P("ifsc");
            request.BankName = P("bankname");
            return Ok(await _gateway.RegisterBeneficiary(request));
        }

        /// <summary>POST /api/v1/dmt/beneficiary/otp/verify — activate a beneficiary.</summary>
        [HttpPost("beneficiary/otp/verify")]
        public async Task<IActionResult> VerifyBeneficiaryOtp()
        {
            await Load();
            return Ok(await _gateway.VerifyBeneficiaryOtp(BindBeneficiaryOtp()));
        }

        /// <summary>POST /api/v1/dmt/beneficiary/otp/resend — send a fresh beneficiary OTP.</summary>
        [HttpPost("beneficiary/otp/resend")]
        public async Task<IActionResult> ResendBeneficiaryOtp()
        {
            await Load();
            return Ok(await _gateway.ResendBeneficiaryOtp(BindBeneficiaryOtp()));
        }

        /// <summary>POST /api/v1/dmt/beneficiary/list — every beneficiary under one remitter.</summary>
        [HttpPost("beneficiary/list")]
        public async Task<IActionResult> ListBeneficiaries()
        {
            await Load();
            var request = Bind(new ApiUserDmtBeneficiaryListRequest());
            request.SenderMobile = P("sendermobile");
            return Ok(await _gateway.ListBeneficiaries(request));
        }

        /// <summary>POST /api/v1/dmt/beneficiary/delete — detach a beneficiary.</summary>
        [HttpPost("beneficiary/delete")]
        public async Task<IActionResult> DeleteBeneficiary()
        {
            await Load();
            var request = Bind(new ApiUserDmtBeneficiaryRefRequest());
            request.BeneficiaryId = P("beneficiaryid");
            return Ok(await _gateway.DeleteBeneficiary(request));
        }

        /// <summary>POST /api/v1/dmt/beneficiary/verify — chargeable penny-drop name enquiry.</summary>
        [HttpPost("beneficiary/verify")]
        public async Task<IActionResult> VerifyBeneficiaryAccount()
        {
            await Load();
            var request = Bind(new ApiUserDmtBeneficiaryVerifyRequest());
            request.SenderMobile = P("sendermobile");
            request.AccountNo = P("accountno");
            request.Ifsc = P("ifsc");
            request.BankName = P("bankname");
            return Ok(await _gateway.VerifyBeneficiaryAccount(request));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Money movement
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// POST /api/v1/dmt/transfer — send money.
        /// <c>requestid</c> is mandatory and is the idempotency key.
        /// </summary>
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer()
        {
            await Load();
            var request = Bind(new ApiUserDmtTransferRequest());
            request.SenderMobile = P("sendermobile");
            request.BeneficiaryId = P("beneficiaryid");
            // Inline destination — accepted only when the platform's DMT gateway is a payout
            // rail, where there is nothing to pre-register. The gateway service decides; the
            // controller only carries the fields.
            request.AccountNo = P("accountno");
            request.Ifsc = P("ifsc");
            request.BeneficiaryName = P("beneficiaryname");
            request.BankName = P("bankname");
            request.Amount = P("amount");
            request.PaymentType = P("paymenttype");
            request.Remark = P("remark");
            request.Latitude = P("latitude");
            request.Longitude = P("longitude");
            return Ok(await _gateway.Transfer(request));
        }

        /// <summary>
        /// POST /api/v1/dmt/status — current state of a transfer, by your requestid or our txnid.
        /// Returns exactly the same payload shape as /transfer and as the callback.
        /// </summary>
        [HttpPost("status")]
        public async Task<IActionResult> Status()
        {
            await Load();
            var request = Bind(new ApiUserDmtStatusRequest());
            request.TxnId = P("txnid");
            return Ok(await _gateway.Status(request));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET guards
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Catches a GET aimed at any DMT endpoint other than /balance and answers with the
        /// contract's own error shape.
        ///
        /// Without this the framework would reply 405 with an empty body, and an integrator who
        /// built against a GET-based API elsewhere would see a transport-looking failure with no
        /// explanation. Refusing it in-contract tells them exactly what to change — and it is a
        /// refusal, never a fallback: the request is not executed.
        /// </summary>
        [HttpGet("banks")]
        [HttpGet("ifsc")]
        [HttpGet("sender/register")]
        [HttpGet("sender/otp/verify")]
        [HttpGet("sender/otp/resend")]
        [HttpGet("sender/fetch")]
        [HttpGet("beneficiary/register")]
        [HttpGet("beneficiary/otp/verify")]
        [HttpGet("beneficiary/otp/resend")]
        [HttpGet("beneficiary/list")]
        [HttpGet("beneficiary/delete")]
        [HttpGet("beneficiary/verify")]
        [HttpGet("transfer")]
        [HttpGet("status")]
        public IActionResult MethodNotAllowed() => Ok(ApiUserResponseFactory.Fail(
            new Responses.ApiUser.ApiUserDmtAckResponse(),
            ApiUserErrorCode.MethodNotAllowed,
            $"{Request.Path} accepts POST only. Only /api/v1/dmt/balance may be called with GET. " +
            "Money-moving and customer-data endpoints must not put parameters in a URL.",
            Request.Query["requestid"].FirstOrDefault() ?? string.Empty));

        // ─────────────────────────────────────────────────────────────────────
        //  Binding
        // ─────────────────────────────────────────────────────────────────────

        private ApiUserDmtSenderOtpRequest BindSenderOtp()
        {
            var request = Bind(new ApiUserDmtSenderOtpRequest());
            request.SenderMobile = P("sendermobile");
            request.Otp = P("otp");
            return request;
        }

        private ApiUserDmtBeneficiaryOtpRequest BindBeneficiaryOtp()
        {
            var request = Bind(new ApiUserDmtBeneficiaryOtpRequest());
            request.BeneficiaryId = P("beneficiaryid");
            request.Otp = P("otp");
            return request;
        }
    }
}
