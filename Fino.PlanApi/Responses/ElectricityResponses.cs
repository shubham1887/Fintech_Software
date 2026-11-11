using System.Globalization;
using System.Text.Json;

namespace Fino.PlanApi.Responses
{
    /// <summary>
    /// The bill as the provider states it, normalised for direct display.
    ///
    /// The provider sends every figure as a display string ("1,260.00") and every date in whatever
    /// format the discom's own site used ("09 Sep 2026", "2024-11-22", "NA"). Both are kept as the
    /// provider sent them (<see cref="DueAmountText"/>, <see cref="DueDateText"/>) and also parsed
    /// into real types, so a screen can bind a number/date and a report can total it.
    /// </summary>
    public class ElectricityBillDetails
    {
        /// <summary>Registered consumer name on the connection.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Amount payable, parsed. Null when the provider sent nothing usable.</summary>
        public decimal? DueAmount { get; set; }

        /// <summary>Amount payable exactly as the provider sent it ("1,260.00").</summary>
        public string DueAmountText { get; set; } = string.Empty;

        /// <summary>Due date as yyyy-MM-dd when it could be parsed, else empty.</summary>
        public string DueDate { get; set; } = string.Empty;

        /// <summary>Due date exactly as the provider sent it ("09 Sep 2026", "NA").</summary>
        public string DueDateText { get; set; } = string.Empty;

        /// <summary>Provider's bill number. Often null on fetch — the requested number then stands in.</summary>
        public string BillNumber { get; set; } = string.Empty;

        /// <summary>Bill date as yyyy-MM-dd when it could be parsed, else empty.</summary>
        public string BillDate { get; set; } = string.Empty;

        /// <summary>Bill date exactly as the provider sent it.</summary>
        public string BillDateText { get; set; } = string.Empty;

        /// <summary>Outstanding balance, parsed. Null when the provider sent nothing usable.</summary>
        public decimal? Balance { get; set; }

        /// <summary>Outstanding balance exactly as the provider sent it.</summary>
        public string BalanceText { get; set; } = string.Empty;

        /// <summary>Billing period label ("Aug 2026"), as sent.</summary>
        public string BillPeriod { get; set; } = string.Empty;
    }

    /// <summary>
    /// Outcome of an ElectricityBillFetch call.
    ///
    /// Success is decided by the provider's STATUS field alone, never by ERROR: a wrong operator
    /// code answers ERROR "0" with STATUS "3" ("Operator Site Not Bill Fetch"), so reading ERROR
    /// would report a success carrying no bill and the screen would render empty fields.
    /// </summary>
    public class ElectricityBillFetchResult
    {
        /// <summary>True only when the provider returned STATUS "1" together with a bill.</summary>
        public bool Success { get; set; }

        /// <summary>Provider message, or the library's own when the call never reached the provider.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Provider STATUS verbatim ("1" fetched, "3" not fetched).</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Provider ERROR verbatim ("3" not authenticated, "11" request error, "14" no data).</summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>The bill. Null on any failure.</summary>
        public ElectricityBillDetails? BillDetails { get; set; }

        /// <summary>Provider ORDERID, when one is issued.</summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>Raw provider body. Server-side diagnostics only — strip it before replying to a client.</summary>
        public string Raw { get; set; } = string.Empty;

        public static ElectricityBillFetchResult Fail(string message, string raw = "")
            => new() { Success = false, Message = message, Raw = raw };

        /// <summary>
        /// Reads the provider body. The bill object arrives under the provider's own misspelling
        /// "BILLDEATILS"; "BILLDETAILS" is accepted too so a future correction upstream cannot
        /// silently empty the screen.
        /// </summary>
        public static ElectricityBillFetchResult Parse(string body, string fallbackMessage, string requestedBillNumber = "")
        {
            var result = new ElectricityBillFetchResult { Raw = body ?? string.Empty };
            JsonElement root;
            try
            {
                using var doc = JsonDocument.Parse(result.Raw);
                root = doc.RootElement.Clone();
            }
            catch
            {
                return Fail(fallbackMessage, result.Raw);
            }

            if (root.ValueKind != JsonValueKind.Object) return Fail(fallbackMessage, result.Raw);

            result.Status = Text(root, "STATUS");
            result.ErrorCode = Text(root, "ERROR");
            result.OrderId = Text(root, "ORDERID");
            result.Message = Text(root, "MESSAGE");

            var bill = Property(root, "BILLDEATILS", "BILLDETAILS");
            if (bill is JsonElement node && node.ValueKind == JsonValueKind.Object)
            {
                string billNumber = Text(node, "BillNumber");
                result.BillDetails = new ElectricityBillDetails
                {
                    Name = Text(node, "Name"),
                    DueAmountText = Text(node, "DueAmount"),
                    DueAmount = Amount(Text(node, "DueAmount")),
                    DueDateText = Text(node, "DueDate"),
                    DueDate = Date(Text(node, "DueDate")),
                    BillNumber = billNumber.Length > 0 ? billNumber : (requestedBillNumber ?? string.Empty).Trim(),
                    BillDateText = Text(node, "BillDate"),
                    BillDate = Date(Text(node, "BillDate")),
                    BalanceText = Text(node, "Balance"),
                    Balance = Amount(Text(node, "Balance")),
                    BillPeriod = Text(node, "BillPeriod"),
                };
            }

            result.Success = result.Status == "1" && result.BillDetails != null;
            if (result.Message.Length == 0) result.Message = fallbackMessage;
            if (!result.Success) result.BillDetails = null;
            return result;
        }

        private static JsonElement? Property(JsonElement parent, params string[] names)
        {
            foreach (string name in names)
                if (parent.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null)
                    return value;
            return null;
        }

        // Provider fields are strings, but a number sometimes arrives unquoted.
        private static string Text(JsonElement parent, string name)
        {
            if (!parent.TryGetProperty(name, out var value)) return string.Empty;
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.Number => value.GetRawText(),
                JsonValueKind.True or JsonValueKind.False => value.GetRawText(),
                _ => string.Empty,
            };
        }

        /// <summary>
        /// "1,260.00" / "Rs. 1,260" / "1,26,000.00" to 1260.00 / 1260 / 126000.00. Group separators
        /// are stripped rather than parsed, so Indian lakh grouping works as well as thousands
        /// grouping. "NA" and "" yield null, never 0 — a screen must not show a real bill as paid.
        /// </summary>
        internal static decimal? Amount(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var digits = new string(value.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
            if (digits.Length == 0) return null;
            return decimal.TryParse(digits, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal parsed)
                ? parsed
                : null;
        }

        // Formats seen across discoms; "NA" and anything unrecognised yield empty rather than today.
        private static readonly string[] DateFormats =
        {
            "yyyy-MM-dd", "dd MMM yyyy", "dd-MMM-yyyy", "dd/MM/yyyy", "dd-MM-yyyy",
            "yyyy/MM/dd", "MM/dd/yyyy", "dd MMMM yyyy", "yyyy-MM-ddTHH:mm:ss",
        };

        internal static string Date(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            string trimmed = value.Trim();
            return DateTime.TryParseExact(trimmed, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact)
                ? exact.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : string.Empty;
        }
    }

    /// <summary>One district / discom code as the provider publishes it.</summary>
    public class ElectricityDistrict
    {
        public int Id { get; set; }

        /// <summary>The provider's circle id this district belongs to.</summary>
        public int StateId { get; set; }

        /// <summary>The value to send as Optional1 on a bill fetch ("Agra-DVVNL").</summary>
        public string DistrictCode { get; set; } = string.Empty;
    }

    /// <summary>Outcome of an ElectricityDistrictCode call.</summary>
    public class ElectricityDistrictResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IReadOnlyList<ElectricityDistrict> Districts { get; set; } = Array.Empty<ElectricityDistrict>();
        public string Raw { get; set; } = string.Empty;

        public static ElectricityDistrictResult Fail(string message, string raw = "")
            => new() { Success = false, Message = message, Raw = raw };

        /// <summary>
        /// Reads the provider's district array. A provider error comes back as an object rather
        /// than an array, which is reported as a failure instead of an empty dropdown.
        /// </summary>
        public static ElectricityDistrictResult Parse(string body, string fallbackMessage)
        {
            string raw = body ?? string.Empty;
            List<ElectricityDistrict>? parsed;
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) return Fail(fallbackMessage, raw);
                parsed = JsonSerializer.Deserialize<List<ElectricityDistrict>>(raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return Fail(fallbackMessage, raw);
            }

            return new ElectricityDistrictResult
            {
                Success = true,
                Districts = parsed ?? new List<ElectricityDistrict>(),
                Raw = raw,
            };
        }
    }
}
