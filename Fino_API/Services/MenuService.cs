using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;
using MenuItem = FintechApp.Responses.Menu.MenuItem;

namespace FintechApp.Services
{
    // Role-based sidebar/navigation menu, served from the backend and filtered by the caller's
    // user-type. The definition mirrors the frontend menu; visibility is governed by per-node
    // user-type id lists. Children with no explicit list inherit the parent's visibility.
    // Leaves may also carry a ServiceKind gate: for non-admin roles the item is hidden while
    // no ACTIVE service of that kind exists (admin turns a service off → its report/menu
    // disappears for retailers/distributors, and comes back when it is switched on again).
    public class MenuService : IMenuService
    {
        // Internal definition node (carries the role + service gates). Never returned to the client.
        private sealed class Node
        {
            public int[]? Roles { get; init; }
            public string Name { get; init; } = string.Empty;
            public string? Url { get; init; }
            public string? Icon { get; init; }
            public Node[]? Children { get; init; }
            /// <summary>ServiceKinds constant — hide this leaf when no active service of the kind exists.</summary>
            public string? Service { get; init; }
        }

        private readonly IServiceRepository _serviceRepo;
        private readonly IWalletServiceMappingRepository _walletServiceRepo;
        private readonly IUserServiceAccessService _serviceAccess;
        private readonly IMenuVisibilityService _visibility;

        public MenuService(IServiceRepository serviceRepo, IWalletServiceMappingRepository walletServiceRepo,
            IUserServiceAccessService serviceAccess, IMenuVisibilityService visibility)
        {
            _serviceRepo = serviceRepo;
            _walletServiceRepo = walletServiceRepo;
            _serviceAccess = serviceAccess;
            _visibility = visibility;
        }

        /// <summary>
        /// The key a visibility decision is recorded against: a leaf's URL, or
        /// <c>group:&lt;name&gt;</c> for a group, which has no URL of its own.
        /// </summary>
        public static string KeyFor(string? url, string name) =>
            string.IsNullOrWhiteSpace(url)
                ? $"group:{(name ?? string.Empty).Trim().ToLowerInvariant()}"
                : url.Trim().ToLowerInvariant();

        /// <summary>One entry of the shipped menu definition, flattened.</summary>
        public readonly record struct CatalogEntry(
            string Key, string Name, string Group, string? Url, bool IsGroup, int[]? Roles);

        /// <summary>
        /// The whole shipped definition as a flat list — what the Menu Visibility screen is built
        /// from. It reports what the PRODUCT has, not what any one role sees, because that is the
        /// list an admin is choosing from; the per-role reality is applied when the menu is built.
        /// </summary>
        public static List<CatalogEntry> Catalog()
        {
            var list = new List<CatalogEntry>();
            foreach (var node in Definition) Walk(node, string.Empty, list);
            return list;

            static void Walk(Node node, string group, List<CatalogEntry> into)
            {
                bool isGroup = node.Children is { Length: > 0 };
                into.Add(new CatalogEntry(
                    KeyFor(node.Url, node.Name), node.Name, group, node.Url, isGroup, node.Roles));
                if (!isGroup) return;
                foreach (var child in node.Children!) Walk(child, node.Name, into);
            }
        }

        public async Task<List<MenuItem>> GetMenu(int userTypeId, int userId = 0)
        {
            // Admin & support keep the full menu (they manage the services themselves).
            bool serviceGated = userTypeId != UserTypeCode.Admin && userTypeId != UserTypeCode.SupportUser;
            List<ServiceMaster> services = serviceGated ? await _serviceRepo.GetAllServices() : new List<ServiceMaster>();

            // A service the admin "disables" by turning IsDisplay off must drop its report/menu leaf
            // for downline roles too. Gate on displayed services only (AnyActive already requires
            // IsActive), so a service that is inactive OR hidden no longer keeps its report visible.
            if (serviceGated)
                services = services.Where(s => s.IsDisplay).ToList();

            // A service reached through a wallet is only usable while that wallet is on. If admin
            // turns a wallet off, every service mapped exclusively to disabled wallets is treated as
            // inactive here so its report/menu leaf disappears (alongside the direct service toggle).
            if (serviceGated && services.Count > 0)
            {
                var mappings = await _walletServiceRepo.GetAll();
                var mappedServiceIds = mappings.Select(m => m.ServiceID).ToHashSet();
                var walletBackedServiceIds = mappings
                    .Where(m => m.WalletMaster != null && m.WalletMaster.IsActive)
                    .Select(m => m.ServiceID)
                    .ToHashSet();
                services = services
                    .Where(s => !mappedServiceIds.Contains(s.ServiceID) || walletBackedServiceIds.Contains(s.ServiceID))
                    .ToList();
            }

            // A service withheld from this user ("Off" on their services grid) takes its report /
            // menu leaf with it. "Lock" keeps the leaf — the service is theirs, just suspended, and
            // they still need its history. A service held back only by the KYC gate keeps its leaf
            // for the same reason. null grants = unrestricted, so nothing is dropped.
            if (serviceGated && services.Count > 0 && userId > 0)
            {
                var access = await _serviceAccess.GetContext(userId);
                if (access.Grants != null)
                    services = services
                        .Where(s => UserServiceAccessService.Resolve(access, s.ServiceID) != ServiceAccess.NotGranted)
                        .ToList();
            }

            // What this install has chosen to hide. Read once per menu build rather than per node:
            // the sidebar is requested on every page load by every signed-in user.
            var hidden = await HiddenKeys(userTypeId);

            var result = new List<MenuItem>();
            foreach (var node in Definition)
            {
                var item = Filter(node, userTypeId, serviceGated, services, hidden);
                if (item != null) result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// The keys this role must not be shown. Built by asking the visibility service about the
        /// entries the role could otherwise see — a hide decision is presentation, so it is
        /// applied after the role and service gates, never instead of them.
        /// </summary>
        private async Task<HashSet<string>> HiddenKeys(int userTypeId)
        {
            var hidden = new HashSet<string>();
            foreach (var entry in Catalog())
            {
                if (!await _visibility.IsVisible(entry.Key, userTypeId)) hidden.Add(entry.Key);
            }
            return hidden;
        }

        private static MenuItem? Filter(Node node, int userTypeId, bool serviceGated,
            List<ServiceMaster> services, HashSet<string> hidden)
        {
            bool allowed = node.Roles == null || node.Roles.Contains(userTypeId);

            // The admin switched this entry off for this install. Applied to groups and leaves
            // alike: hiding "Reports" hides what is inside it, which is what an admin means.
            if (hidden.Contains(KeyFor(node.Url, node.Name))) return null;

            if (node.Children is { Length: > 0 })
            {
                // Parent gate: if the user can't see the group, hide it entirely.
                if (!allowed) return null;

                var kids = new List<MenuItem>();
                foreach (var child in node.Children)
                {
                    var c = Filter(child, userTypeId, serviceGated, services, hidden);
                    if (c != null) kids.Add(c);
                }
                if (kids.Count == 0) return null; // no visible children → hide group

                return new MenuItem { Name = node.Name, Icon = node.Icon, Url = node.Url, Children = kids };
            }

            // Leaf
            if (!allowed) return null;
            if (serviceGated && node.Service != null && !ServiceKinds.AnyActive(services, node.Service))
                return null; // service switched off → hide its menu/report
            return new MenuItem { Name = node.Name, Url = node.Url, Icon = node.Icon };
        }

        // ── Menu definition (mirrors the frontend sidebar) ───────────────────────────────
        // Role codes: 1=Admin 2=SuperDist 3=Dist 4=Retailer 5=ApiUser 6=Support 7=Client 9=Accountant 10=SDFOS 11=DFOS
        private static readonly Node[] Definition =
        {
            new Node { Roles = [1], Name = "Dashboard", Url = "/AdminDashboard", Icon = "DashboardIcon" },
            new Node { Roles = [2], Name = "Dashboard", Url = "/SuperDistributorDashboard", Icon = "DashboardIcon" },
            new Node { Roles = [3], Name = "Dashboard", Url = "/DistributorDashboard", Icon = "DashboardIcon" },
            new Node { Roles = [4, 5], Name = "Dashboard", Url = "/RetailerDashboard", Icon = "DashboardIcon" },
            new Node { Roles = [7], Name = "Dashboard", Url = "/ClientDashboard", Icon = "DashboardIcon" },

            new Node { Roles = [1, 6], Name = "Transaction Search", Url = "/Search", Icon = "Search" },
            new Node { Roles = [4, 5], Name = "Transaction Search", Url = "/RetailerSearch", Icon = "Search" },

            // ONE row, not a group. The hub lists every queue this install actually has - recharge,
            // DMT, bill payment, payout, AEPS and whatever is added next - and opens each one in
            // the same workbench, so the three extra child rows (Recharge / DMT / Reconciliation)
            // were the same screens reached a second way. Recharge and DMT keep their own
            // capabilities (provider switching, the transfer hold) inside the hub.
            new Node { Roles = [1, 6], Name = "Pending Queue", Url = "/PendingQueues", Icon = "Pets" },

            // ── Support & Alerts ────────────────────────────────────────────────────────────
            //
            // ONE section, every role, always in the same place. Complaints and tickets used to be
            // scattered: the admin's were buried in a 17-item "Admin" group and a 30-item "Report"
            // group, support had two loose top-level rows, and a retailer had to scroll the Report
            // list to find help. Three different places for the same desk means nobody finds it —
            // the feature was there and read as missing.
            //
            // Everything that is "somebody needs answering, or the network needs telling" now
            // lives here, and each row is shown only to the roles that can act on it.
            new Node { Roles = [1, 2, 3, 4, 5, 6, 7, 9, 10, 11], Name = "Support", Icon = "SupportIcon", Children = new[]
            {
                // Admin and support WORK the queue; everyone else sees only what they raised —
                // the API scopes the rows, so one URL serves both readings.
                new Node { Roles = [1, 6], Name = "Support Tickets", Url = "/TicketReport" },
                new Node { Roles = [2, 3, 4, 5, 7, 9, 10, 11], Name = "My Tickets", Url = "/TicketReport" },
                new Node { Roles = [1, 2, 3, 4, 5, 6, 7, 9, 10, 11], Name = "Complaints", Url = "/Complaints" },
                new Node { Roles = [1, 2, 3, 4, 5, 6, 7, 9, 10, 11], Name = "Notifications", Url = "/Notifications" },
                // The admin's own copy of what went out, and the composer. These sat under
                // Setting — a place nobody looks for "tell my network something" — which is part
                // of why App Alert read as broken: it was sent from one section and read from
                // another, with nothing linking them.
                // Broadcasting to the network is the platform operator's alone. A distributor
                // addressing their own downline reads to the retailer exactly like an official
                // notice, so the two cannot be told apart - and that is the one thing an alert
                // channel has to get right. Support keeps it because support IS the operator's desk.
                new Node { Roles = [1, 6], Name = "Broadcast Alert", Url = "/PushNotifications" },
                new Node { Roles = [1, 6], Name = "News & Offers", Url = "/News/Index" },
                // Without at least one category the "Raise a ticket" form cannot be completed,
                // so the place to manage them belongs beside the queue they feed.
                new Node { Roles = [1], Name = "Ticket Categories", Url = "/TicketCategory" },
            }},

            new Node { Roles = [1, 2, 3, 4, 5, 6, 7, 10, 11], Name = "Funds", Icon = "MoneyIcon", Children = new[]
            {
                new Node { Roles = [1], Name = "Virtual Balance", Url = "/VirtualBalance" },
                new Node { Roles = [1], Name = "Payment Modes", Url = "/ModeMaster" },
                new Node { Roles = [1, 2, 3, 4, 10, 11], Name = "Fund Requests", Url = "/Fund" },
                // Outstanding sits here, next to Fund Requests, because it is the other half of the
                // same act: a fund request approved on Credit funds a wallet AND records a debt.
                //
                // Every role that can owe money or be owed it, the retailer included - they need to
                // see what they owe and the statement behind it, which is the difference between a
                // credit facility and an argument at the counter. What each of them actually SEES
                // is scoped server-side: admin and support get the network, everyone else their own
                // downline and their own balance.
                new Node { Roles = [1, 2, 3, 4, 6, 10, 11], Name = "Outstanding", Url = "/Outstanding" },
                new Node { Roles = [1, 2, 3, 10, 11], Name = "Fund Transfer", Url = "/Fund/Transfer" },
                new Node { Roles = [1, 2, 3], Name = "Fund Reversal", Url = "/FundReverse" },
                // Reverse requests raised by the distributor tier, waiting on the operator to
                // accept or reject - and, once decided, the record of what was pulled back.
                new Node { Roles = [1, 6], Name = "Reversal Requests", Url = "/FundReverseRequestReport" },
                // "Fund Reversal Report" lives under Reports, not here: it is the ledger of what was
                // pulled back (requests AND completed reversals), which is a reporting question.
                // Funds keeps only the things you DO — reverse, and work the request queue.
                new Node { Roles = [2, 3, 4, 5], Name = "Raise Fund Request", Url = "/Fund/Request" },
                new Node { Roles = [2, 3], Name = "Raise Reversal Request", Url = "/Fund/ReverseRequest" },
                // Moving AEPS earnings into a wallet they can spend from. Every role that HOLDS
                // wallets needs it, and only the distributor tier was shown it: an API user or an
                // operator with money sitting in the AEPS wallet had no screen at all to move it.
                // This is the only wallet movement offered — free wallet-to-wallet is not on the
                // menu, and the endpoint refuses any source but AEPS.
                // The retailer reaches the same screen from Services → AEPS to Wallet, so they
                // are not listed here as well.
                new Node { Roles = [1, 2, 3, 5, 7, 10, 11], Name = "AEPS to Wallet", Url = "/WalletTransfer" },
            }},

            // ── Accountant ──────────────────────────────────────────────────────────────────
            //
            // What the accounting desk works in — and only that. This section had grown to
            // twenty-five rows: every table the GST work had ever needed got a screen, and every
            // screen got a menu row. The people using it are not accountants by training; they
            // know that GST has to be filed and want to be walked through it. So the month now
            // lives on ONE page (GST Returns) — sales, purchases, the 2B file, the credit and the
            // tax to pay — and the screens that were only ever a step of that (E-Invoice Queue,
            // Credit-Note Window, GST Collection, GSTR-2B Reconciliation, ITC Register) are off
            // the menu. So are the double-entry screens (Chart of Accounts, Ledger Vouchers, Trial
            // Balance), Commission Tax and the Tax Rule Simulator: nobody on this desk reads
            // them. Their routes still exist for anyone holding a link, and the jobs behind them
            // still run; they are simply no longer something a person has to walk past.
            //
            // Both roles get this section. The accountant reads everything in it; the writes that
            // change what the law sees (issuing a document, claiming a credit, returning withheld
            // TDS to a wallet) are pinned to Admin in ApiAccessPolicy and hidden on the screens.
            // A row here with no matching grant in that policy renders and then fails silently,
            // which is the one failure mode this menu has produced before.
            new Node { Roles = [1, 9], Name = "Accountant", Icon = "AssessmentIcon", Children = new[]
            {
                // What we billed and what we were billed.
                new Node { Roles = [1, 9], Name = "Invoices", Url = "/Invoice" },
                new Node { Roles = [1, 9], Name = "GST Documents", Url = "/TaxInvoices" },
                new Node { Roles = [1, 9], Name = "Exchange Register", Url = "/ExchangeRegister" },
                new Node { Roles = [1, 9], Name = "Purchase Invoice", Url = "/PurchaseRegister" },
                new Node { Roles = [1, 9], Name = "Purchase Report", Url = "/PurchaseReport" },

                // The month: sales, purchases, the 2B file, the credit and the tax to pay.
                new Node { Roles = [1, 9], Name = "GST Returns", Url = "/GstReturns" },
                new Node { Roles = [1, 9], Name = "Bank Reconciliation", Url = "/BankRecon" },
                // The configuration behind it: the banks (pulled from Payment Modes), how each
                // one writes its statement, what a line means, and deposits claimed twice.
                new Node { Roles = [1, 9], Name = "Bank Setup", Url = "/BankSetup" },

                // What the provider side looks like, which is where most of the margin is decided.
                new Node { Roles = [1, 9], Name = "Provider Daybook", Url = "/ProviderDayBook" },
                new Node { Roles = [1, 9], Name = "Provider Sales", Url = "/ProviderSale" },

                // The state each party is billed in — it decides CGST + SGST against IGST.
                new Node { Roles = [1, 9], Name = "Place of Supply", Url = "/PartyPlaceOfSupply" },

                // TDS: the rules, and the month. Both roles set the rules — the desk is the one
                // that knows what the rate should be — and only Admin can hand withheld money back.
                new Node { Roles = [1, 9], Name = "TDS Setting", Url = "/TdsSetting" },
                new Node { Roles = [1, 9], Name = "TDS Report", Url = "/TdsMonthly" },
            }},

            // ── Administration ─────────────────────────────────────────────────────────────
            // The operator's own desk: the company's books at a glance, the logs, and the two
            // actions that change a transaction's outcome.
            new Node { Roles = [1], Name = "Administration", Icon = "MoneyIcon", Children = new[]
            {
                // Which of the product's screens this install puts on the sidebar. First in the
                // group because it is the answer to "why is there a report here we never use" —
                // and because an admin looking for it is looking for a menu setting, not a log.
                new Node { Roles = [1], Name = "Menu Visibility", Url = "/MenuVisibility" },
                // The company's own money: wallet-wise opening / closing, sale, purchase and
                // commission, and the running-balance statement behind it. These are the owner's
                // figures rather than a GST step, so they sit with the owner.
                new Node { Roles = [1], Name = "Accounting Summary", Url = "/AdminAccounting" },
                new Node { Roles = [1], Name = "Platform Ledger", Url = "/AdminLedger" },
                new Node { Roles = [1], Name = "SMS Log", Url = "/SmsReport" },
                new Node { Roles = [1], Name = "Provider Logs", Url = "/ProviderLog" },
                new Node { Roles = [1], Name = "API Partner Logs", Url = "/ApiUserLog" },
                // These two change a transaction's OUTCOME, so they stay with the admin: the
                // accountant reads the books, it does not decide whether a transfer succeeded.
                new Node { Roles = [1], Name = "Status Correction", Url = "/ChangeStatus" },
                new Node { Roles = [1], Name = "Manual Settlement", Url = "/ManualSuccessFailReport" },
                new Node { Roles = [1], Name = "User Onboarding Report", Url = "/UserOnboardedReport" },
                new Node { Roles = [1], Name = "Sender & Beneficiary Report", Url = "/SenderBeneficiaryReport" },
            }},

            new Node { Roles = [4, 5, 7, 10, 11], Name = "Services", Icon = "AccountBalanceWalletIcon", Children = new[]
            {
                new Node { Roles = [4, 7], Name = "Mobile & DTH Recharge", Url = "/Transaction", Service = ServiceKinds.Recharge },
                new Node { Roles = [4], Name = "Money Transfer", Url = "/DMTTransaction", Service = ServiceKinds.DMT },
                new Node { Roles = [4, 5], Name = "Bank Payout", Url = "/PayoutV1", Service = ServiceKinds.Payout },
                new Node { Roles = [4], Name = "AEPS", Url = "/AEPSIndex", Service = ServiceKinds.AEPS },
                new Node { Roles = [4], Name = "Cash Deposit", Url = "/CashDepositIndex", Service = ServiceKinds.CashDeposit },
                new Node { Roles = [4], Name = "AEPS to Wallet", Url = "/WalletTransfer" },
            }},

            new Node { Roles = [1, 2, 3, 4, 5, 7, 9, 10, 11], Name = "Reports", Icon = "AssessmentIcon", Children = new[]
            {
                new Node { Roles = [1, 2, 3, 4, 5, 7, 11], Name = "Recharge Report", Url = "/Recharge", Service = ServiceKinds.Recharge },
                new Node { Roles = [1, 2, 3, 4, 5, 11], Name = "DMT Report", Url = "/DMTReport", Service = ServiceKinds.DMT },
                new Node { Roles = [1, 2, 3, 4, 5, 11], Name = "AEPS Report", Url = "/AEPSReport", Service = ServiceKinds.AEPS },
                new Node { Roles = [1, 2, 3, 4, 5, 11], Name = "MicroATM Report", Url = "/MicroATMReport", Service = ServiceKinds.MicroATM },
                new Node { Roles = [1, 2, 3, 4, 5, 11], Name = "Payout Report", Url = "/PayoutReport", Service = ServiceKinds.Payout },
                // A report belongs to whoever may sell the service behind it. BBPS, Settlement
                // and TDS were pinned to a narrower set than the rest for no reason anyone could
                // state, so a distributor or an API user saw a Reports list with holes in it —
                // bill payments they had made, and tax they had paid, with no screen to read
                // them on. They now carry the same roles as the other service reports; the
                // Service gate still hides BBPS where no BBPS service is switched on, and every
                // one of these endpoints already scopes a non-admin caller to their own rows.
                new Node { Roles = [1, 2, 3, 4, 5, 11], Name = "BBPS Report", Url = "/BBPSReport", Service = ServiceKinds.BBPS },
                new Node { Roles = [1, 2, 3, 4, 5, 7, 11], Name = "Settlement Report", Url = "/SettlementReport" },
                new Node { Roles = [1, 2, 3, 4, 5, 7, 9, 10, 11], Name = "Fund Report", Url = "/Fund/Report" },
                // Beside Fund Report, because the two are read together: what went down the
                // network, and what was pulled back up. Carries both the raised requests and the
                // completed reversals, told apart by the Kind column.
                new Node { Roles = [1, 2, 3], Name = "Fund Reversal Report", Url = "/FundReverseReport" },
                // The credit position of the network, read rather than worked: what is owed, how
                // old each part of it is, and who is past their limit. The operational screen
                // (set a limit, record a collection) stays under Fund Management; this is the one
                // a reader comes to Reports looking for, and the one that exports. The accountant
                // is included because an unrecovered receivable is their figure too.
                //
                // Deliberately NOT the retailer or API-user tier. The report is a downline view
                // and those roles have no downline, so it could only ever come back empty for
                // them — and the endpoint refuses them outright (ApiAccessPolicy), which would
                // make this a menu row that opens an error. Their own side of the ledger is on
                // the Outstanding screen and on the header bar, where they already read it.
                new Node { Roles = [1, 2, 3, 6, 9, 10, 11], Name = "Outstanding Report", Url = "/OutstandingReport" },
                // "Wallet Transfer Report" is deliberately not listed any more: it and Settlement
                // Report above are the same report of the same rows (Fund/SelectWalletTransferReport
                // — AEPS earnings moved out of the AEPS wallet), and two menu rows for one report
                // is how a reader ends up believing there are two different figures. Settlement
                // Report is the name the business uses for it. The screen itself still exists at
                // /MoveFundsReport for anyone holding the link.
                // Tickets and Complaints moved to the Support section above — see the note there.
                // A support desk listed among 30 financial reports is a support desk nobody finds.
                new Node { Roles = [1, 2, 3, 4, 5, 7, 11], Name = "Account Statement", Url = "/AccountStatement" },
                // The sale invoices raised on this account. The admin and the accountant read
                // every party's under Accountant; a retailer, distributor or super-distributor
                // reads their own here - Invoice/Select scopes a non-admin caller to themselves.
                new Node { Roles = [2, 3, 4, 5, 7], Name = "My Invoices", Url = "/Invoice" },
                // Admin is included so they can look up the slab (and margin) assigned to any
                // downline user from the report's Entity picker.
                new Node { Roles = [1, 2, 3, 4, 5, 7], Name = "Commission Structure", Url = "/CommissionStructure" },
                new Node { Roles = [1, 2, 3, 4, 5, 7], Name = "Sales by Service", Url = "/SaleReport" },
                new Node { Roles = [1], Name = "Sales by Operator", Url = "/OperatorSale" },
                new Node { Roles = [1, 2, 3, 4, 5], Name = "Sales by User", Url = "/UserwiseSale" },
                new Node { Roles = [1], Name = "User Accounting", Url = "/UserwiseAccounting" },
                // "Ledger" is deliberately not listed any more either. Account Statement above
                // reads the same postings with more of them resolved — the wallet each movement
                // was in, the counterparty, the charge, the commission and the running balance —
                // so the Ledger row was the same data with fewer columns under a second name. The
                // screen still exists at /UserLedger for anyone holding the link; the PLATFORM's
                // own ledger is a different report and stays on the Administration menu.
                // The accountant (9) belongs on all three of these and was on none of them. The
                // daybook is the operator-wise day's earnings, the income report is the margin and
                // the TDS report is what was withheld — between them they are most of what the
                // role exists to read, and it had access to neither the figures nor the screens.
                // Profit & Loss sits with the daybook, the income report and TDS: the four screens
                // the accounting desk exists to read. Admin and the accountant only - every column
                // on it is a margin or a tax, and a distributor reading the house's margin on the
                // services they resell is a commercial problem, not a permissions nicety.
                new Node { Roles = [1, 6, 9], Name = "Profit & Loss", Url = "/ProfitLoss" },
                new Node { Roles = [1, 2, 3, 4, 5, 7, 9], Name = "Daybook", Url = "/Daybook/Report" },
                new Node { Roles = [1, 2, 3, 4, 5, 7, 9], Name = "Income Report", Url = "/Income" },
                // The old per-transaction TDS report is off this menu: the month's TDS lives on
                // the Accountant section (TDS Report → /TdsMonthly), and two rows of the same
                // name with different figures is how a reader stops trusting both.
                new Node { Roles = [1, 2, 3, 4, 5], Name = "Past Settlements", Url = "/PastSuccessFailReport" },
                new Node { Roles = [1, 2, 3, 4, 5, 6, 7, 8], Name = "Login History", Url = "/LoginHistory" },
                // Why a balance is frozen, and by whom. Visible to the user whose money it is as
                // well as to the admin — a hold the user cannot see explained is a support call.
                new Node { Roles = [1, 2, 3, 4, 5, 6], Name = "Balance Hold Report", Url = "/HoldReport" },
                new Node { Roles = [1], Name = "DMT Range by User", Url = "/UserWiseDMTRangeReport" },
                new Node { Roles = [1], Name = "Recharge by Range", Url = "/RangeWiseRecharge" },
                new Node { Roles = [1, 2, 3, 4, 5], Name = "Service Charges", Url = "/ServiceChargeDeductionReport" },
                new Node { Roles = [1], Name = "Operator Sale by User", Url = "/OperatorWiseUserWiseReport" },
                new Node { Roles = [1], Name = "Daily Sale Comparison", Url = "/UserWiseServiceSellReport" },
                new Node { Roles = [1, 4, 5], Name = "Payout Beneficiaries", Url = "/PayoutBeneficiaryReport" },
                new Node { Roles = [1], Name = "Provider Performance", Url = "/ProviderWisePerformanceGraph" },
            }},

            new Node { Roles = [1, 2, 3, 10, 11], Name = "Users", Icon = "PeopleIcon", Children = new[]
            {
                new Node { Roles = [1, 2, 3, 10, 11], Name = "All Users", Url = "/Users" },
                new Node { Roles = [1], Name = "Registration Approvals", Url = "/RegistrationApprovals" },
                new Node { Roles = [1], Name = "Agent Data Import/Export", Url = "/AgentDetailReport" },
                new Node { Roles = [1], Name = "Subscription Plans", Url = "/CreatePlanIndex" },
                new Node { Roles = [1], Name = "Assign Subscription", Url = "/AssignPlan" },
                new Node { Roles = [1], Name = "Inactive Users", Url = "/Users/Inactive" },
                new Node { Roles = [1, 6], Name = "KYC Approvals", Url = "/Users/KycApprovals" },
                // Downline is the whole-network view with balances - the platform operator's
                // report. A distributor still sees their own network, with balances, on their
                // dashboard; this listing is not a second copy of it for them.
                // Roles [1, 6] on the operator-only rows below: ApiAccessPolicy.Admin — the gate
                // every one of these endpoints sits behind — resolves to Admin AND Support, so
                // listing only [1] here would have let support staff past the API check while the
                // menu never showed them the screen. The two layers now say the same thing.
                new Node { Roles = [1, 6], Name = "Downline Network", Url = "/DownlineReport" },
                new Node { Roles = [1, 2, 3], Name = "Top Performers", Url = "/Performance" },
                new Node { Roles = [1], Name = "User Aliases", Url = "/Users/UserAlias" },
                new Node { Roles = [1], Name = "New User Report", Url = "/Users/DatewiseReport" },
                // Deleting a user, and restoring one, is the operator's call (Users/Delete is
                // Admin-only server-side), so the screen that lists deleted accounts and offers
                // Restore belongs with it rather than in a distributor's menu.
                new Node { Roles = [1, 6], Name = "Deleted Users", Url = "/Users/DeletedUsers" },
                new Node { Roles = [1, 6], Name = "User Transfer", Url = "/Users/UserTransfer" },
                // Bulk action re-parents, deactivates and holds commission across many accounts
                // at once. Users/BulkUpdate is Admin-only server-side, so the SD / Distributor
                // copy of this screen could only ever fail - it is gone rather than left to 403.
                new Node { Roles = [1, 6], Name = "Bulk User Actions", Url = "/Users/UserWiseBulkAction" },
                // Who changed what on a user record, when, why and from which IP.
                new Node { Roles = [1, 6], Name = "User Audit Trail", Url = "/Users/ChangeHistory" },
            }},

            // The group is visible to the distribution tier now, because a super-distributor and a
            // distributor cut their own rate cards here. The two admin screens inside it stay
            // admin-only: "Manage Slabs" writes the card that caps the whole ecosystem, and
            // "Assign Slab" hands any card to any user. A reseller works from "My Commission
            // Slabs", which shows them their own ceiling, their own cards and only their own
            // network to assign to.
            new Node { Roles = [1, 2, 3, 6], Name = "Commission Slabs", Icon = "LocalAtmIcon", Children = new[]
            {
                new Node { Roles = [1, 6], Name = "Manage Slabs", Url = "/Slabs" },
                new Node { Roles = [1, 6], Name = "Assign Slab", Url = "/Slabs/Assign" },
                new Node { Roles = [1, 2, 3, 6], Name = "My Commission Slabs", Url = "/Slabs/Delegation" },
                new Node { Roles = [1, 2, 3, 6], Name = "Commission Audit Trail", Url = "/Slabs/Audit" },
            }},

            new Node { Roles = [1], Name = "Operators", Icon = "DockIcon", Children = new[]
            {
                new Node { Roles = [1], Name = "Add Operator", Url = "/Operators/Add" },
                new Node { Roles = [1], Name = "Operator List", Url = "/Operators/Index" },
                new Node { Roles = [1], Name = "BBPS Configuration", Url = "/BBPSConfiguration/Index" },
                // "Userwise Operator" is deliberately not listed: per-user operator control is set
                // from the user's own service settings, and a second read-only listing of the same
                // mapping only invited edits in the wrong place. The screen itself still exists at
                // /OperatorUserwise for anyone who has the link.
            }},

            new Node { Roles = [1], Name = "Providers", Icon = "BusinessIcon", Children = new[]
            {
                new Node { Roles = [1], Name = "API Providers", Url = "/ProvidersAdd" },
                new Node { Roles = [1], Name = "User-wise Routing", Url = "/UserwiseSequenceIndex" },
                new Node { Roles = [1], Name = "User-wise DMT Routing", Url = "/UserWiseDMTSequenceAdd" },
                new Node { Roles = [1], Name = "Provider Routing", Url = "/ProvidersSequence" },
                new Node { Roles = [1], Name = "Denominations", Url = "/DenominationDetails" },
                new Node { Roles = [1], Name = "Denomination Mapping", Url = "/DenominationMapping" },
                new Node { Roles = [1], Name = "DMT Range Routing", Url = "/RangeWiseSequence" },
                new Node { Roles = [1], Name = "Circle-wise Routing", Url = "/CircleAPIAdd" },
                // Reconciliation Settings / Upload are gone: provider reconciliation is done from
                // the Provider Sales and Bank Reconciliation screens, and the two legacy rows only
                // ever opened an empty list and a form nothing read.
                new Node { Roles = [1], Name = "Provider Purchase", Url = "/AddPurchase" },
            }},

            new Node { Roles = [1, 5], Name = "Settings", Icon = "Settings", Children = new[]
            {
                new Node { Roles = [1], Name = "Key Settings", Url = "/KeySetting" },
                new Node { Roles = [1], Name = "UI Settings", Url = "/UISetting" },
                new Node { Roles = [1], Name = "API Documentation", Url = "/ApiDocs" },
                new Node { Roles = [1], Name = "Website Pages", Url = "/WebsitePages" },
                new Node { Roles = [1], Name = "SMS Gateway", Url = "/SMSAdd" },
                new Node { Roles = [1], Name = "Message Templates", Url = "/MessageSetting" },
                // Rewrite a provider's own wording before a user reads it. Next to Message
                // Templates because both answer "what does the platform say"; it had no menu row
                // at all, which is most of why it was reported as not working — the only way to
                // reach the screen was to type the URL.
                new Node { Roles = [1], Name = "Response Messages", Url = "/MessageRules" },
                // Banner Sliders, Service Banners, Menu Configuration, Bulk Messages and Bank
                // Accounts are off the menu: the banners are edited on Website Pages, the menu
                // from Menu Visibility, bulk messaging from Broadcast Alert, and a user's bank
                // account from their own profile. Each of these was a read-only dump of a table
                // with a second, working screen elsewhere.
                // Support Access and Virtual Accounts stay until the support user type lands
                // (next sprint); they are listed so the operator can see them exist.
                new Node { Roles = [1], Name = "Support Access", Url = "/SupportAccess" },
                new Node { Roles = [1], Name = "DMT Bank Settings", Url = "/DMTBankSetting" },
                new Node { Roles = [1], Name = "Virtual Accounts", Url = "/VirtualAccountSetting" },
            }},

            new Node { Roles = [1], Name = "Service Configuration", Icon = "Build", Children = new[]
            {
                new Node { Name = "DMT Settings", Url = "/DMTSettings" },
                new Node { Name = "AEPS Settings", Url = "/AEPSSetting" },
                new Node { Name = "MicroATM Settings", Url = "/MicroATMSetting" },
                new Node { Name = "POS Settings", Url = "/POSSetting" },
                new Node { Name = "Payout Settings", Url = "/PayoutSetting" },
                new Node { Name = "Cash Deposit Settings", Url = "/CashDepositSetting" },
                new Node { Name = "E-Collection Settings", Url = "/EcollectionSetting" },
                new Node { Name = "e-KYC Settings", Url = "/eKYCSetting" },
                new Node { Name = "CMS Settings", Url = "/CMSSetting" },
                new Node { Name = "Invoice Settings", Url = "/InvoiceSettings" },
                new Node { Name = "Verification Setup", Url = "/VerificationSettings" },
                new Node { Name = "Wallets", Url = "/Wallets/Index" },
                new Node { Name = "Service Catalog", Url = "/Services/Index" },
                new Node { Name = "Service Wallet Mapping", Url = "/SeviceswiseWallet" },
                new Node { Name = "Subscription Charges", Url = "/ServiceSubscriptionCharge" },
            }},
        };
    }
}
