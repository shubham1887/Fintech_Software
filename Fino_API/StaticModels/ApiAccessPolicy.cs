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
        // The accounting desk: admin/support plus the Accountant. Read-only by construction —
        // none of the controllers granted this set is in SelfServiceWrite, so WriteVerb still
        // sends Create/Update/Delete to Admin. An accountant may look at the books, not move money.
        public static readonly int[] Accounting =
        {
            UserTypeCode.Admin, UserTypeCode.SupportUser, UserTypeCode.Accountant,
        };
        // Narrower than Admin, and the only set in this file that is. Everywhere else Support is
        // admin-equivalent, because everywhere else a mistake can be undone. These acts cannot:
        // an allocated invoice number has gone into GSTR-1 and can only be credit-noted, and a
        // superseded tax rule changes what every later transaction is stamped with. When an
        // auditor asks who did it, the answer should be a very short list.
        public static readonly int[] TaxDocumentWrite = { UserTypeCode.Admin };
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
        // Reports the accounting desk shares with the transacting roles. The transacting roles
        // still see only their own rows (the services scope a non-admin caller); the accountant
        // reaches the same screens because the Administration menu now lists them for role 9.
        public static readonly int[] TxnAndAccounting = [.. Txn, UserTypeCode.Accountant];
        // Distributor tier (fund transfer to downline, onboarding).
        public static readonly int[] Dist =
        {
            UserTypeCode.Admin, UserTypeCode.SuperDistributor, UserTypeCode.Distributor,
            UserTypeCode.SupportUser, UserTypeCode.SDFOS, UserTypeCode.DFOS,
        };
        // The distributor tier plus the accounting desk. Reads that are about money owed BETWEEN
        // businesses in the network: the tier that extended the credit needs them to collect, and
        // the accountant needs them because an unrecovered receivable is a figure in the books.
        public static readonly int[] DistAndAccounting = [.. Dist, UserTypeCode.Accountant];
        // Roles that may add a second login to their own account (LinkedProfilePolicy).
        // Reading the list is open to everyone, because the account switcher has to know
        // whether to appear — including for the linked account itself, which needs the
        // switcher to get back to the primary.
        public static readonly int[] LinkedProfileSetup =
        {
            UserTypeCode.SuperDistributor, UserTypeCode.Distributor, UserTypeCode.ApiUser,
        };
        // API-user self-service (own API token, IP security, callback URLs) — API users only.
        // (Admin still passes via the superadmin safety net in ApiAccessPolicyService.IsAllowed,
        // but the token/IP service methods reject non-API-user callers and the UI hides the page.)
        public static readonly int[] ApiSelf =
        {
            UserTypeCode.ApiUser,
        };
        // Who may read the API-user request/response and callback logs: the API user (its own
        // rows) plus admin/support (any row), for answering an integrator's ticket.
        public static readonly int[] ApiUserLogs =
        {
            UserTypeCode.Admin, UserTypeCode.SupportUser, UserTypeCode.ApiUser,
        };
        // The retailer alone. "Quick Services" is the counter operator's shortcut row: the one
        // role that opens the same four services all day and loses time hunting for them. Every
        // other role either does not transact (admin, accountant, support, field officers), works
        // from reports and network screens rather than a service grid (distributor tier), or has
        // no dashboard at all (an API user is a server-to-server credential). Stated as its own
        // set so the decision lives in one place instead of an exclusion at each call site.
        public static readonly int[] RetailerOnly =
        {
            UserTypeCode.Retailer,
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
            // A user editing their OWN shortcut tiles — the only id involved is the token's own.
            "quickservice",
            // Own MPIN / own password. Every authenticated action on this controller takes
            // no target id — it acts on the session's own user — so the write verbs here
            // (SetMpin, ChangePasswordAfterExpiry) are self-service, not administration.
            "credential",
            // Own authenticator enrolment. "Disable" reads as a write verb, but it acts on the
            // caller's own account and already demands their password plus a live code, so
            // forcing it to Admin would leave users able to switch two-step verification on and
            // never off. The one action that names another user, ClearForUser, is pinned to Admin
            // by an action-level rule below.
            "twofactor",
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
                "androidsectionmaster", "userwiselowbalance", "trustabledevice", "settingmaster",
                // The notification bell and the counts strip render on EVERY screen for EVERY
                // role, so both have to be reachable by every role. Leaving "appnotification"
                // unlisted made deny-by-default silently admin-only, which is why a retailer's
                // bell was always empty and why RegisterDevice never stored a token — with no
                // token there is no address to push to, so Android alerts could not arrive at
                // all. Composing is narrowed again by action below.
                "appnotification", "workqueue",
                // Own MPIN status / set / verify, own logout, own password change after expiry.
                // Unlisted, this controller fell to the Admin-only default, so a retailer or an
                // API user got 403 on the MPIN gate that runs before every screen, could not
                // change an expired password and could not log out server-side. Nothing here
                // names another user, so every role may reach it.
                "credential",
                // Ticket categories are the dropdown on the "Raise a ticket" form. Read is for
                // everyone who may raise one (i.e. everyone); Create/Update/Delete still match
                // WriteVerb and stay Admin-only.
                "ticketcategory");

            // Reference masters lower roles READ (to render service grids, bill fields, plans).
            // mode/submode: read-only for the Fund Request/Transfer Mode+SubMode dropdowns; writes
            // stay Admin-only (not in SelfServiceWrite), so retailers can pick but not manage them.
            Set(Txn, "bbpsconfiguration", "denominationmaster", "mode", "submode",
                "operatorsequence", "userwalletmaster", "paymentmodes", "paymentmodemaster",
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
            // NOTE: "providerrequestresponselog" is deliberately NOT here. It holds the upstream
            // exchange (provider endpoint, vendor id, our opening/closing balance and margin), so
            // it is Admin/Support only. An API user reads its own API traffic through
            // "apiuserinitialrequestresponselog" / "apiusercallbackrequestresponselog" instead.
            Set(Txn, "transactionwisereport",
                // Account Statement. Leaving this unlisted made deny-by-default silently
                // admin-only, so every retailer and distributor opening their own statement got
                // "no permission" and read it as the report being broken. The service already
                // scopes a non-admin caller to their OWN rows (TransactionDetailsService.
                // SelectForFilter), and SelectAdminAccounting is pinned to the accounting desk by
                // action below — never to the transacting roles.
                "transactiondetails",
                "loginhistory", "userplanutilization", "userwiseplanview",
                // Read-only report of the charges levied on the caller's own network.
                "servicechargededuction");

            // The same reports plus the accounting desk. These four carry the Administration
            // menu's accounting rows: Accounting Summary (useraccounting.UserwiseAccounting),
            // Platform Ledger (ledger.AdminLedger / AdminReconciliation), Provider Daybook
            // (daybookreport.ProviderDaybook) and Provider Sales / Purchase Report
            // (transactionreport.ProviderSale / .PurchaseReport).
            Set(TxnAndAccounting, "transactionreport", "ledger", "useraccounting", "daybookreport",
                // The filter dropdowns those accounting screens render with: the wallet picker on
                // Accounting Summary, the service and operator pickers on Provider Daybook. Plain
                // reference names, no credentials in any of the three responses, and writes still
                // fall to Admin via WriteVerb. Left in the Txn set alone, the screens opened and
                // every dropdown came back empty.
                "service", "operator", "wallet");

            // Accounting desk only (admin/support + accountant), never the transacting roles.
            // "providerpurchase" carries the Purchase Report rows; its Create/Update/Delete still
            // fall to Admin because WriteVerb matches and the controller is not in
            // SelfServiceWrite, so an accountant reads purchase entries but cannot add one yet.
            // NOTE: "providermaster" is deliberately NOT here. ProviderMasterResponse carries the
            // upstream LoginID and Token, and hiding a column in the UI does not remove it from
            // the JSON — the provider dropdown on the daybook/purchase screens stays empty for an
            // accountant until there is a names-only endpoint to grant instead.
            Set(Accounting, "invoice", "invoicesetting", "invoicenumberhistory", "providerpurchase");

            // ── GST engine and documents (Phases 1 and 2) ───────────────────────────────
            // Granted to the accounting desk for READING. The actions that change a rule, raise a
            // document or drive the IRP queue are pinned to Admin below, one by one - because the
            // WriteVerb regex does not match "Generate", "Issue", "Supersede" or "Process", so
            // relying on it here would have handed an accountant the ability to issue invoices.
            Set(Accounting,
                "taxengine",            // the simulator - read-only by construction
                "taxrule",              // rule list, detail, audit trail
                "taxmaster",            // rates, SAC codes, TDS sections
                "partytaxprofile",      // a party's tax identity and relationships
                "taxinvoice",           // document list and detail
                "creditnote",           // window alerts
                "einvoice",             // IRP status and deadline alerts
                "exchangeregister");    // the barter register

            // ── GST Phase 3: purchases, 2B, reconciliation, ITC register ────────────────
            // Reads for the whole accounting desk. Every WRITE here is pinned below, because the
            // WriteVerb regex does not match Capture / Commit / Build / Mark / Claim / Reverse, so
            // without an explicit pin they would fall through to the controller grant and hand an
            // accountant the ability to capture a purchase or claim a credit.
            Set(Accounting,
                "purchase",             // the purchase register + capture + e-invoice import
                "gstr2b",               // 2B snapshots and their lines
                "purchaserecon",        // the 4-bucket reconciliation + three-way
                "itc");                 // the ITC register and its deadline alerts

            // ── GST Phase 4: the double-entry ledger ────────────────────────────────────
            // Reads for the desk. Every WRITE is pinned below — Enqueue / ProcessOutbox / PostJournal
            // / Reverse / Lock / Unlock are none of them matched by the WriteVerb regex, so without a
            // pin an accountant could post to the books or reopen a closed period.
            Set(Accounting,
                "acctaccount",          // the chart of accounts (read-only over the API)
                "acctledger",           // vouchers, outbox, and the posting acts
                "acctperiod",           // period locks
                "acctintegrity");       // trial balance + the two tie-outs

            // ── GST Phase 5: bank statements and the bank reconciliation ────────────────
            // Reads for the desk. Import and Save ARE matched by the WriteVerb regex, so they fall
            // to Admin on their own; Preview / Commit / Match / Unmatch are not, and the three that
            // write are pinned below. Preview is deliberately left open — it changes nothing, and
            // an accountant who cannot run the comparison cannot do the job the role exists for.
            Set(Accounting,
                "bankaccount",          // the accounts we reconcile, and their ledger code
                "bankstatement",        // statement uploads and their lines
                "bankrecon");           // the 4-bucket bank-vs-books comparison

            // ── GST Phase 6: the returns ────────────────────────────────────────────────
            // Building and exporting a return change nothing and are the accounting desk's actual
            // job, so the controller is theirs. "Save" IS matched by the WriteVerb regex and so
            // falls to Admin on its own — which is right: a saved snapshot is what GSTR-9 later
            // reconciles against, and marking a period filed freezes what was declared.
            Set(Accounting, "gstreturn");

            // The place-of-supply review. Reading it is the desk's job; Backfill and Set write the
            // party's tax identity, which every later document is stamped from, so both are pinned
            // to Admin below — neither verb is caught by the WriteVerb regex.
            Set(Accounting, "partystate");

            // How a commission is taxed. Reading the profiles and trying one out change nothing;
            // Save and Delete change how every later commission splits and whether money is
            // withheld from real people, so both are pinned to Admin below.
            Set(Accounting, "commissiontax");

            // The per-band GST preview. It writes nothing — it works a hypothetical amount through
            // the rate cards and says which layer decided — and the desk needs it to answer "why is
            // this charge 5.90 and where is the 0.90?" without being able to edit a band.
            Set(Accounting, "chargegst");

            // TDS. The brief is explicit that the admin AND the accountant set these, and it is
            // the accounting desk that knows what the rate should be - so unusually for this file,
            // the writes are granted to the desk as well. The one exception is pinned below:
            // returning withheld money to a wallet moves real money and stays with Admin.
            Set(Accounting, "tdssetting", "tdsreport");

            // The GST collection report. Read-only, and the desk's own screen.
            Set(Accounting, "gstcollection");

            // Bank reconciliation setup. Reading it is the desk's; the writes below are pinned to
            // Admin, because a format or a category rule decides how every later statement is read.
            Set(Accounting, "banksetup");

            // Distributor-tier: fund transfer + downline onboarding/management.
            Set(Dist, "onboardinguser", "onboardinguserdocument");

            // Outstanding / cash collection. "cashcollection" itself is already in the Txn set
            // above (and in SelfServiceWrite) because a retailer reads their own balance from it;
            // these two were left UNLISTED, which under deny-by-default made them silently
            // Admin-only - so a distributor opening the outstanding report, the one person who
            // actually has to chase the money, got "no permission" and read it as broken.
            //
            // Who may see WHOSE rows is not decided here: the service scopes a non-admin caller to
            // their own downline and pins a statement to their own edge of the network, from the
            // signed token. This grant decides reachability only.
            Set(Dist, "cashcollectionreport", "cashcollectiondetail");

            // Fund is mixed (request-raise vs transfer/accept) — granted to Txn here; action-level
            // rules in Resolve keep transfer/accept for the distributor tier.
            Set(Txn, "fund");

            // API-user self-service: own callback URLs (read + write via SelfServiceWrite).
            Set(ApiSelf, "callbackmaster");

            // An API user's own API traffic: the request it sent us with our reply, and the
            // callbacks we pushed back. Reads are scoped to the caller's own rows in the service,
            // so an API user sees only its own payloads; support is listed explicitly because the
            // superadmin safety net covers Admin alone.
            Set(ApiUserLogs, "apiuserinitialrequestresponselog", "apiusercallbackrequestresponselog");

            // The header's live summary (queue counts + own wallet balances). Every authenticated
            // role, because this draws the chrome on every screen — and it takes no target user,
            // so it can only ever answer with the caller's own numbers.
            Set(AllAuth, "live");

            // Own two-step verification. Every role, because an account worth protecting with an
            // authenticator is not only an admin one — a retailer holding a wallet balance is
            // exactly who gets phished. Nothing here takes a target user except ClearForUser,
            // which an action-level rule pins back to Admin.
            Set(AllAuth, "twofactor");

            // "Quick Services": the retailer's own shortcut tiles — read and written by that role
            // alone, scoped to the caller by the token and never by a payload id. Admin still
            // reaches it through the superadmin safety net in ApiAccessPolicyService.IsAllowed.
            Set(RetailerOnly, "quickservice");

            // Image upload (payment slip on a Fund Request, etc.) — transacting tier and up. The
            // action is "Image" (not a write-verb), so this grants upload to retailers/distributors
            // without opening reference-master writes.
            Set(Txn, "upload");

            return d;
        }

        // Action-level exceptions for mixed controllers. Key "controller.action" (lowercase).
        private static readonly Dictionary<string, int[]> ActionRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            // The company's own account statement stays with the house, even though every role may
            // now read its own statement from the same controller.
            // The Platform Ledger screen (/AdminLedger) calls this, not Ledger/AdminLedger, so the
            // accounting desk needs it. Still closed to the transacting roles: it reads the
            // company's own account statement, which is nobody else's business. The service
            // resolves the company account for a non-admin caller (TransactionDetailsService).
            ["transactiondetails.selectadminaccounting"] = Accounting,
            ["transactiondetails.getall"] = Admin,

            // The credit limit is the only real control on cash collection: it decides whether the
            // Credit payment mode is offered at all and it is the ceiling a credit posting refuses
            // to cross. An upline able to raise their own downline's limit would make it a
            // formality, so it is Admin's alone - narrower than the controller it lives on, which
            // every transacting role can reach for their own balance. (The endpoint also carries
            // [JwtAuthorization(Admin)]; this is the same decision in the place that is audited.)
            ["cashcollection.setlimit"] = TaxDocumentWrite,
            ["cashcollection.clearlimit"] = TaxDocumentWrite,
            // Recording and undoing a collection are self-service writes for the tier that is
            // owed the money - the service then requires the caller to BE the direct upline (or
            // admin) and to confirm with their MPIN. Stated explicitly because "Reverse" is not
            // one of the verbs WriteVerb matches, so leaving it implicit meant it was governed by
            // a regex rather than a decision.
            ["cashcollection.add"] = Txn,
            ["cashcollection.reverse"] = Txn,
            ["cashcollection.statement"] = Txn,
            // The distributor tier, who are owed the money — plus the accountant, who has to be
            // able to read it. An unrecovered receivable is a figure in the books before it is a
            // collection problem, and the Outstanding Report is where it is read; the accountant
            // already reads the whole company's ledgers, so withholding this one list would only
            // mean the receivable is the number they cannot tie out. They still cannot act on it:
            // Add, Reverse, SetLimit and SendCollectionMessage are all granted separately above.
            ["cashcollection.outstandingreport"] = DistAndAccounting,
            ["cashcollection.sendcollectionmessage"] = Dist,

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

            // Clearing somebody else's authenticator is the one way to strip the second step from
            // an account without presenting a code. Admin/Support only, regardless of the
            // controller-level grant above (the controller re-checks this too).
            ["twofactor.clearforuser"] = Admin,

            // The maintenance banner renders on every screen for every role, so reading the
            // window's state has to be reachable by all of them. The controller itself stays
            // unlisted, which keeps SelectPolicy and SavePolicy on the Admin-only default - a
            // window any role could switch on would be a way to stop the whole network trading.
            ["systemdown.status"] = AllAuth,

            // Users: self-service reads/profile for everyone; management stays Admin/Dist.
            ["users.selectprofile"] = AllAuth,
            // Self-edit of the caller's own contact / address details. Safe for every role: the
            // endpoint takes no target id, so it can only ever write the session's own record.
            ["users.updateprofile"] = AllAuth,
            ["users.updatepassword"] = AllAuth,
            ["users.select"] = Txn,
            ["users.selectforddl"] = Txn,
            // The "maps under" picker on Add User: a super-distributor placing a new retailer
            // under one of its own distributors. The service clips the list to the caller's
            // branch, so this grant cannot become a directory of other resellers' firms.
            ["users.selectforddlwithusertype"] = Dist,
            ["users.selectuserfordropdown"] = Txn,
            ["users.selectdownlinfordropdown"] = Dist,
            ["users.selectdownlinefordropdown"] = Dist,
            ["users.selectparentwithadmin"] = Txn,
            ["users.fetchdetailsforpincode"] = Txn,
            ["users.updatefcmtoken"] = AllAuth,
            // Network reports. The "Downline" report is the platform operator's view of the whole
            // tree (every tier's balances side by side) and is deliberately NOT a downline user's
            // report — a distributor reads their own network from All Users, which is already
            // scoped to their branch. Keeping it open to Txn put the same table behind two very
            // different readings of "your downline".
            ["users.selectdownlinereport"] = Admin,
            ["users.selectdatewise"] = Dist,
            ["users.selectuseronbordinglist"] = Dist,

            // Moving a user under a different parent, and acting on many users at once, re-shape
            // the commission tree — every slab, every payout and every report keys off it. The
            // platform operator owns that decision, so neither is a downline capability.
            ["users.transferusers"] = Admin,
            ["users.selecteligibleparents"] = Admin,
            ["users.bulkupdate"] = Admin,
            // Soft-delete / restore of an account, and the list of deleted ones.
            ["users.delete"] = Admin,
            ["users.restore"] = Admin,
            ["users.selectdeletedusers"] = Admin,

            // ── Onboarding by the distribution tier ────────────────────────────────────────
            // Creating and editing a user in your OWN branch. Every one of these was unlisted,
            // so deny-by-default made it Admin-only and a super-distributor or distributor
            // opening Add User got "You don't have permission to perform this action." on submit
            // — the screen, the role dropdown and the upline picker were all already wired for
            // them. The server-side scoping these grants rely on is not new either:
            //   Insert  — UserService.CreatableRoles refuses a role the caller's tier may not
            //             create, and a non-admin creator's ParentID is forced into their own
            //             branch (anything outside it is refused, not re-pointed).
            //   Update /
            //   SelectByID — both run IsWithinCallerScope first, so a downline caller can only
            //             ever name themselves or someone beneath them.
            // Insert/Update match WriteVerb, but an action-level entry is resolved before that
            // rule, which is why they have to be stated here rather than added to
            // SelfServiceWrite (that would open Activate/Deactivate/Delete as well).
            ["users.insert"] = Dist,
            ["users.update"] = Dist,
            ["users.selectbyid"] = Dist,
            // The inline "already registered" hint on the Add User form. Answers a strict
            // yes/no for the one mobile/email being typed and returns no user data at all.
            ["users.checkavailability"] = Dist,
            // Texting a user their credentials again. Scoped to the caller's branch in the
            // service, and it is the first thing an upline is asked for after onboarding.
            ["users.sendpassword"] = Dist,

            // The three pickers the Add User form renders besides the ones granted above: the
            // commission-slab dropdown, the slab already mapped to the chosen upline (shown as
            // "parent's slab" so the creator can see what they are working within), and the KYC
            // document checklist. All three are reference reads; the write actions on those
            // controllers stay Admin-only through WriteVerb.
            ["slabs.getdropdown"] = Dist,
            ["userslabmapping.getbyuserid"] = Dist,
            ["verificationdocument.getall"] = Dist,

            // Add User shows the auto-generated User Code the moment a role is picked, so every
            // role that may create a user must be able to read the preview. Configuring the
            // formats (UserCode/GetAll, Save, Delete) stays Admin-only by deny-by-default.
            ["usercode.preview"] = Dist,

            // Commission Structure: every transacting role must be able to see the margin their
            // OWN assigned slab pays them. SlabDetail's CRUD actions stay Admin-only (the
            // controller is unlisted, so deny-by-default still applies to them).
            ["slabdetail.commissionstructure"] = Txn,

            // ── Hierarchy commission-slab delegation ──────────────────────────────────────
            // The distribution tier cutting its own rate cards out of the one it was given. This
            // is the one place a downline role is granted writes on commission, so it is worth
            // being explicit about why that is safe:
            //
            //   * every action resolves the caller's id and role from the SESSION, and a slab is
            //     editable only when its OwnerUserID IS that caller — a role check alone would
            //     not be enough, and is not what the service relies on;
            //   * a figure is validated against the matching band of the caller's OWN assigned
            //     card before it is written, per tier and on the row total, so nobody can hand
            //     out more than they hold;
            //   * amount ranges, commission types and the GST decision are not accepted from the
            //     client at all — they are cloned from the parent card and re-checked;
            //   * assignment is clipped to the caller's downline AND to the roles their tier may
            //     assign to (SlabDelegationPolicy.AssignableTo).
            //
            // Granted to Dist rather than Txn deliberately: a retailer reads its rate through
            // slabdetail.commissionstructure above and has nothing to delegate.
            ["slabdelegation.myparentslab"] = Dist,
            ["slabdelegation.myslabs"] = Dist,
            ["slabdelegation.slabview"] = Dist,
            ["slabdelegation.createslab"] = Dist,
            ["slabdelegation.updaterow"] = Dist,
            ["slabdelegation.createrow"] = Dist,
            ["slabdelegation.deleterow"] = Dist,
            ["slabdelegation.deleteslab"] = Dist,
            ["slabdelegation.assignslab"] = Dist,
            ["slabdelegation.assignments"] = Dist,
            ["slabdelegation.unassignslab"] = Dist,
            ["slabdelegation.fixtoparent"] = Dist,
            ["slabdelegation.audit"] = Dist,

            // Alerts: reading the feed is everyone's (granted at controller level above), but
            // BROADCASTING is the platform operator's alone. An alert carries the operator's
            // voice — it lands in the bell and as a push on a user's phone with no "sent by"
            // framing a recipient can judge — so letting a distributor address their own branch
            // put words in the operator's mouth. Both the composer and the sent-alert ledger are
            // Admin/Support now.
            ["appnotification.send"] = Admin,
            ["appnotification.sent"] = Admin,
            // Whether push is configured on this install is a deployment fact for the people who
            // can act on it, not something to show a retailer.
            ["appnotification.pushstatus"] = Admin,

            // One mobile number, two roles. Reading the pair is everyone's — the switcher and
            // the profile card render for both accounts in it. Creating the second one is only
            // for the types LinkedProfilePolicy allows, and the service re-checks the role pair
            // as well, so this grant alone cannot produce an illegal combination.
            ["users.linkedprofiles"] = AllAuth,
            ["users.linkedprofileadd"] = LinkedProfileSetup,

            // Choosing which of your own logins to continue as. Authenticated, and the service
            // refuses any target that is not in the caller's own linked group.
            ["auth.switchprofile"] = AllAuth,
            // Ending your own session. Unlisted, the auth controller fell to the Admin-only
            // default, so every other role's sign-out failed server-side and its refresh tokens
            // stayed valid — the client cleared local storage and called it done.
            ["auth.logout"] = AllAuth,

            // API-user self-service: own API token + IP security (OTP-gated in the service).
            ["users.selectapitoken"] = ApiSelf,
            ["users.sendapitokenotp"] = ApiSelf,
            ["users.tokengeneration"] = ApiSelf,
            ["users.selectissecureip"] = ApiSelf,
            ["users.sendsecureipotp"] = ApiSelf,
            ["users.updateissecureip"] = ApiSelf,

            // ── GST: the acts the accounting desk may NOT perform ───────────────────────────
            // Every entry below is listed explicitly even where WriteVerb would already catch the
            // name (Create, Deactivate, Save). Relying on the regex here would mean the rule that
            // keeps Support away from an irrevocable act is invisible at the point it matters,
            // and one rename away from quietly not applying.
            //
            // The rest of this file treats Support as admin-equivalent. These are the exception:
            // see TaxDocumentWrite above for why.

            // A rule decides what every later transaction is stamped with. Superseding one is how
            // a rate change is made; it is also how a year of figures is made wrong.
            ["taxrule.create"] = TaxDocumentWrite,
            ["taxrule.supersede"] = TaxDocumentWrite,
            ["taxrule.deactivate"] = TaxDocumentWrite,

            // A party's GSTIN, its place of supply and its P2P-vs-AGENT relationship are the three
            // inputs that decide IGST vs CGST/SGST and whether 194H applies at all.
            ["partytaxprofile.save"] = TaxDocumentWrite,
            ["partytaxprofile.saverelationship"] = TaxDocumentWrite,

            // Raising a sale document. An invoice number, once allocated, goes into GSTR-1 and
            // cannot be withdrawn - only credit-noted. That is not a reviewer's button.
            ["taxinvoice.generatetopups"] = TaxDocumentWrite,
            ["taxinvoice.generatecommission"] = TaxDocumentWrite,
            // A whole month in one act, which is the same irreversible thing the two above do:
            // it allocates invoice numbers that go into GSTR-1 and can only be credit-noted.
            // Admin alone, like them — and NOT caught by the WriteVerb regex, which does not match
            // "Generate", so stating it here is what keeps it off the accounting desk.
            ["taxinvoice.generatemonth"] = TaxDocumentWrite,
            // Withdraws numbers a customer may already hold, to re-issue the month. Narrower than
            // narrow: Admin alone, like every other act that restates what the law was told.
            ["taxinvoice.regroupmonth"] = TaxDocumentWrite,
            // Withdrawing one document and handing its number back to the series: the same act.
            ["taxinvoice.cancel"] = TaxDocumentWrite,
            ["taxinvoice.issueselfinvoice"] = TaxDocumentWrite,

            // A credit note reduces declared output tax. WindowAlerts stays open to the desk -
            // seeing what is about to expire is exactly their job - but issuing the note is not.
            ["creditnote.issue"] = TaxDocumentWrite,

            // Raising our side of a barter. Same reasoning as the generators above.
            ["exchangeregister.invoiceoutward"] = TaxDocumentWrite,

            // Operational, and deliberately NOT narrowed to Admin alone: these are re-runnable,
            // allocate no number and decide nothing. Someone has to be able to clear the IRP
            // queue at 11pm on the 30th day, and detecting a swap leg only records what the
            // wallet movements already show.
            ["einvoice.enqueue"] = Admin,
            ["einvoice.enqueueperiod"] = Admin,
            ["einvoice.processqueue"] = Admin,
            ["exchangeregister.detectlegs"] = Admin,

            // ── Phase 3 writes the WriteVerb regex does not catch ───────────────────────
            // Operational (Admin + Support): recording a supplier bill, committing a match,
            // (re)building the register, marking a supplier paid. None allocates a number of ours
            // or changes declared tax on its own; each is idempotent or re-runnable.
            ["purchase.capture"] = Admin,
            ["purchaserecon.commit"] = Admin,
            ["itc.build"] = Admin,
            ["itc.markpaid"] = Admin,
            // Changing declared INPUT tax - the mirror of issuing a document. Admin alone, the same
            // line drawn for the sale side: taking a credit and giving one back both move the
            // return, and when an auditor asks who did it the answer should be a very short list.
            ["itc.claim"] = TaxDocumentWrite,
            ["itc.reverse"] = TaxDocumentWrite,
            // The GST Returns page's one button: store the 2B file, match it, and CLAIM the
            // credit on what matched. It ends in itc.claim, so it carries itc.claim's rule and
            // not the wider Admin + Support that "Import" would otherwise fall to.
            ["gstreturn.import2b"] = TaxDocumentWrite,

            // ── Phase 4 ledger writes the WriteVerb regex does not catch ────────────────
            // Operational (Admin + Support): recording an event to post, and working the outbox.
            ["acctledger.enqueue"] = Admin,
            ["acctledger.processoutbox"] = Admin,
            // Writing the books directly, and sealing / unsealing a filed month. Admin alone - the
            // same weight as issuing a document, and a period lock is the control that makes a filed
            // month unchangeable, so reopening one is not a reviewer's button.
            ["acctledger.postjournal"] = TaxDocumentWrite,
            ["acctledger.reverse"] = TaxDocumentWrite,
            ["acctperiod.lock"] = TaxDocumentWrite,
            ["acctperiod.unlock"] = TaxDocumentWrite,

            // ── Phase 5 bank-recon writes the WriteVerb regex does not catch ────────────
            // Committing a match marks a ledger line reconciled for every FUTURE statement too, so
            // a wrong one hides a real difference months later. Operational (Admin + Support) —
            // it allocates no number and can be undone by Unmatch, which is held to the same bar.
            ["bankrecon.commit"] = Admin,
            ["bankrecon.match"] = Admin,
            ["bankrecon.unmatch"] = Admin,

            // ── The party's place of supply ─────────────────────────────────────────────
            // It decides CGST+SGST against IGST on every document stamped afterwards, so writing
            // it is a document-weight act, not a reviewer's button.
            ["partystate.backfill"] = TaxDocumentWrite,
            ["partystate.setplaceofsupply"] = TaxDocumentWrite,
            // Cancelling real documents, even unfiled ones.
            ["partystate.cancelforregeneration"] = TaxDocumentWrite,

            // ── Commission tax treatment ────────────────────────────────────────────────
            // Save changes how every later commission splits; Delete can remove the row a whole
            // provider was being taxed under. Both carry document weight.
            ["commissiontax.save"] = TaxDocumentWrite,
            ["commissiontax.delete"] = TaxDocumentWrite,

            // ── TDS ─────────────────────────────────────────────────────────────────────
            // Writing a rule changes what every later commission loses, and the desk is meant to
            // do it - so Save and Delete are granted to Accounting rather than pinned to Admin.
            ["tdssetting.save"] = Accounting,
            ["tdssetting.delete"] = Accounting,
            // Recording a deposit is a bookkeeping fact; the desk records it.
            ["tdsreport.markdeposited"] = Accounting,
            // Giving withheld TDS back to a party credits a real wallet. Admin alone, and the
            // verb would not be caught by the WriteVerb regex on its own.
            ["tdsreport.settletowallet"] = TaxDocumentWrite,

            // ── Bank reconciliation setup ───────────────────────────────────────────────
            // A statement format and a category rule decide how money is READ on every statement
            // imported afterwards, so changing them is the operator's own act, not the desk's.
            ["banksetup.syncbanks"] = Admin,
            ["banksetup.saveformat"] = Admin,
            ["banksetup.deleteformat"] = Admin,
            ["banksetup.saverule"] = Admin,
            ["banksetup.deleterule"] = Admin,
            ["banksetup.reclassify"] = Admin,
            // Deciding what ONE line is, on the register, is the desk's own work.
            ["banksetup.setlinecategory"] = Accounting,
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
