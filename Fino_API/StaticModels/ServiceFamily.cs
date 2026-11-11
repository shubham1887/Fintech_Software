using FintechApp.Entities;

namespace FintechApp.StaticModels
{
    /// <summary>
    /// Which services share one upstream provider, for Provider Routing.
    ///
    /// Operator-wise Provider Routing used to pool providers by an exact ServiceID match, widened
    /// only by a hard-coded list of recharge names. That is why the screen worked for Prepaid and
    /// DTH and was empty for every biller: a provider is wired up once (an endpoint on "Bharat
    /// Bill Payment", or no endpoint at all when it is a coded integration), while a biller rail
    /// is thirty ServiceMaster rows — Electricity, Water, Piped Gas, FASTag, each with its own id.
    /// Matching on the id alone meant the provider was invisible on every one of them.
    ///
    /// A family is the rail a service runs on, which is also the unit a provider is bought on: an
    /// operator contracts for "BBPS" or "recharge", never for "Water Bill Payment" alone. The rail
    /// is taken from the admin's own ReportKind mapping when they have set one (Service Catalog →
    /// Report), and otherwise read off the name and category.
    ///
    /// BBPS is the catch-all, deliberately. Every biller rail that is not one of the named ones
    /// (DMT, AEPS, Micro-ATM, Payout, Cash Deposit, Payment Gateway, Verification) and not a
    /// recharge is a bill payment — Property Tax, LPG, OTT, Housing Society. Falling back to
    /// "recharge" instead, as the old code did, put those services in a pool whose providers
    /// cannot serve them.
    /// </summary>
    public static class ServiceFamily
    {
        /// <summary>Rails that are not a ServiceKinds report kind but still route on their own.</summary>
        public const string PaymentGateway = "PG";
        public const string Verification = "VERIFY";

        // Checked in this order, and the order carries the only two judgement calls here.
        //
        // "fastag" and "bbps" are decided BEFORE the recharge words, because "FASTag Recharge"
        // is a biller, not an airtime top-up, whatever it is called. "bill payment" is decided
        // AFTER them, because "Postpaid Bill Payment" is a mobile operator's own postpaid rail
        // and has always pooled with recharge — moving it would re-point a live route.
        //
        // An admin who disagrees with either sets ReportKind on the service and is obeyed.
        private static readonly (string Kind, string[] Words)[] Rails =
        {
            (ServiceKinds.DMT,         new[] { "dmt", "money transfer", "moneytransfer", "remittance" }),
            (ServiceKinds.AEPS,        new[] { "aeps", "aadhaar pay", "aadhar pay" }),
            (ServiceKinds.MicroATM,    new[] { "matm", "micro atm", "microatm" }),
            (ServiceKinds.CashDeposit, new[] { "cash deposit", "cashdeposit", "cash collection" }),
            (ServiceKinds.Payout,      new[] { "payout", "pay out", "transfer to bank" }),
            (PaymentGateway,           new[] { "payment gateway", "paymentgateway", "qr &", "qr and", "upi collect" }),
            (Verification,             new[] { "verification", "kyc", "pan verify", "penny drop" }),
            (ServiceKinds.BBPS,        new[] { "bbps", "biller", "fastag" }),
            (ServiceKinds.Recharge,    new[] { "recharge", "prepaid", "postpaid", "post paid", "dth", "mobile" }),
            (ServiceKinds.BBPS,        new[] { "bill payment", "bill pay", "bill" }),
        };

        /// <summary>
        /// The rail a service runs on. The admin's explicit ReportKind wins; NONE means the
        /// service is kept out of the reports but still has to route somewhere, so it is
        /// classified from its text like any unmapped row.
        /// </summary>
        public static string Of(ServiceMaster? svc)
        {
            if (svc == null) return ServiceKinds.BBPS;

            string mapped = (svc.ReportKind ?? string.Empty).Trim();
            if (mapped.Length > 0 && !mapped.Equals(ServiceKinds.None, StringComparison.OrdinalIgnoreCase))
                return mapped.ToUpperInvariant();

            if (svc.IsBBPS == true) return ServiceKinds.BBPS;

            string text = $"{svc.Name} {svc.ServiceCategory}".ToLowerInvariant();
            foreach (var (kind, words) in Rails)
                if (words.Any(text.Contains))
                    return kind;

            // Unclassified biller service — Property Tax, LPG, OTT, Housing Society. BBPS is the
            // rail every such biller reaches in practice.
            return ServiceKinds.BBPS;
        }

        /// <summary>
        /// Does this service route through operator-wise Provider Routing at all?
        ///
        /// Only the two operator-based rails do. DMT has its own bank/range routing screens, AEPS
        /// and Micro-ATM are single-provider rails, Payout routes per beneficiary bank, and the
        /// payment gateway and verification rails are configured on their own setting pages. All
        /// of them were listed in the Provider Routing service picker, where choosing one produced
        /// a grid that can never be saved against anything.
        /// </summary>
        public static bool IsOperatorRouted(ServiceMaster? svc)
        {
            string family = Of(svc);
            return family == ServiceKinds.Recharge || family == ServiceKinds.BBPS;
        }

        /// <summary>
        /// Every ServiceID that shares a provider pool with <paramref name="serviceID"/> —
        /// the whole rail, so one BBPS endpoint covers all thirty biller services.
        /// </summary>
        public static HashSet<int> Pool(IEnumerable<ServiceMaster> services, int serviceID)
        {
            var all = services as IList<ServiceMaster> ?? services.ToList();
            var current = all.FirstOrDefault(s => s.ServiceID == serviceID);
            if (current == null) return new HashSet<int> { serviceID };

            string family = Of(current);
            var ids = all.Where(s => Of(s) == family).Select(s => s.ServiceID).ToHashSet();
            ids.Add(serviceID);
            return ids;
        }
    }
}
