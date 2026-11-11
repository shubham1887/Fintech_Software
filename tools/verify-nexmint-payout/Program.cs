using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Fino.NexmintApi.Configuration;
using Fino.NexmintApi.Repositories;
using Fino.NexmintApi.Requests;
using Fino.NexmintApi.Security;
using Fino.NexmintApi.Services;
using Microsoft.Extensions.Configuration;

// Verifies the Nexmint payout integration against the written API specification, with no
// credentials and no money: the REAL NexmintApiClient and NexmintApiService run end to end over a
// stubbed HTTP handler, so the URL, the two headers, the signed payload and the parsing of every
// documented reply are all the production code paths - only the socket is stood in for.
//
// The response bodies below are copied from the Nexmint docs (Initiate Payout, Transaction Status,
// Wallet Balance, Webhooks), so a provider change that breaks us shows up as a failing line here
// rather than as a stuck payout.
//
//   dotnet run --project tools/verify-nexmint-payout

const string ClientID = "DFLT3f2a1b4c8d9e0f1a2b3c4d5e6f7a";
const string ClientSecret = "xK9mP2qRvT5wL8nJ3hY6bD1cF4eG7iA0";
const string WebhookSecret = "whsec_0f1e2d3c4b5a69788796a5b4c3d2e1f0";

int pass = 0, fail = 0;

void Check(string what, string actual, string expected)
{
    bool ok = string.Equals(actual, expected, StringComparison.Ordinal);
    if (ok) pass++; else fail++;
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what,-54} {Short(actual),-34} expected {Short(expected)}");
}

void CheckTrue(string what, bool actual)
{
    if (actual) pass++; else fail++;
    Console.WriteLine($"  {(actual ? "PASS" : "FAIL")}  {what}");
}

string Short(string value) => value is null ? "(null)" : value.Length > 32 ? value[..32] + "…" : value;

void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine($"── {title} " + new string('─', Math.Max(0, 86 - title.Length)));
}

// The service reads its defaults from the "NexmintApi" section, exactly as the API does.
IConfiguration Config(bool sandbox) => new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string>
    {
        ["NexmintApi:BaseUrl"] = "https://api.nexmint.in/api/v1/gateway",
        ["NexmintApi:UseSandbox"] = sandbox ? "true" : "false",
        ["NexmintApi:TimeoutSeconds"] = "60"
    })
    .Build();

NexmintCredentials Creds() => new()
{
    ClientID = ClientID,
    ClientSecret = ClientSecret,
    WebhookSecret = WebhookSecret
};

(NexmintApiService Service, StubHandler Stub) Build(bool sandbox = false, HttpStatusCode status = HttpStatusCode.OK, string body = "{}")
{
    var stub = new StubHandler { Status = status, Body = body };
    var service = new NexmintApiService(new NexmintApiClient(new StubFactory(stub)), Config(sandbox));
    return (service, stub);
}

Console.WriteLine("Nexmint payout — request shape, signing, parsing and webhook verification");
Console.WriteLine(new string('=', 100));

// ─────────────────────────────────────────────────────────────────────────────────────────────
Section("JWT — HS256, and the iat the gateway rejects anything without");

var token = NexmintJwt.AuthToken(ClientID, ClientSecret);
string[] parts = token.Split('.');
Check("auth token has three segments", parts.Length.ToString(), "3");
Check("header", Utf8(FromBase64Url(parts[0])), "{\"alg\":\"HS256\",\"typ\":\"JWT\"}");

// Signature recomputed here, independently of the library.
using (var mac = new HMACSHA256(Encoding.UTF8.GetBytes(ClientSecret)))
{
    string expected = ToBase64Url(mac.ComputeHash(Encoding.UTF8.GetBytes($"{parts[0]}.{parts[1]}")));
    Check("signature is HMACSHA256 over header.payload", parts[2], expected);
}

var authClaims = JsonDocument.Parse(Utf8(FromBase64Url(parts[1]))).RootElement;
Check("auth token carries client_id", authClaims.GetProperty("client_id").GetString(), ClientID);
long iat = authClaims.GetProperty("iat").GetInt64();
long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
CheckTrue($"iat is stamped now (iat={iat}, now={now}, drift={now - iat}s, must be < 5)", now - iat < 5);
CheckTrue("verifying with the right secret succeeds", NexmintJwt.TryVerify(token, ClientSecret, out _));
CheckTrue("verifying with the wrong secret fails", !NexmintJwt.TryVerify(token, "wrong-secret", out _));

// ─────────────────────────────────────────────────────────────────────────────────────────────
Section("Initiate payout — endpoint, headers and the signed payload");

var (service, stub) = Build(body: InitiateReply);
var payout = new PayoutRequest
{
    OrderID = "ORD-20260325-001",
    PaymentMethod = "upi",                        // not a payout rail -> IMPS
    AccountHolderName = "Ramesh  Kumar-Singh.",   // punctuation is stripped upstream
    BankName = "State Bank of India",
    AccountNumber = "1234567890",
    IFSC = "sbin0001234",
    Mobile = "+91 98765 43210",
    Amount = 1000.99m,                            // decimals are truncated upstream
    AccountType = "Current",
    Remark = ""                                   // mandatory upstream -> defaulted
};
var reply = await service.SendPayout(payout, Creds());

Check("method", stub.Method, "POST");
Check("url", stub.Url, "https://api.nexmint.in/api/v1/gateway/fund-transfer");
Check("client-id header", stub.ClientIdHeader, ClientID);
CheckTrue("Authorization is a Bearer JWT signed with the client secret",
    stub.AuthorizationHeader.StartsWith("Bearer ") &&
    NexmintJwt.TryVerify(stub.AuthorizationHeader["Bearer ".Length..], ClientSecret, out _));

var sent = JsonDocument.Parse(stub.SentBody).RootElement;
Check("body has exactly one field", sent.EnumerateObject().Count().ToString(), "1");
string encrypted = sent.GetProperty("encrypted_payload").GetString();
CheckTrue("encrypted_payload verifies against the client secret",
    NexmintJwt.TryVerify(encrypted, ClientSecret, out string payloadJson));

var sentPayload = JsonDocument.Parse(payloadJson).RootElement;
Check("order_id", sentPayload.GetProperty("order_id").GetString(), "ORD-20260325-001");
Check("payment_method (UPI has no payout rail)", sentPayload.GetProperty("payment_method").GetString(), "IMPS");
Check("account_holder_name (letters and spaces)", sentPayload.GetProperty("account_holder_name").GetString(), "Ramesh Kumar Singh");
Check("bank_name", sentPayload.GetProperty("bank_name").GetString(), "State Bank of India");
Check("account_number", sentPayload.GetProperty("account_number").GetString(), "1234567890");
Check("ifsc_code (upper-cased)", sentPayload.GetProperty("ifsc_code").GetString(), "SBIN0001234");
Check("mobile (+91 and spaces stripped)", sentPayload.GetProperty("mobile").GetString(), "9876543210");
Check("amount (truncated, sent as a number)", sentPayload.GetProperty("amount").GetRawText(), "1000");
Check("account_type", sentPayload.GetProperty("account_type").GetString(), "current");
Check("remark (mandatory, so defaulted)", sentPayload.GetProperty("remark").GetString(), "Payout");
CheckTrue("payload carries its own iat", sentPayload.TryGetProperty("iat", out _));

Check("reply: status", reply.Status, "PENDING");
Check("reply: transaction_id", reply.TransactionID, "TXN20260325ABCD1234");
Check("reply: order_id", reply.OrderID, "ORD-20260325-001");
Check("reply: amount", reply.Amount.ToString("0.00"), "1000.00");
CheckTrue("reply: accepted", reply.Success && reply.ApiSuccess && !reply.Retryable);

// ─────────────────────────────────────────────────────────────────────────────────────────────
Section("Sandbox — the same call, Nexmint's test money");

var (sandboxService, sandboxStub) = Build(sandbox: true, body: InitiateReply);
await sandboxService.SendPayout(payout, Creds());
Check("url", sandboxStub.Url, "https://api.nexmint.in/api/v1/gateway/sandbox/fund-transfer");

// A row whose URL already ends in /sandbox must not become /sandbox/sandbox.
var (pastedService, pastedStub) = Build(body: InitiateReply);
await pastedService.SendPayout(payout, new NexmintCredentials
{
    ClientID = ClientID,
    ClientSecret = ClientSecret,
    BaseUrl = "https://api.nexmint.in/api/v1/gateway/sandbox/"
});
Check("a pasted /sandbox URL is not doubled", pastedStub.Url, "https://api.nexmint.in/api/v1/gateway/sandbox/fund-transfer");

// ─────────────────────────────────────────────────────────────────────────────────────────────
Section("Refusals that never reach the network");

async Task CheckRefusal(string what, PayoutRequest bad)
{
    var (svc, s) = Build(body: InitiateReply);
    var r = await svc.SendPayout(bad, Creds());
    bool ok = !r.Success && !r.Retryable && !s.Called && r.Message.Length > 0;
    if (ok) pass++; else fail++;
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what,-54} {Short(r.Message)}");
}

PayoutRequest Valid() => new()
{
    OrderID = "ORD-1", PaymentMethod = "IMPS", AccountHolderName = "Ramesh Kumar",
    BankName = "State Bank of India", AccountNumber = "1234567890", IFSC = "SBIN0001234",
    Mobile = "9876543210", Amount = 100, AccountType = "saving", Remark = "Salary"
};

await CheckRefusal("blank order id", Mutate(Valid(), p => p.OrderID = ""));
await CheckRefusal("blank account number", Mutate(Valid(), p => p.AccountNumber = ""));
await CheckRefusal("blank IFSC", Mutate(Valid(), p => p.IFSC = ""));
await CheckRefusal("name with no letters", Mutate(Valid(), p => p.AccountHolderName = "123 ###"));
await CheckRefusal("short mobile", Mutate(Valid(), p => p.Mobile = "98765"));
await CheckRefusal("amount below 1", Mutate(Valid(), p => p.Amount = 0.40m));

var (unconfigured, unconfiguredStub) = Build(body: InitiateReply);
var unconfiguredReply = await unconfigured.SendPayout(Valid(), new NexmintCredentials());
CheckTrue("no credentials: refused locally, nothing sent", !unconfiguredReply.Success && !unconfiguredStub.Called);

// ─────────────────────────────────────────────────────────────────────────────────────────────
Section("Status enquiry");

var (statusService, statusStub) = Build(body: StatusSuccessReply);
var statusReply = await statusService.GetPayoutStatus(new PayoutStatusRequest { OrderID = "ORD-20260325-001" }, Creds());
Check("url", statusStub.Url, "https://api.nexmint.in/api/v1/gateway/fund-transfer/status");
NexmintJwt.TryVerify(JsonDocument.Parse(statusStub.SentBody).RootElement.GetProperty("encrypted_payload").GetString(),
    ClientSecret, out string statusPayload);
var asked = JsonDocument.Parse(statusPayload).RootElement;
Check("asks by order_id", asked.GetProperty("order_id").GetString(), "ORD-20260325-001");
CheckTrue("does not also send a blank transaction_id", !asked.TryGetProperty("transaction_id", out _));
Check("status", statusReply.Status, "SUCCESS");
Check("utr_number", statusReply.UtrNumber, "400123456789");

var (bothService, bothStub) = Build(body: StatusSuccessReply);
await bothService.GetPayoutStatus(new PayoutStatusRequest { TransactionID = "TXN20260325ABCD1234", OrderID = "ORD-1" }, Creds());
NexmintJwt.TryVerify(JsonDocument.Parse(bothStub.SentBody).RootElement.GetProperty("encrypted_payload").GetString(),
    ClientSecret, out string bothPayload);
var bothAsked = JsonDocument.Parse(bothPayload).RootElement;
CheckTrue("given both, asks by transaction_id alone",
    bothAsked.TryGetProperty("transaction_id", out _) && !bothAsked.TryGetProperty("order_id", out _));

var (noRefService, noRefStub) = Build(body: StatusSuccessReply);
var noRef = await noRefService.GetPayoutStatus(new PayoutStatusRequest(), Creds());
CheckTrue("neither reference: refused locally, nothing sent", !noRef.Success && !noRefStub.Called);

var (failedService, _) = Build(body: StatusFailedRefundPendingReply);
var failedReply = await failedService.GetPayoutStatus(new PayoutStatusRequest { OrderID = "ORD-1" }, Creds());
Check("failed transfer: status", failedReply.Status, "FAILED");
Check("failed transfer: refund_status", failedReply.RefundStatus, "pending");
Check("failed transfer: message", failedReply.Message, "Payment failed — refund has been initiated");

// ─────────────────────────────────────────────────────────────────────────────────────────────
Section("Wallet balance");

var (balanceService, balanceStub) = Build(body: BalanceReply);
var balance = await balanceService.GetBalance(Creds());
Check("method", balanceStub.Method, "GET");
Check("url", balanceStub.Url, "https://api.nexmint.in/api/v1/gateway/fund-transfer/balance");
Check("no body is sent", balanceStub.SentBody, "");
Check("balance", balance.Balance.ToString("0.00"), "12500.00");
Check("hold_balance", balance.HoldBalance.ToString("0.00"), "500.00");

// ─────────────────────────────────────────────────────────────────────────────────────────────
Section("Failure modes — a refusal and an outage must not look alike");

var (refusedService, _) = Build(status: HttpStatusCode.BadRequest, body: "{\"success\":false,\"message\":\"Invalid or Duplicate order ID\"}");
var refused = await refusedService.SendPayout(Valid(), Creds());
CheckTrue("HTTP 400 is a definite no, not retryable", refused.Success && !refused.ApiSuccess && !refused.Retryable);
Check("400 message reaches the caller", refused.Message, "Invalid or Duplicate order ID");

var (brokenService, _) = Build(status: HttpStatusCode.InternalServerError, body: "{\"success\":false,\"message\":\"Internal Server Error\"}");
var broken = await brokenService.SendPayout(Valid(), Creds());
CheckTrue("HTTP 500 is retryable — the payout may exist upstream", broken.Retryable);

var (gibberishService, _) = Build(body: "<html>502 Bad Gateway</html>");
var gibberish = await gibberishService.SendPayout(Valid(), Creds());
CheckTrue("a non-JSON reply is not read as an accepted payout", !gibberish.ApiSuccess);

var dead = await new NexmintApiService(new NexmintApiClient(new StubFactory(new StubHandler { Throw = true })), Config(false))
    .SendPayout(Valid(), Creds());
CheckTrue("a network failure is retryable, never failed", dead.Retryable && !dead.Success);

// ─────────────────────────────────────────────────────────────────────────────────────────────
Section("Webhook — the only thing that may settle a payout unasked");

var (callbackService, _) = Build();
string webhookBody = WebhookPush;
string goodToken = NexmintJwt.Sign(new Dictionary<string, object> { ["event"] = "payout" }, WebhookSecret);

var verified = callbackService.ParseCallback($"Bearer {goodToken}", webhookBody, Creds());
CheckTrue("a correctly signed push verifies", verified.Verified);
Check("order_id", verified.OrderID, "ORD-20260325-001");
Check("status", verified.Status, "SUCCESS");
Check("utr_number", verified.UtrNumber, "UTR826491057382");
Check("amount", verified.Amount.ToString("0.00"), "1000.00");

var wrongKey = callbackService.ParseCallback(
    $"Bearer {NexmintJwt.Sign(new Dictionary<string, object> { ["event"] = "payout" }, "not-the-webhook-secret")}",
    webhookBody, Creds());
CheckTrue("a push signed with the wrong key is rejected", !wrongKey.Verified);

CheckTrue("an unsigned push is rejected", !callbackService.ParseCallback(null, webhookBody, Creds()).Verified);
CheckTrue("a tampered token is rejected",
    !callbackService.ParseCallback($"Bearer {goodToken[..^2]}xx", webhookBody, Creds()).Verified);
CheckTrue("no webhook secret configured: nothing is accepted",
    !callbackService.ParseCallback($"Bearer {goodToken}", webhookBody,
        new NexmintCredentials { ClientID = ClientID, ClientSecret = ClientSecret }).Verified);

// The client secret is NOT the webhook secret; a push signed with it must not pass.
CheckTrue("a push signed with the client secret is rejected",
    !callbackService.ParseCallback($"Bearer {NexmintJwt.Sign(new Dictionary<string, object>(), ClientSecret)}",
        webhookBody, Creds()).Verified);

Console.WriteLine();
Console.WriteLine(new string('=', 100));
Console.WriteLine($"{pass} passed, {fail} failed");
return fail == 0 ? 0 : 1;

static PayoutRequest Mutate(PayoutRequest request, Action<PayoutRequest> change)
{
    change(request);
    return request;
}

static string ToBase64Url(byte[] bytes) =>
    Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

static byte[] FromBase64Url(string value)
{
    string padded = value.Replace('-', '+').Replace('_', '/');
    switch (padded.Length % 4)
    {
        case 2: padded += "=="; break;
        case 3: padded += "="; break;
    }
    return Convert.FromBase64String(padded);
}

static string Utf8(byte[] bytes) => Encoding.UTF8.GetString(bytes);

// ── Documented replies, copied from the Nexmint API reference ────────────────────────────────

partial class Program
{
    public const string InitiateReply = """
    {
      "success": true,
      "message": "Fund Transfer Intiated Successfully",
      "data": {
        "transaction_id": "TXN20260325ABCD1234",
        "order_id": "ORD-20260325-001",
        "status": "pending",
        "amount": "1000.00"
      }
    }
    """;

    public const string StatusSuccessReply = """
    {
      "success": true,
      "data": {
        "transaction_id": "TXN20260325ABCD1234",
        "order_id": "ORD-20260325-001",
        "status": "success",
        "message": "Payment processed successfully",
        "utr_number": "400123456789",
        "refund_status": null,
        "refund_transaction_id": null
      }
    }
    """;

    public const string StatusFailedRefundPendingReply = """
    {
      "success": true,
      "data": {
        "status": "failed",
        "message": "Payment failed — refund has been initiated",
        "transaction_id": "TXN20260325ABCD1234",
        "order_id": "ORD-20260325-001",
        "utr_number": null,
        "refund_status": "pending",
        "refund_transaction_id": null
      }
    }
    """;

    public const string BalanceReply = """
    {
      "success": true,
      "data": {
        "balance": 12500.00,
        "hold_balance": 500.00
      }
    }
    """;

    public const string WebhookPush = """
    {
      "transaction_id": "TXN20260325ABCD1234",
      "order_id": "ORD-20260325-001",
      "amount": "1000.00",
      "payment_method": "IMPS",
      "remark": "March salary",
      "account_holder_name": "Ramesh Kumar",
      "bank_name": "State Bank of India",
      "account_number": "1234567890",
      "ifsc_code": "SBIN0001234",
      "mobile": "9876543210",
      "account_type": "saving",
      "status": "success",
      "message": "Payment processed successfully",
      "utr_number": "UTR826491057382",
      "refund_status": null,
      "refund_transaction_id": null,
      "initiated_at": "2026-03-25 10:30:00",
      "cleared_at": "2026-03-25 10:31:42"
    }
    """;
}

// ── The socket, and only the socket ──────────────────────────────────────────────────────────

class StubHandler : HttpMessageHandler
{
    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
    public string Body { get; set; } = "{}";
    public bool Throw { get; set; }

    public bool Called { get; private set; }
    public string Method { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string ClientIdHeader { get; private set; } = string.Empty;
    public string AuthorizationHeader { get; private set; } = string.Empty;
    public string SentBody { get; private set; } = string.Empty;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Called = true;
        Method = request.Method.Method;
        Url = request.RequestUri?.ToString() ?? string.Empty;
        ClientIdHeader = Header(request, "client-id");
        AuthorizationHeader = Header(request, "Authorization");
        SentBody = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);

        if (Throw) throw new HttpRequestException("connection refused");

        return new HttpResponseMessage(Status) { Content = new StringContent(Body, Encoding.UTF8, "application/json") };
    }

    private static string Header(HttpRequestMessage request, string name) =>
        request.Headers.TryGetValues(name, out var values) ? string.Join(",", values) : string.Empty;
}

class StubFactory : IHttpClientFactory
{
    private readonly StubHandler _handler;
    public StubFactory(StubHandler handler) => _handler = handler;
    public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
}
