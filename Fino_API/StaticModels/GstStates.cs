namespace FintechApp.StaticModels
{
    /// <summary>
    /// Indian state codes as GST uses them — the first two digits of a GSTIN.
    ///
    /// The returns need this to put a readable state beside a place-of-supply code: GSTR-1's B2CS
    /// table and GSTR-3B's table 3.2 are both reported BY place of supply, and a screen showing
    /// "27" where it means Maharashtra cannot be checked by the person filing it.
    ///
    /// The UI keeps its own copy in lib/gstStates.ts for the pickers. Both lists exist because the
    /// two sides use them for different jobs and neither should have to call the other to render a
    /// label; they agree on the codes, which are set by statute and do not drift.
    ///
    /// 25 (the old Daman &amp; Diu) is absent on purpose: it merged into 26 in 2020 and no new
    /// registration can be issued under it, though an old document may still carry it — hence
    /// <see cref="Name"/> answering with the code itself rather than throwing.
    /// </summary>
    public static class GstStates
    {
        private static readonly Dictionary<string, string> ByCode = new()
        {
            ["01"] = "Jammu & Kashmir",
            ["02"] = "Himachal Pradesh",
            ["03"] = "Punjab",
            ["04"] = "Chandigarh",
            ["05"] = "Uttarakhand",
            ["06"] = "Haryana",
            ["07"] = "Delhi",
            ["08"] = "Rajasthan",
            ["09"] = "Uttar Pradesh",
            ["10"] = "Bihar",
            ["11"] = "Sikkim",
            ["12"] = "Arunachal Pradesh",
            ["13"] = "Nagaland",
            ["14"] = "Manipur",
            ["15"] = "Mizoram",
            ["16"] = "Tripura",
            ["17"] = "Meghalaya",
            ["18"] = "Assam",
            ["19"] = "West Bengal",
            ["20"] = "Jharkhand",
            ["21"] = "Odisha",
            ["22"] = "Chhattisgarh",
            ["23"] = "Madhya Pradesh",
            ["24"] = "Gujarat",
            ["25"] = "Daman & Diu (merged into 26)",
            ["26"] = "Dadra & Nagar Haveli and Daman & Diu",
            ["27"] = "Maharashtra",
            ["29"] = "Karnataka",
            ["30"] = "Goa",
            ["31"] = "Lakshadweep",
            ["32"] = "Kerala",
            ["33"] = "Tamil Nadu",
            ["34"] = "Puducherry",
            ["35"] = "Andaman & Nicobar Islands",
            ["36"] = "Telangana",
            ["37"] = "Andhra Pradesh",
            ["38"] = "Ladakh",
            ["97"] = "Other Territory",
            ["99"] = "Centre Jurisdiction",
        };

        /// <summary>The state's name, or the code itself when it is one we do not know — a return
        /// must still render a row for an unexpected code rather than lose the supply.</summary>
        public static string Name(string? code)
        {
            string c = (code ?? string.Empty).Trim();
            if (c.Length == 0) return "(not set)";
            return ByCode.TryGetValue(c, out var n) ? n : c;
        }

        /// <summary>
        /// The GST code for a state NAME, or empty when the name is not one we know.
        ///
        /// This is the join between the application's own StateMasters table — seeded
        /// alphabetically, so its ids mean nothing to GST — and the statutory codes. Matching is
        /// deliberately forgiving about the things the two lists spell differently: "&amp;" against
        /// "and", punctuation, case and spacing. "Orissa" and "Pondicherry" are included because
        /// an older seed may still carry the pre-rename names.
        /// </summary>
        public static string CodeForName(string? stateName)
        {
            string key = Fold(stateName);
            if (key.Length == 0) return string.Empty;

            foreach (var (code, name) in ByCode)
                if (Fold(name) == key) return code;

            return Aliases.TryGetValue(key, out var aliased) ? aliased : string.Empty;
        }

        /// <summary>Names the statutory list and the app's own table spell differently.</summary>
        private static readonly Dictionary<string, string> Aliases = new()
        {
            [Fold("Orissa")] = "21",
            [Fold("Pondicherry")] = "34",
            [Fold("Uttaranchal")] = "05",
            [Fold("Jammu and Kashmir")] = "01",
            [Fold("Andaman and Nicobar")] = "35",
            [Fold("Andaman and Nicobar Islands")] = "35",
            [Fold("Dadra and Nagar Haveli")] = "26",
            [Fold("Daman and Diu")] = "26",
            [Fold("Delhi NCR")] = "07",
            [Fold("New Delhi")] = "07",
            [Fold("Telangana State")] = "36",
        };

        /// <summary>Lower-cased letters and digits only, with "&amp;" read as "and" — so
        /// "Jammu &amp; Kashmir" and "Jammu and Kashmir" fold to the same key.</summary>
        private static string Fold(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var sb = new System.Text.StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (c == '&') sb.Append("and");
                else if (char.IsLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Whether this is a state code the portal will accept as a place of supply.
        ///
        /// Worth checking before a return is filed rather than after: a document whose party has
        /// no state on record carries an empty or "00" place of supply, which looks like a number
        /// on a screen and is rejected outright by the portal. The supply itself is real, so the
        /// return still reports it — it just says loudly that it cannot be filed as it stands.
        /// </summary>
        public static bool IsKnown(string? code) =>
            !string.IsNullOrWhiteSpace(code) && ByCode.ContainsKey(code.Trim());

        /// <summary>"27 — Maharashtra", for a column that has room for both.</summary>
        public static string Label(string? code)
        {
            string c = (code ?? string.Empty).Trim();
            return c.Length == 0 ? "(not set)" : $"{c} — {Name(c)}";
        }
    }
}
