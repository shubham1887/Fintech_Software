using System.Text.RegularExpressions;

namespace FintechApp.StaticModels
{
    /// <summary>
    /// Central, deny-by-default API access policy keyed by controller (and, where a controller
    /// mixes duties, by action). A user-type may only reach an endpoint that its role is granted.
    /// Anything not explicitly opened to lower roles is Admin/Support only.
    ///
    /// The static map below is the SECURE DEFAULT. A per-deployment override (appsettings
    /// "ApiAccess:Overrides") is layered on top by ApiAccessPolicyService, so each reseller can
    /// tune access without recompiling — dynamic + white-label friendly.
    /// </summary>
    public static class ApiAccessPolicy
    {
        // Role sets (ids are the DB-authoritative UserTypeCode values).
        public static readonly int[] Admin = { UserTypeCode.Admin, UserTypeCode.SupportUser };
        // Every authenticated, non-deactivated identity (shared infra / reference reads).
        public static readonly int[] AllAuth =
        {
            UserTypeCode.Admin, UserTypeCode.SuperDistributor, UserTypeCode.Distributor,
            UserTypeCode.Retailer, UserTypeCode.ApiUser, UserTypeCode.SupportUser,
            UserTypeCode.Client, UserTypeCode.Accountant, UserTypeCode.SDFOS, UserTypeCode.DFOS,
        };
        // Transacting roles (retailer + up) — recharge / DMT / AEPS / payout / own data.
        public static readonly int[] Txn =
        {
            UserTypeCode.Admin, UserTypeCode.SuperDistributor, UserTypeCode.Distributor,
            UserTypeCode.Retailer, UserTypeCode.ApiUser, UserTypeCode.SupportUser,
            UserTypeCode.Client, UserTypeCode.SDFOS, UserTypeCode.DFOS,
        };
        // Distributor tier (fund transfer to downline, onboarding).
        public static readonly int[] Dist =
        {
            UserTypeCode.Admin, UserTypeCode.SuperDistributor, UserTypeCode.Distributor,
            UserTypeCode.SupportUser, UserTypeCode.SDFOS, UserTypeCode.DFOS,
        };
        // API-user self-service (own API token, IP security, callback URLs) — API users only.
        // (Admin still passes via the superadmin safety net in ApiAccessPolicyService.IsAllowed,
        // but the token/IP service methods reject non-API-user callers and the UI hides the page.)
        public static readonly int[] ApiSelf =
        {
            UserTypeCode.ApiUser,
        };

        // Action name fragments that MUTATE state. Writes are Admin-only on reference-master
        // controllers even when lower roles may read them.
        private static readonly Regex WriteVerb = new(
            @"^(Create|Update|Delete|Insert|Add|Remove|Save|Edit|Bulk|Approve|Reject|Activate|Deactivate|Assign|Upload|Import|Reset|Set|Change|Enable|Disable|Hold|Release|Map|Unmap)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Controllers whose granted roles may also WRITE (self-service: transactions, own KYC/bank,
        // raise request/complaint/ticket). Keys are lowercase controller names (no "Controller").
        private static readonly HashSet<string> SelfServiceWrite = new(StringComparer.OrdinalIgnoreCase)
        {
            "transactionprocessing","aeps","microatm","hitachiyesbankaeps","yesbankdmt",
            "dmtsender","dmtbeneficiary","sender","beneficiary","senderbeneficiary","senderotp",
            "beneficiaryotp","puredmttransferbenedetail","upisender","upibeneficiary",
            "payoutbeneficiary","payoutbeneficiaryotp","wallettowallettransfer","paymentgatewaytransaction",
            "utility","check","prepaidactivation","cashcollection","complaintmaster","userbankdetails",
            "kyc","ekyc","onboardinguser","onboardinguserdocument","ticketmaster","callmerequest",
            "enquirydetails","fund","otp","fundrequestapi","subscriptionplan","subscription",
            "callbackmaster","verification",
        };

        // Controllers accessible to lower roles. Unlisted controller ⇒ Admin/Support only.
        // Reads always allowed for the listed roles; writes only if the controller is SelfServiceWrite.
        private static readonly Dictionary<string, int[]> ControllerRoles = Build();

        private static Dictionary<string, int[]> Build()
        {
            var d = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
            void Set(int[] roles, params string[] controllers)
            {
                foreach (var c in controllers) d[c] = roles;
            }

            // Shared infrastructure — any authenticated user.
            Set(AllAuth, "sidebarmenu", "appbar", "dashboard", "todaysummary", "dateformat",
                "versionhistory", "inbox", "state", "zonemaster", "locationlookup", "company",
                "news", "notification", "slidermaster", "publicslidermaster", "usertype",
                "mediamaster", "externalmedia", "richtextmedia", "uisetting", "androidmenumaster",
                "androidsectionmaster", "userwiselowbalance", "trustabledevice", "settingmaster");

            // Reference masters lower roles READ (to render service grids, bill fields, plans).
            // mode/submode: read-only for the Fund Request/Transfer Mode+SubMode dropdowns; writes
            // stay Admin-only (not in SelfServiceWrite), so retailers can pick but not manage them.
            Set(Txn, "service", "operator", "bbpsconfiguration", "denominationmaster", "mode", "submode",
                "operatorsequence", "wallet", "userwalletmaster", "paymentmodes", "paymentmodemaster",
                "planapi", "roffer", "publicapi", "category", "subcategory", "channelcategory",
                "consumertypemaster", "circleapiuser", "npcibank", "paysprintbank", "dmtbank",
                "tax", "userservicecharge", "userservicemapping", "servicesubscriptionplanmaster",
                "servicesubscriptioncreateplanmaster", "servicesubscriptionplandetails");

            // Self-service transactions + own data (writes allowed via SelfServiceWrite).
            Set(Txn, "transactionprocessing", "aeps", "microatm", "hitachiyesbankaeps", "yesbankdmt",
                "dmtsender", "dmtbeneficiary", "sender", "beneficiary", "senderbeneficiary", "senderotp",
                "beneficiaryotp", "puredmttransferbenedetail", "upisender", "upibeneficiary",
                "payoutbeneficiary", "payoutbeneficiaryotp", "wallettowallettransfer",
                "paymentgatewaytransaction", "utility", "check", "prepaidactivation", "cashcollection",
                "complaintmaster", "userbankdetails", "kyc", "ekyc", "ticketmaster", "callmerequest",
                "enquirydetails", "otp", "subscriptionplan", "subscription", "fundrequestapi", "verification");

            // Own reports (the report SERVICES must scope non-admin to the caller's userId — data authz).
            Set(Txn, "transactionreport", "transactionwisereport", "ledger", "useraccounting",
                "providerrequestresponselog",
                "daybookreport", "loginhistory", "userplanutilization", "userwiseplanview",
                // Read-only report of the charges levied on the caller's own network.
                "servicechargededuction");

            // Distributor-tier: fund transfer + downline onboarding/management.
            Set(Dist, "onboardinguser", "onboardinguserdocument");

            // Fund is mixed (request-raise vs transfer/accept) — granted to Txn here; action-level
            // rules in Resolve keep transfer/accept for the distributor tier.
            Set(Txn, "fund");

            // API-user self-service: own callback URLs (read + write via SelfServiceWrite).
            Set(ApiSelf, "callbackmaster");

            // Image upload (payment slip on a Fund Request, etc.) — transacting tier and up. The
            // action is "Image" (not a write-verb), so this grants upload to retailers/distributors
            // without opening reference-master writes.
            Set(Txn, "upload");

            return d;
        }

        // Action-level exceptions for mixed controllers. Key "controller.action" (lowercase).
        private static readonly Dictionary<string, int[]> ActionRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            // Fund: raising requests is retailer-and-up; moving money down is distributor-tier.
            ["fund.requestinsert"] = Txn,
            ["fund.reverserequest"] = Txn,
            ["fund.selectlast10request"] = Txn,
            ["fund.selectlast10byreceiverid"] = Txn,
            ["fund.selectrequestbyfilter"] = Txn,
            ["fund.transfer"] = Dist,
            ["fund.reverse"] = Dist,
            ["fund.acceptreject"] = Dist,
            ["fund.generatefundreverseotp"] = Dist,
            ["fund.reversewithotp"] = Dist,

            // Users: self-service reads/profile for everyone; management stays Admin/Dist.
            ["users.selectprofile"] = AllAuth,
            // Self-edit of the caller's own contact / address details. Safe for every role: the
            // endpoint takes no target id, so it can only ever write the session's own record.
            ["users.updateprofile"] = AllAuth,
            ["users.updatepassword"] = AllAuth,
            ["users.select"] = Txn,
            ["users.selectforddl"] = Txn,
            ["users.selectuserfordropdown"] = Txn,
            ["users.selectdownlinfordropdown"] = Dist,
            ["users.selectdownlinefordropdown"] = Dist,
            ["users.selectparentwithadmin"] = Txn,
            ["users.fetchdetailsforpincode"] = Txn,
            ["users.updatefcmtoken"] = AllAuth,
            // Network reports: a user may always read their OWN downline / onboarding / new-user
            // lists. The analytics service scopes the rows to the caller's branch.
            ["users.selectdownlinereport"] = Txn,
            ["users.selectdatewise"] = Dist,
            ["users.selectuseronbordinglist"] = Dist,

            // Add User shows the auto-generated User Code the moment a role is picked, so every
            // role that may create a user must be able to read the preview. Configuring the
            // formats (UserCode/GetAll, Save, Delete) stays Admin-only by deny-by-default.
            ["usercode.preview"] = Dist,

            // Commission Structure: every transacting role must be able to see the margin their
            // OWN assigned slab pays them. SlabDetail's CRUD actions stay Admin-only (the
            // controller is unlisted, so deny-by-default still applies to them).
            ["slabdetail.commissionstructure"] = Txn,

            // API-user self-service: own API token + IP security (OTP-gated in the service).
            ["users.selectapitoken"] = ApiSelf,
            ["users.sendapitokenotp"] = ApiSelf,
            ["users.tokengeneration"] = ApiSelf,
            ["users.selectissecureip"] = ApiSelf,
            ["users.sendsecureipotp"] = ApiSelf,
            ["users.updateissecureip"] = ApiSelf,
        };

        /// <summary>Resolve the allowed role ids for a controller/action from the static defaults.</summary>
        public static int[] Resolve(string controller, string action)
        {
            var c = Normalize(controller);
            if (ActionRoles.TryGetValue($"{c}.{action.ToLowerInvariant()}", out var ar)) return ar;

            if (!ControllerRoles.TryGetValue(c, out var roles)) return Admin; // deny-by-default
            // Reference masters: lower roles read only; writes are Admin-only.
            if (WriteVerb.IsMatch(action) && !SelfServiceWrite.Contains(c)) return Admin;
            return roles;
        }

        public static string Normalize(string controller) =>
            controller.Replace("Controller", string.Empty).ToLowerInvariant();
    }
}
