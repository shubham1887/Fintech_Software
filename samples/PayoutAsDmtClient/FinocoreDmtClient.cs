using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Finocore.Integration;

/// <summary>
/// A drop-in client for the payout-as-DMT contract (POST /api/v1/dmt/transfer).
///
/// One call moves the money: there is no sender registration, no beneficiary registration and no
/// OTP, so the destination account travels on the transfer request itself. Copy this file into
/// your project — it has no dependencies beyond the .NET base class library.
///
/// The one rule the whole integration turns on: the body is serialised to a string ONCE, and that
/// exact string is both hashed and sent. Hashing a re-serialised copy — a different key order, a
/// space more or less — produces a different signature and every request is refused with
/// SEC_SIGNATURE_INVALID. <see cref="SendAsync"/> is written so that cannot happen by accident.
/// </summary>
public sealed class FinocoreDmtClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _mobile;
    private readonly string _apiToken;

    /// <param name="baseUrl">e.g. https://api.finocore.net</param>
    /// <param name="mobile">Your account's registered 10-digit mobile (panel → API Access).</param>
    /// <param name="apiToken">Your API token. Also the HMAC signing key — keep it in a secret store.</param>
    /// <param name="http">
    /// Supply a pooled HttpClient from IHttpClientFactory in a server app. A new HttpClient per
    /// call exhausts sockets under load, which looks exactly like the gateway being down.
    /// </param>
    public FinocoreDmtClient(string baseUrl, string mobile, string apiToken, HttpClient? http = null)
    {
        _baseUrl = (baseUrl ?? throw new ArgumentNullException(nameof(baseUrl))).TrimEnd('/');
        _mobile = mobile ?? throw new ArgumentNullException(nameof(mobile));
        _apiToken = apiToken ?? throw new ArgumentNullException(nameof(apiToken));
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(90) };
    }

    // ── The two calls ────────────────────────────────────────────────────────

    /// <summary>
    /// Sends money to a bank account.
    ///
    /// <paramref name="requestId"/> is YOUR reference and the idempotency key. Write it to your
    /// database BEFORE calling: a requestid you generated but never persisted cannot be used to
    /// resolve a timeout, and a timeout is the one case where guessing costs real money.
    /// </summary>
    public Task<DmtResult> TransferAsync(
        string requestId,
        string senderMobile,
        string accountNo,
        string ifsc,
        string beneficiaryName,
        decimal amount,
        string paymentType = "IMPS",
        string? remark = null,
        string? latitude = null,
        string? longitude = null,
        CancellationToken cancellationToken = default)
    {
        // Ordered so the JSON reads the way the documentation shows it. The order does not matter
        // to the server — the signature is over whatever bytes we send — but a payload that
        // matches the docs is one less thing to doubt when something is wrong.
        var fields = new Dictionary<string, string>
        {
            ["mobile"] = _mobile,
            ["requestid"] = requestId,
            ["sendermobile"] = senderMobile,
            ["accountno"] = accountNo,
            ["ifsc"] = ifsc.ToUpperInvariant(),
            ["beneficiaryname"] = beneficiaryName,
            // Invariant culture: on a machine with a comma decimal separator "5000.50" would
            // otherwise go out as "5000,50" and be refused as a non-number.
            ["amount"] = amount.ToString("0.##", CultureInfo.InvariantCulture),
            ["paymenttype"] = paymentType.ToUpperInvariant(),
        };

        if (!string.IsNullOrWhiteSpace(remark)) fields["remark"] = remark!;

        // Both or neither — the server refuses a half pair with VAL_LOCATION_INVALID, because a
        // transfer stored with half a coordinate is an audit record that proves nothing.
        if (!string.IsNullOrWhiteSpace(latitude) && !string.IsNullOrWhiteSpace(longitude))
        {
            fields["latitude"] = latitude!;
            fields["longitude"] = longitude!;
        }

        return SendAsync("/api/v1/dmt/transfer", fields, cancellationToken);
    }

    /// <summary>
    /// The current state of a transfer, by your own requestid. The reply is the same shape as
    /// <see cref="TransferAsync"/>, so one handler settles both.
    /// </summary>
    public Task<DmtResult> StatusAsync(string requestId, CancellationToken cancellationToken = default) =>
        SendAsync("/api/v1/dmt/status", new Dictionary<string, string>
        {
            ["mobile"] = _mobile,
            ["requestid"] = requestId,
        }, cancellationToken);

    /// <summary>The same enquiry by our transaction id, for when you kept that instead.</summary>
    public Task<DmtResult> StatusByTxnIdAsync(int txnId, CancellationToken cancellationToken = default) =>
        SendAsync("/api/v1/dmt/status", new Dictionary<string, string>
        {
            ["mobile"] = _mobile,
            ["txnid"] = txnId.ToString(CultureInfo.InvariantCulture),
        }, cancellationToken);

    // ── Signing and transport ────────────────────────────────────────────────

    private async Task<DmtResult> SendAsync(
        string path, Dictionary<string, string> fields, CancellationToken cancellationToken)
    {
        // Serialise ONCE. Everything below — the hash, the bytes on the wire — uses this string
        // and nothing else.
        string body = JsonSerializer.Serialize(fields);
        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

        // Epoch SECONDS. Milliseconds read as roughly 55,000 years in the future and are refused
        // with SEC_TIMESTAMP_INVALID; it is the second most common integration mistake.
        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

        // Single-use, per REQUEST. A nonce cached per session is replayed and refused.
        string nonce = Guid.NewGuid().ToString();

        string signature = Sign("POST", path, timestamp, nonce, bodyBytes, _apiToken);

        using var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + path)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("X-Api-Mobile", _mobile);
        request.Headers.Add("X-Api-Token", _apiToken);
        request.Headers.Add("X-Api-Timestamp", timestamp);
        request.Headers.Add("X-Api-Nonce", nonce);
        request.Headers.Add("X-Api-Signature", signature);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        // Every reply is HTTP 200 and carries the outcome in the body, so the status code is
        // never the verdict. A non-200 means the request did not reach the contract at all.
        var result = JsonSerializer.Deserialize<DmtResult>(text) ?? new DmtResult();
        result.RawResponse = text;
        result.HttpStatus = (int)response.StatusCode;
        return result;
    }

    /// <summary>
    /// Builds the canonical string and signs it. Public so your tests can reproduce the worked
    /// example from the integration guide without going near the network.
    ///
    /// canonical = METHOD \n PATH(lowercase) \n TIMESTAMP \n NONCE \n SHA256(body) as lowercase hex
    /// </summary>
    public static string Sign(string method, string path, string timestamp, string nonce, byte[] body, string apiToken)
    {
        string canonical = string.Join('\n',
            method.ToUpperInvariant(),
            path.ToLowerInvariant(),
            timestamp,
            nonce,
            Hex(SHA256.HashData(body ?? Array.Empty<byte>())));

        return Hex(HMACSHA256.HashData(Encoding.UTF8.GetBytes(apiToken), Encoding.UTF8.GetBytes(canonical)));
    }

    /// <summary>
    /// Verifies an inbound callback. Call this before you settle anything: an endpoint that does
    /// not check the signature accepts anyone's idea of "this transfer succeeded".
    /// </summary>
    public bool IsCallbackAuthentic(byte[] rawBody, string? suppliedSignature)
    {
        string expected = Hex(HMACSHA256.HashData(Encoding.UTF8.GetBytes(_apiToken), rawBody ?? Array.Empty<byte>()));
        string supplied = (suppliedSignature ?? string.Empty).Trim().ToLowerInvariant();

        if (supplied.Length != expected.Length || supplied.Length == 0) return false;

        // Fixed-time compare: a plain == returns at the first differing character, and that
        // timing difference is enough to recover a valid signature byte by byte.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(supplied), Encoding.ASCII.GetBytes(expected));
    }

    private static string Hex(byte[] data) => Convert.ToHexString(data).ToLowerInvariant();
}

/// <summary>
/// One transfer, as the contract reports it. The same object comes back from /dmt/transfer,
/// /dmt/status and the callback — only <see cref="Event"/> differs.
/// </summary>
public sealed class DmtResult
{
    [JsonPropertyName("status")] public string Status { get; set; } = "ERROR";
    [JsonPropertyName("statuscode")] public int StatusCode { get; set; }
    [JsonPropertyName("errorcode")] public string ErrorCode { get; set; } = "";
    [JsonPropertyName("message")] public string Message { get; set; } = "";
    [JsonPropertyName("requestid")] public string RequestId { get; set; } = "";
    [JsonPropertyName("event")] public string Event { get; set; } = "";
    [JsonPropertyName("final")] public bool Final { get; set; }
    [JsonPropertyName("duplicate")] public bool Duplicate { get; set; }
    [JsonPropertyName("txnid")] public int TxnId { get; set; }
    [JsonPropertyName("providerref")] public string ProviderRef { get; set; } = "";
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("debitamount")] public decimal DebitAmount { get; set; }
    [JsonPropertyName("balance")] public decimal Balance { get; set; }
    [JsonPropertyName("date")] public string Date { get; set; } = "";
    [JsonPropertyName("sendermobile")] public string SenderMobile { get; set; } = "";
    [JsonPropertyName("beneficiaryname")] public string BeneficiaryName { get; set; } = "";
    [JsonPropertyName("accountno")] public string AccountNo { get; set; } = "";
    [JsonPropertyName("ifsc")] public string Ifsc { get; set; } = "";
    [JsonPropertyName("bankname")] public string BankName { get; set; } = "";
    [JsonPropertyName("paymenttype")] public string PaymentType { get; set; } = "";

    /// <summary>The bank UTR. Present only once the transfer reached SUCCESS.</summary>
    [JsonPropertyName("utr")] public string Utr { get; set; } = "";

    [JsonPropertyName("charge")] public decimal Charge { get; set; }

    /// <summary>The reply exactly as received. Log this — support asks for it.</summary>
    [JsonIgnore] public string RawResponse { get; set; } = "";

    /// <summary>The HTTP code. Informational only: the contract answers 200 even for declines.</summary>
    [JsonIgnore] public int HttpStatus { get; set; }

    /// <summary>Money reached the beneficiary. A UTR is present.</summary>
    public bool IsSuccess => Status == "SUCCESS";

    /// <summary>Accepted and debited, not yet settled. Poll /dmt/status or wait for the callback.</summary>
    public bool IsPending => Status == "PENDING";

    /// <summary>Declined and refunded. Final.</summary>
    public bool IsFailed => Status == "FAILED";

    /// <summary>No transaction was created — auth, validation or a system fault. Nothing was debited.</summary>
    public bool IsError => Status == "ERROR";
}
