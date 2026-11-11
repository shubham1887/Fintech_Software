using System.Globalization;
using Finocore.Integration;

// Payout-as-DMT runnable example.
//
//   dotnet run -- --selftest
//       Reproduces the worked example from the integration guide. No network, no credentials.
//       If this prints MATCH, your copy of the signing code is correct.
//
//   dotnet run -- --base https://api.finocore.net --mobile 9000012345 --token YOUR_TOKEN \
//                 --sendermobile 9876543210 --account 50100234567890 --ifsc HDFC0000123 \
//                 --name "Sunita Devi" --amount 100
//       Sends one real transfer, then checks its status with the SAME requestid.
//
// Every argument also reads from the environment (FINO_BASE, FINO_MOBILE, FINO_TOKEN, …) so a
// token never has to sit in shell history.

var opts = Args.Parse(args);

if (opts.Has("selftest")) return SelfTest();

string baseUrl = opts.Require("base", "FINO_BASE", "API base URL, e.g. https://api.finocore.net");
string mobile = opts.Require("mobile", "FINO_MOBILE", "Your registered mobile — panel → API Access");
string token = opts.Require("token", "FINO_TOKEN", "Your API token — panel → API Access");

string senderMobile = opts.Require("sendermobile", "FINO_SENDERMOBILE",
    "The customer's 10-digit mobile. Required, but it does not have to be registered anywhere");
string account = opts.Require("account", "FINO_ACCOUNT", "Destination account number");
string ifsc = opts.Require("ifsc", "FINO_IFSC", "Destination IFSC");
string holder = opts.Require("name", "FINO_NAME", "Account holder's name");
decimal amount = decimal.Parse(opts.Get("amount", "FINO_AMOUNT") ?? "100", CultureInfo.InvariantCulture);
string payType = (opts.Get("type", "FINO_TYPE") ?? "IMPS").ToUpperInvariant();

// Generated ONCE and reused by the status call. In a real system this is written to your
// database before the transfer call and kept for the life of the transaction.
string requestId = opts.Get("requestid", "FINO_REQUESTID") ?? $"PO-{DateTime.Now:yyyyMMddHHmmss}";

var client = new FinocoreDmtClient(baseUrl, mobile, token);

Console.WriteLine($"Request ID : {requestId}   (persist this BEFORE sending)");
Console.WriteLine($"Sending    : {amount} to {account} / {ifsc} ({holder}) over {payType}");
Console.WriteLine();

DmtResult result;
try
{
    result = await client.TransferAsync(
        requestId: requestId,
        senderMobile: senderMobile,
        accountNo: account,
        ifsc: ifsc,
        beneficiaryName: holder,
        amount: amount,
        paymentType: payType,
        remark: "API test",
        latitude: opts.Get("lat", "FINO_LAT"),
        longitude: opts.Get("long", "FINO_LONG"));
}
catch (Exception ex)
{
    // A transport failure does NOT mean the transfer did not happen. This is the one branch
    // where a wrong assumption sends the money twice.
    Console.WriteLine($"Transport failure: {ex.Message}");
    Console.WriteLine($"→ Resolve with /dmt/status and requestid {requestId}. Do NOT re-send with a new one.");
    return 1;
}

Print("TRANSFER", result);

if (result.Duplicate)
    Console.WriteLine("This requestid had already been used — the original transfer was returned, not a second one.");

if (result.Final)
{
    Console.WriteLine(result.IsSuccess
        ? $"Settled. UTR {result.Utr} — quote this to the beneficiary's bank, not providerref."
        : "Final and not successful. Nothing further to wait for.");
    return 0;
}

Console.WriteLine("Still PENDING. Waiting 60s before the first status check, as the guide advises …");
await Task.Delay(TimeSpan.FromSeconds(60));

for (int attempt = 1; attempt <= 5; attempt++)
{
    var status = await client.StatusAsync(requestId);
    Print($"STATUS {attempt}", status);
    if (status.Final) return 0;
    await Task.Delay(TimeSpan.FromSeconds(30));
}

Console.WriteLine("Still pending after 5 checks. Leave it pending and settle on the callback — never re-send.");
return 0;

static void Print(string label, DmtResult r)
{
    Console.WriteLine($"── {label}");
    Console.WriteLine($"   status     {r.Status}  (statuscode {r.StatusCode}, final={r.Final})");
    if (r.ErrorCode.Length > 0) Console.WriteLine($"   errorcode  {r.ErrorCode}");
    Console.WriteLine($"   message    {r.Message}");
    if (r.TxnId > 0)
    {
        Console.WriteLine($"   txnid      {r.TxnId}");
        Console.WriteLine($"   amount     {r.Amount}   debited {r.DebitAmount}   charge {r.Charge}");
        Console.WriteLine($"   balance    {r.Balance}");
        Console.WriteLine($"   utr        {(r.Utr.Length > 0 ? r.Utr : "(empty until SUCCESS)")}");
    }
    Console.WriteLine();
}

// Reproduces the published worked example. Nothing here touches the network, so it is the first
// thing to run on a new integration — and the first thing to re-run when signatures start failing.
static int SelfTest()
{
    const string token = "TESTTOKEN123456";
    const string timestamp = "1760342400";
    const string nonce = "5c2f7a91-3e48-4d7b-9a02-6f1b8c4d2e30";
    const string body =
        "{\"mobile\":\"9000012345\",\"requestid\":\"PO-1001\",\"sendermobile\":\"9876543210\"," +
        "\"accountno\":\"50100234567890\",\"ifsc\":\"HDFC0000123\",\"beneficiaryname\":\"Sunita Devi\"," +
        "\"amount\":\"5000\",\"paymenttype\":\"IMPS\",\"latitude\":\"22.7196\",\"longitude\":\"75.8577\"}";

    const string expectedSignature = "f14499d4ddf6768d290e9e952f3717ff64eb1d72da584727b1ae8c31552f89fe";

    string actual = FinocoreDmtClient.Sign(
        "POST", "/api/v1/dmt/transfer", timestamp, nonce,
        System.Text.Encoding.UTF8.GetBytes(body), token);

    Console.WriteLine($"signature  {actual}");
    Console.WriteLine($"expected   {expectedSignature}");
    Console.WriteLine();
    bool ok = actual == expectedSignature;
    Console.WriteLine(ok
        ? "MATCH — your signing is correct."
        : "MISMATCH — do not go live. Check that you hash the exact bytes you send.");
    return ok ? 0 : 1;
}

/// <summary>Minimal --flag / environment-variable argument reader.</summary>
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
