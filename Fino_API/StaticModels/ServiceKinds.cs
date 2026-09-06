using FintechApp.Entities;

namespace FintechApp.StaticModels
{
    /// <summary>
    /// Groups DB-driven services (ServiceMaster rows are created by the admin, so their ids are
    /// not fixed) into functional kinds by keyword-matching Name + ServiceCategory. Used by the
    /// sidebar menu (hide a report when all its services are off) and by the filter reports
    /// (scope the Recharge/DMT/BBPS/Payout report to its own transactions).
    /// </summary>
    public static class ServiceKinds
    {
        public const string Recharge = "RECHARGE";
        public const string DMT = "DMT";
        public const string BBPS = "BBPS";
        public const string Payout = "PAYOUT";
        public const string AEPS = "AEPS";
        public const string MicroATM = "MATM";
        public const string CashDeposit = "CASHDEPOSIT";

        private static readonly Dictionary<string, string[]> Keywords = new()
        {
            [Recharge] = new[] { "recharge", "prepaid", "postpaid", "dth", "mobile" },
            [DMT] = new[] { "dmt", "money transfer", "moneytransfer" },
            [BBPS] = new[] { "bbps", "bill" },
            [Payout] = new[] { "payout", "pay out", "transfer to bank" },
            [AEPS] = new[] { "aeps" },
            [MicroATM] = new[] { "matm", "micro atm", "microatm" },
            [CashDeposit] = new[] { "cash deposit", "cashdeposit" },
        };

        /// <summary>Value of ReportKind meaning "keep this service out of every report".</summary>
        public const string None = "NONE";

        /// <summary>All assignable kinds, for validation of ServiceMaster.ReportKind.</summary>
        public static readonly string[] All = { Recharge, DMT, BBPS, Payout, AEPS, MicroATM, CashDeposit, None };

        public static bool Matches(ServiceMaster svc, string kind)
        {
            // Explicit admin mapping wins (Service Catalog "Report" dropdown). NONE matches nothing.
            string mapped = (svc.ReportKind ?? string.Empty).Trim();
            if (mapped.Length > 0)
                return mapped.Equals(kind, StringComparison.OrdinalIgnoreCase);

            // No mapping set → auto-detect from name/category keywords.
            if (kind == BBPS && svc.IsBBPS == true) return true;
            if (!Keywords.TryGetValue(kind, out var keys)) return false;
            string text = $"{svc.Name} {svc.ServiceCategory}".ToLowerInvariant();
            return keys.Any(text.Contains);
        }

        /// <summary>
        /// The kind a single service belongs to. Used where a caller has a transaction and needs
        /// the service type string the provider pipeline expects (status check / resend).
        /// Falls back to <see cref="Recharge"/> — the recharge pipeline is the generic one.
        /// </summary>
        public static string KindOf(ServiceMaster? svc)
        {
            if (svc == null) return Recharge;
            foreach (string kind in All)
            {
                if (kind == None) continue;
                if (Matches(svc, kind)) return kind;
            }
            return Recharge;
        }

        /// <summary>ServiceIDs of all services belonging to a kind (active or not — caller filters).</summary>
        public static List<int> IdsOf(IEnumerable<ServiceMaster> services, string kind)
            => services.Where(s => Matches(s, kind)).Select(s => s.ServiceID).ToList();

        /// <summary>True when at least one ACTIVE service of this kind exists.</summary>
        public static bool AnyActive(IEnumerable<ServiceMaster> services, string kind)
            => services.Any(s => s.IsActive && !s.IsDelete && Matches(s, kind));
    }
}
