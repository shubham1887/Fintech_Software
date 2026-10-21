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

        public MenuService(IServiceRepository serviceRepo, IWalletServiceMappingRepository walletServiceRepo,
            IUserServiceAccessService serviceAccess)
        {
            _serviceRepo = serviceRepo;
            _walletServiceRepo = walletServiceRepo;
            _serviceAccess = serviceAccess;
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

            var result = new List<MenuItem>();
            foreach (var node in Definition)
            {
                var item = Filter(node, userTypeId, serviceGated, services);
                if (item != null) result.Add(item);
            }
            return result;
        }

        private static MenuItem? Filter(Node node, int userTypeId, bool serviceGated, List<ServiceMaster> services)
        {
            bool allowed = node.Roles == null || node.Roles.Contains(userTypeId);

            if (node.Children is { Length: > 0 })
            {
                // Parent gate: if the user can't see the group, hide it entirely.
                if (!allowed) return null;

                var kids = new List<MenuItem>();
                foreach (var child in node.Children)
                {
                    var c = Filter(child, userTypeId, serviceGated, services);
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

            new Node { Roles = [1, 6], Name = "Pending Queue", Icon = "Pets", Children = new[]
            {
                new Node { Name = "Recharge", Url = "/Pending" },
                new Node { Name = "DMT", Url = "/PendingDmt" },
                new Node { Name = "Reconciliation", Url = "/PendingRecon" },
            }},

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

            new Node { Roles = [1, 2, 3, 4, 5, 7, 10, 11], Name = "Funds", Icon = "MoneyIcon", Children = new[]
            {
                new Node { Roles = [1], Name = "Virtual Balance", Url = "/VirtualBalance" },
                new Node { Roles = [1], Name = "Payment Modes", Url = "/ModeMaster" },
                new Node { Roles = [1, 2, 3, 4, 10, 11], Name = "Fund Requests", Url = "/Fund" },
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

            new Node { Roles = [1, 9], Name = "Administration", Icon = "MoneyIcon", Children = new[]
            {
                // Role 9 (Accountant) gets the books: invoices, accounting summary, platform
                // ledger, provider daybook, provider sales and the purchase report. It does NOT
                // get Status Correction or Manual Settlement — those change a transaction's
                // outcome, which stays with the admin. ApiAccessPolicy grants the matching
                // controllers to the Accounting / TxnAndAccounting sets; without that the rows
                // would render and every call behind them would fail silently.
                new Node { Roles = [1, 9], Name = "Invoices", Url = "/Invoice" },
                new Node { Roles = [1], Name = "SMS Log", Url = "/SmsReport" },
                new Node { Roles = [1], Name = "Provider Logs", Url = "/ProviderLog" },
                new Node { Roles = [1], Name = "API Partner Logs", Url = "/ApiUserLog" },
                new Node { Roles = [1, 9], Name = "Accounting Summary", Url = "/AdminAccounting" },
                new Node { Roles = [1, 9], Name = "Platform Ledger", Url = "/AdminLedger" },
                // Read-only: it resolves and explains, it writes nothing. The accountant needs it
                // most — it is the only way to see what a tax rule actually does before a month
                // of invoices has been raised on it.
                new Node { Roles = [1, 9], Name = "Tax Rule Simulator", Url = "/TaxSimulator" },
                new Node { Roles = [1, 9], Name = "Provider Daybook", Url = "/ProviderDayBook" },
                new Node { Roles = [1, 9], Name = "Provider Sales", Url = "/ProviderSale" },
                new Node { Roles = [1, 9], Name = "Purchase Report", Url = "/PurchaseReport" },
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
                new Node { Roles = [1, 2, 3, 4, 5, 7, 9, 10, 11], Name = "Wallet Transfer Report", Url = "/MoveFundsReport" },
                // Tickets and Complaints moved to the Support section above — see the note there.
                // A support desk listed among 30 financial reports is a support desk nobody finds.
                new Node { Roles = [1, 2, 3, 4, 5, 7, 11], Name = "Account Statement", Url = "/AccountStatement" },
                // Admin is included so they can look up the slab (and margin) assigned to any
                // downline user from the report's Entity picker.
                new Node { Roles = [1, 2, 3, 4, 5, 7], Name = "Commission Structure", Url = "/CommissionStructure" },
                new Node { Roles = [1, 2, 3, 4, 5, 7], Name = "Sales by Service", Url = "/SaleReport" },
                new Node { Roles = [1], Name = "Sales by Operator", Url = "/OperatorSale" },
                new Node { Roles = [1, 2, 3, 4, 5], Name = "Sales by User", Url = "/UserwiseSale" },
                new Node { Roles = [1], Name = "User Accounting", Url = "/UserwiseAccounting" },
                new Node { Roles = [1, 2, 3, 4, 5, 7], Name = "Ledger", Url = "/UserLedger" },
                new Node { Roles = [1, 2, 3, 4, 5, 7], Name = "Daybook", Url = "/Daybook/Report" },
                new Node { Roles = [1, 2, 3, 4, 5, 7], Name = "Income Report", Url = "/Income" },
                new Node { Roles = [1, 2, 3, 4, 5], Name = "TDS Report", Url = "/TDS" },
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

            new Node { Roles = [1, 6], Name = "Commission Slabs", Icon = "LocalAtmIcon", Children = new[]
            {
                new Node { Roles = [1, 6], Name = "Manage Slabs", Url = "/Slabs" },
                new Node { Roles = [1, 6], Name = "Assign Slab", Url = "/Slabs/Assign" },
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
                new Node { Roles = [1], Name = "Reconciliation Settings", Url = "/ReconciliationSettings" },
                new Node { Roles = [1], Name = "Reconciliation Upload", Url = "/ReconciliationUpload" },
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
                new Node { Roles = [1], Name = "Banner Sliders", Url = "/Slider" },
                new Node { Roles = [1], Name = "Service Banners", Url = "/ServiceWiseSlider" },
                new Node { Roles = [1], Name = "Support Access", Url = "/SupportAccess" },
                new Node { Roles = [1, 2, 3], Name = "Bank Accounts", Url = "/AddBank" },
                new Node { Roles = [1], Name = "DMT Bank Settings", Url = "/DMTBankSetting" },
                new Node { Roles = [1], Name = "Virtual Accounts", Url = "/VirtualAccountSetting" },
                new Node { Roles = [1], Name = "Menu Configuration", Url = "/MenuAdd" },
                new Node { Roles = [1], Name = "Bulk Messages", Url = "/BulkMessageIndex" },
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
