// PayoutApiTest — a signed end-to-end tester for the /api/v1/payout contract.
//
// It is the answer to the two questions every integrator asks first:
//
//   "What do I pass for beneficiaryid?"  → Nothing. Payout has no beneficiaryid. The destination
//                                          account, IFSC and holder name go on /payout/transfer.
//   "What do I authenticate with?"       → Your registered mobile number + API token. Panel → API
//                                          Access shows both ("API Mobile Number").
//                                          Pass it here with --mobile.
//
// Usage (one line, from this folder):
//
//   dotnet run -- --base https://api.example.com --mobile 9000012345 --token YOUR_TOKEN \
//                 --account 50100234567890 --ifsc HDFC0000123 --name "Sunita Devi" --amount 100
//
// Every argument may also come from the environment (PAYOUT_BASE, PAYOUT_MOBILE, PAYOUT_TOKEN, …)
// so a token never has to sit in shell history. Nothing is hard-coded: this file is safe to hand
// to a partner as-is.
//
// What it does, in order:
//   1. POST /payout/ifsc      — proves signing works on a call that moves no money.
//   2. GET  /payout/balance   — proves signing an EMPTY body works.
//   3. POST /payout/transfer  — the real thing, with a requestid it prints and reuses.
//   4. POST /payout/status    — the same requestid, which is how a timeout must be resolved.
//
// Pass --dry-run to do steps 1 and 2 only. Start there: it exercises the whole security gate
// without debiting a wallet.

using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var opts = Args.Parse(args);

string baseUrl = opts.Require("base", "PAYOUT_BASE", "API base URL, e.g. https://api.example.com");
string mobile = opts.Require("mobile", "PAYOUT_MOBILE", "Your registered mobile number — panel → API Access");
string token = opts.Require("token", "PAYOUT_TOKEN", "Your API token — panel → API Access");

baseUrl = baseUrl.TrimEnd('/');
bool dryRun = opts.Has("dry-run");

string account = opts.Get("account", "PAYOUT_ACCOUNT") ?? "";
string ifsc = (opts.Get("ifsc", "PAYOUT_IFSC") ?? "").ToUpperInvariant();
string holder = opts.Get("name", "PAYOUT_NAME") ?? "";
string amount = opts.Get("amount", "PAYOUT_AMOUNT") ?? "100";
string payType = (opts.Get("type", "PAYOUT_TYPE") ?? "IMPS").ToUpperInvariant();

// The caller's own reference, and the idempotency key. Generated ONCE and reused by the status
// call on purpose: a new requestid on a retry is a second payout, not a retry.
string requestId = opts.Get("requestid", "PAYOUT_REQUESTID") ?? $"PO-{DateTime.Now:yyyyMMddHHmmss}";

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(90) };

Console.WriteLine($"Base      : {baseUrl}");
Console.WriteLine($"Mobile    : {mobile}");
Console.WriteLine($"Token     : {Mask(token)}");
Console.WriteLine($"Request ID: {requestId}");
Console.WriteLine();

// ── 1. IFSC lookup — a signed POST that moves no money ──────────────────────────
if (ifsc.Length > 0)
{
    await Call(HttpMethod.Post, "/api/v1/payout/ifsc", new Dictionary<string, string>
    {
        ["requestid"] = requestId + "-IFSC",
        ["ifsc"] = ifsc,
    });
}

// ── 2. Balance — the only GET, and the one that proves empty-body signing ───────
await Call(HttpMethod.Get, "/api/v1/payout/balance", null);

if (dryRun)
{
    Console.WriteLine("--dry-run: stopping before the transfer. Signing works if both calls above returned SUCCESS.");
    return;
}

if (account.Length == 0 || ifsc.Length == 0 || holder.Length == 0)
{
    Console.WriteLine("To send a real payout, pass --account, --ifsc and --name (or use --dry-run).");
    return;
}

// ── 3. The payout itself ────────────────────────────────────────────────────────
// NOTE what is absent: no sendermobile, no beneficiaryid. Payout takes the destination inline.
Console.WriteLine($"Sending {amount} to {account} / {ifsc} ({holder}) over {payType} …");
Console.WriteLine();

await Call(HttpMethod.Post, "/api/v1/payout/transfer", new Dictionary<string, string>
{
    ["requestid"] = requestId,
    ["accountno"] = account,
    ["ifsc"] = ifsc,
    ["beneficiaryname"] = holder,
    ["amount"] = amount,
    ["paymenttype"] = payType,
    ["remark"] = "API test",
});

// ── 4. Status, with the SAME requestid ──────────────────────────────────────────
// A pending payout is re-enquired with the bank, so give it a moment before asking.
Console.WriteLine("Waiting 5s, then checking status with the SAME requestid …");
Console.WriteLine();
await Task.Delay(TimeSpan.FromSeconds(5));

await Call(HttpMethod.Post, "/api/v1/payout/status", new Dictionary<string, string>
{
    ["requestid"] = requestId,
});

Console.WriteLine("Done. If status is PENDING, poll /payout/status every 30s until final=true,");
Console.WriteLine($"always with requestid {requestId} — never with a new one.");

// ── The signing + call helper ───────────────────────────────────────────────────

/// <summary>
/// Signs and sends one request, then prints the reply.
///
/// The ONE rule that matters: the body is serialised to a string ONCE, and that exact string is
/// both hashed and sent. Hashing a re-serialised copy — different key order, different spacing —
/// is the single most common cause of SEC_SIGNATURE_INVALID.
/// </summary>
async Task Call(HttpMethod method, string path, Dictionary<string, string>? fields)
{
    string body = fields == null ? "" : JsonSerializer.Serialize(fields);
    byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

    // Epoch SECONDS, not milliseconds. Milliseconds reads as ~55,000 years in the future and
    // every request fails with SEC_TIMESTAMP_INVALID.
    string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

    // Single-use, per REQUEST — never per session.
    string nonce = Guid.NewGuid().ToString();

    // canonical = METHOD \n PATH(lowercase) \n TIMESTAMP \n NONCE \n SHA256(body) as hex
    string canonical = string.Join('\n',
        method.Method.ToUpperInvariant(),
        path.ToLowerInvariant(),
        timestamp,
        nonce,
        Hex(SHA256.HashData(bodyBytes)));

    string signature = Hex(HMACSHA256.HashData(Encoding.UTF8.GetBytes(token), Encoding.UTF8.GetBytes(canonical)));

    using var request = new HttpRequestMessage(method, baseUrl + path);
    request.Headers.Add("X-Api-Mobile", mobile);
    request.Headers.Add("X-Api-Token", token);
    request.Headers.Add("X-Api-Timestamp", timestamp);
    request.Headers.Add("X-Api-Nonce", nonce);
    request.Headers.Add("X-Api-Signature", signature);
    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    if (fields != null)
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

    Console.WriteLine($"── {method.Method} {path}");
    if (fields != null) Console.WriteLine($"   body {body}");

    try
    {
        using var response = await http.SendAsync(request);
        string text = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"   HTTP {(int)response.StatusCode}");
        Console.WriteLine(Pretty(text));
    }
    catch (Exception ex)
    {
        // A transport failure on /payout/transfer does NOT mean the payout did not happen.
        Console.WriteLine($"   transport failure: {ex.Message}");
        Console.WriteLine($"   → resolve it with /payout/status and requestid {requestId}. Do not re-send.");
    }

    Console.WriteLine();
}

static string Hex(byte[] data) => Convert.ToHexString(data).ToLowerInvariant();

static string Mask(string value) =>
    value.Length <= 4 ? new string('*', value.Length) : value[..4] + new string('*', Math.Min(value.Length - 4, 8));

static string Pretty(string json)
{
    try
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
    }
    catch { return json; }
}

/// <summary>Minimal --flag value / environment-variable argument reader.</summary>
file sealed class Args
{
    private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _flags = new(StringComparer.OrdinalIgnoreCase);

    public static Args Parse(string[] args)
    {
        var parsed = new Args();
        for (int i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith("--", StringComparison.Ordinal)) continue;
            string key = args[i][2..];
            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                parsed._values[key] = args[++i];
            else
                parsed._flags.Add(key);
        }
        return parsed;
    }

    public bool Has(string flag) => _flags.Contains(flag);

    public string? Get(string key, string envVar)
    {
        if (_values.TryGetValue(key, out string? v) && !string.IsNullOrWhiteSpace(v)) return v.Trim();
        string? env = Environment.GetEnvironmentVariable(envVar);
        return string.IsNullOrWhiteSpace(env) ? null : env.Trim();
    }

    public string Require(string key, string envVar, string what)
    {
        string? value = Get(key, envVar);
        if (value != null) return value;

        Console.Error.WriteLine($"Missing --{key} (or {envVar}): {what}");
        Environment.Exit(2);
        return string.Empty;
    }
}
