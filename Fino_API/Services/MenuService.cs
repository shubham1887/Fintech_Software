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

            new Node { Roles = [1, 6], Name = "Search", Url = "/Search", Icon = "Search" },
            new Node { Roles = [4, 5], Name = "Search", Url = "/RetailerSearch", Icon = "Search" },

            new Node { Roles = [1, 6], Name = "Pending", Icon = "Pets", Children = new[]
            {
                new Node { Name = "Recharge ", Url = "/Pending" },
                new Node { Name = "DMT", Url = "/PendingDmt" },
                new Node { Name = "Recon", Url = "/PendingRecon" },
            }},

            new Node { Roles = [1, 2, 3, 4, 5, 10, 11], Name = "Fund", Icon = "MoneyIcon", Children = new[]
            {
                new Node { Roles = [1], Name = "Virtual Balance", Url = "/VirtualBalance" },
                new Node { Roles = [1], Name = "Mode Master", Url = "/ModeMaster" },
                new Node { Roles = [1, 2, 3, 4, 10, 11], Name = "Pending Request", Url = "/Fund" },
                new Node { Roles = [1, 2, 3, 10, 11], Name = "Fund Transfer", Url = "/Fund/Transfer" },
                new Node { Roles = [1, 2, 3], Name = "Fund Reverse", Url = "/FundReverse" },
                new Node { Roles = [1], Name = "Reverse Requests", Url = "/FundReverseRequestReport" },
                new Node { Roles = [2, 3, 4, 5], Name = "Request Raise", Url = "/Fund/Request" },
                new Node { Roles = [2, 3], Name = "Rev Request Raise", Url = "/Fund/ReverseRequest" },
                new Node { Roles = [2, 3], Name = "Transfer to e-balance", Url = "/WalletTransfer" },
            }},

            new Node { Roles = [1, 9], Name = "Admin", Icon = "MoneyIcon", Children = new[]
            {
                new Node { Roles = [1, 4, 5], Name = "Complaints", Url = "/Complaints" },
                new Node { Roles = [1], Name = "Invoice", Url = "/Invoice" },
                new Node { Roles = [1], Name = "Sent SMS", Url = "/SmsReport" },
                new Node { Roles = [1], Name = "Provider Log", Url = "/ProviderLog" },
                new Node { Roles = [1], Name = "API-User Log", Url = "/ApiUserLog" },
                new Node { Roles = [1], Name = "Admin Accounting", Url = "/AdminAccounting" },
                new Node { Roles = [1], Name = "Admin Ledger", Url = "/AdminLedger" },
                new Node { Roles = [1], Name = "Provider Daybook", Url = "/ProviderDayBook" },
                new Node { Roles = [1], Name = "Provider Sale", Url = "/ProviderSale" },
                new Node { Roles = [1], Name = "Purchase Report", Url = "/PurchaseReport" },
                new Node { Roles = [1], Name = "Change Status", Url = "/ChangeStatus" },
                new Node { Roles = [1], Name = "Manual Update", Url = "/ManualSuccessFailReport" },
                new Node { Roles = [1], Name = "User Onboard Report", Url = "/UserOnboardedReport" },
                new Node { Roles = [1], Name = "Ticket Report", Url = "/TicketReport" },
                new Node { Roles = [1], Name = "Sender Beneficiary", Url = "/SenderBeneficiaryReport" },
            }},

            new Node { Roles = [4, 5, 7, 10, 11], Name = "Services", Icon = "AccountBalanceWalletIcon", Children = new[]
            {
                new Node { Roles = [4, 7], Name = "Recharge", Url = "/Transaction", Service = ServiceKinds.Recharge },
                new Node { Roles = [4], Name = "DMT", Url = "/DMTTransaction", Service = ServiceKinds.DMT },
                new Node { Roles = [4, 5], Name = "Transfer to Bank A/c", Url = "/PayoutV1", Service = ServiceKinds.Payout },
                new Node { Roles = [4], Name = "AEPS", Url = "/AEPSIndex", Service = ServiceKinds.AEPS },
                new Node { Roles = [4], Name = "Cash Deposit", Url = "/CashDepositIndex", Service = ServiceKinds.CashDeposit },
                new Node { Roles = [4], Name = "Transfer to e-balance", Url = "/WalletTransfer" },
            }},

            new Node { Roles = [1, 2, 3, 4, 5, 7, 9, 10, 11], Name = "Report", Icon = "AssessmentIcon", Children = new[]
            {
                new Node { Roles = [1, 2, 3, 4, 5, 7, 11], Name = "Recharge Report", Url = "/Recharge", Service = ServiceKinds.Recharge },
                new Node { Roles = [1, 2, 3, 4, 5, 11], Name = "DMT Report", Url = "/DMTReport", Service = ServiceKinds.DMT },
                new Node { Roles = [1, 2, 3, 4, 5, 11], Name = "AEPS Report", Url = "/AEPSReport", Service = ServiceKinds.AEPS },
                new Node { Roles = [1, 2, 3, 4, 5], Name = "MicroATM Report", Url = "/MicroATMReport", Service = ServiceKinds.MicroATM },
                new Node { Roles = [1, 2, 3, 4, 5], Name = "PayOut Report", Url = "/PayoutReport", Service = ServiceKinds.Payout },
                new Node { Roles = [1, 4], Name = "BBPS Report", Url = "/BBPSReport", Service = ServiceKinds.BBPS },
                new Node { Roles = [1, 4, 2, 3], Name = "Settlement Report", Url = "/SettlementReport" },
                new Node { Roles = [1, 2, 3, 4, 5, 7, 9, 10, 11], Name = "Fund Report", Url = "/Fund/Report" },
                new Node { Roles = [1, 2, 3, 4, 5, 7, 9, 10, 11], Name = "Transfer to e-Balance Report", Url = "/MoveFundsReport" },
                new Node { Roles = [4, 5, 7], Name = "Complaints", Url = "/Complaints" },
                new Node { Roles = [1, 2, 3, 4, 5, 7, 11], Name = "Account Statement", Url = "/AccountStatement" },
                // Admin is included so they can look up the slab (and margin) assigned to any
                // downline user from the report's Entity picker.
                new Node { Roles = [1, 2, 3, 4, 5, 7], Name = "Commission Structure", Url = "/CommissionStructure" },
                new Node { Roles = [1, 4, 5, 7], Name = "Service Wise Sale", Url = "/SaleReport" },
                new Node { Roles = [1], Name = "Operator Sale", Url = "/OperatorSale" },
                new Node { Roles = [1, 4, 5], Name = "UserWiseSale", Url = "/UserwiseSale" },
                new Node { Roles = [1], Name = "UserwiseAccounting", Url = "/UserwiseAccounting" },
                new Node { Roles = [1, 2, 3, 4, 5, 7], Name = "Ledger", Url = "/UserLedger" },
                new Node { Roles = [1, 2, 3, 4, 5, 7], Name = "Daybook", Url = "/Daybook/Report" },
                new Node { Roles = [1, 2, 3, 4, 5, 7], Name = "Income Report", Url = "/Income" },
                new Node { Roles = [1, 4], Name = "TDS Report", Url = "/TDS" },
                new Node { Roles = [1, 4, 5], Name = "Past SuccessFail Report", Url = "/PastSuccessFailReport" },
                new Node { Roles = [1, 2, 3, 4, 5, 6, 7, 8], Name = "Login History", Url = "/LoginHistory" },
                new Node { Roles = [1], Name = "UserWise Range Report", Url = "/UserWiseDMTRangeReport" },
                new Node { Roles = [1], Name = "RangeWise Recharge", Url = "/RangeWiseRecharge" },
                new Node { Roles = [1, 2, 3, 4, 5], Name = "Service Charge", Url = "/ServiceChargeDeductionReport" },
                new Node { Roles = [1], Name = "OperatorWise UserWise Report", Url = "/OperatorWiseUserWiseReport" },
                new Node { Roles = [1], Name = " UserWise OperatorSale Report", Url = "/UserWiseServiceSellReport" },
                new Node { Roles = [1, 4, 5], Name = "Payout Beneficiary ", Url = "/PayoutBeneficiaryReport" },
                new Node { Roles = [1], Name = "Provider Wise Performance Graph", Url = "/ProviderWisePerformanceGraph" },
            }},

            new Node { Roles = [1, 2, 3, 10, 11], Name = "Users", Icon = "PeopleIcon", Children = new[]
            {
                new Node { Roles = [1, 2, 3, 10, 11], Name = "All Users", Url = "/Users" },
                new Node { Roles = [1], Name = "Agent Detail Upload Download", Url = "/AgentDetailReport" },
                new Node { Roles = [1], Name = "Subscription", Url = "/CreatePlanIndex" },
                new Node { Roles = [1], Name = "Assign Plan", Url = "/AssignPlan" },
                new Node { Roles = [1], Name = "Inactive Users", Url = "/Users/Inactive" },
                new Node { Roles = [1, 6], Name = "KYC Approvals", Url = "/Users/KycApprovals" },
                new Node { Roles = [1, 2, 3], Name = "Downline", Url = "/DownlineReport" },
                new Node { Roles = [1, 2, 3], Name = "Top Performance", Url = "/Performance" },
                new Node { Roles = [1], Name = "UserAlias", Url = "/Users/UserAlias" },
                new Node { Roles = [1], Name = "Newly Added User Report", Url = "/Users/DatewiseReport" },
                new Node { Roles = [1, 2, 3], Name = "Deleted Users", Url = "/Users/DeletedUsers" },
                new Node { Roles = [1], Name = "User Transfer", Url = "/Users/UserTransfer" },
                new Node { Roles = [1], Name = "Userwise Bulk Action", Url = "/Users/UserWiseBulkAction" },
                new Node { Roles = [2, 3], Name = "Userwise Bulk Action", Url = "/UserWiseBulkActionSDAndDist" },
            }},

            new Node { Roles = [1, 6], Name = "Slab", Icon = "LocalAtmIcon", Children = new[]
            {
                new Node { Roles = [1, 6], Name = "General Add", Url = "/Slabs" },
                new Node { Roles = [1, 6], Name = "General Assign", Url = "/Slabs/Assign" },
            }},

            new Node { Roles = [1], Name = "Operators", Icon = "DockIcon", Children = new[]
            {
                new Node { Roles = [1], Name = "Add Operator", Url = "/Operators/Add" },
                new Node { Roles = [1], Name = "Operator Details", Url = "/Operators/Index" },
                new Node { Roles = [1], Name = "BBPS Configuration", Url = "/BBPSConfiguration/Index" },
                new Node { Roles = [1], Name = "Userwise Operator", Url = "/OperatorUserwise" },
            }},

            new Node { Roles = [1], Name = "API", Icon = "BusinessIcon", Children = new[]
            {
                new Node { Roles = [1], Name = "Add API", Url = "/ProvidersAdd" },
                new Node { Roles = [1], Name = "Userwise Sequence", Url = "/UserwiseSequenceIndex" },
                new Node { Roles = [1], Name = "UserwiseDMT Sequence", Url = "/UserWiseDMTSequenceAdd" },
                new Node { Roles = [1], Name = "Provider Sequence", Url = "/ProvidersSequence" },
                new Node { Roles = [1], Name = "Denomination", Url = "/DenominationDetails" },
                new Node { Roles = [1], Name = "Denomination Mapping", Url = "/DenominationMapping" },
                new Node { Roles = [1], Name = "DMT Range Sequence", Url = "/RangeWiseSequence" },
                new Node { Roles = [1], Name = "Circle API", Url = "/CircleAPIAdd" },
                new Node { Roles = [1], Name = "Reconciliation", Url = "/ReconciliationSettings" },
                new Node { Roles = [1], Name = "Reconciliation Upload", Url = "/ReconciliationUpload" },
                new Node { Roles = [1], Name = "Provider Purchase", Url = "/AddPurchase" },
            }},

            new Node { Roles = [1, 5], Name = "Setting", Icon = "Settings", Children = new[]
            {
                new Node { Roles = [1], Name = "Key Setting", Url = "/KeySetting" },
                new Node { Roles = [1], Name = "UI Setting", Url = "/UISetting" },
                new Node { Roles = [1], Name = "Website Pages", Url = "/WebsitePages" },
                new Node { Roles = [1], Name = "Plan Setting", Url = "/PlanSetting" },
                new Node { Roles = [1], Name = "SMS Setting", Url = "/SMSAdd" },
                new Node { Roles = [1], Name = "Msg Setting", Url = "/MessageSetting" },
                new Node { Roles = [1], Name = "Notification (News)", Url = "/News/Index" },
                new Node { Roles = [1], Name = "Slider Setting", Url = "/Slider" },
                new Node { Roles = [1], Name = "Service Wise Slider", Url = "/ServiceWiseSlider" },
                new Node { Roles = [1], Name = "Support Access", Url = "/SupportAccess" },
                new Node { Roles = [1, 2, 3], Name = "Add Bank", Url = "/AddBank" },
                new Node { Roles = [1], Name = "DMT Bank Setting", Url = "/DMTBankSetting" },
                new Node { Roles = [1], Name = "Virtual Account", Url = "/VirtualAccountSetting" },
                new Node { Roles = [1], Name = "Menu Add", Url = "/MenuAdd" },
                new Node { Roles = [1], Name = "App Alert", Url = "/PushNotifications" },
                new Node { Roles = [1], Name = "Bulk Message", Url = "/BulkMessageIndex" },
            }},

            new Node { Roles = [1], Name = "Service", Icon = "Build", Children = new[]
            {
                new Node { Name = "DMT Settings", Url = "/DMTSettings" },
                new Node { Name = "AEPS Setting", Url = "/AEPSSetting" },
                new Node { Name = "MicroATM Setting", Url = "/MicroATMSetting" },
                new Node { Name = "POS Setting", Url = "/POSSetting" },
                new Node { Name = "Payout Setting", Url = "/PayoutSetting" },
                new Node { Name = "Cash Deposit Setting", Url = "/CashDepositSetting" },
                new Node { Name = "Ecollection Setting", Url = "/EcollectionSetting" },
                new Node { Name = "eKYC Setting", Url = "/eKYCSetting" },
                new Node { Name = "CMS Setting", Url = "/CMSSetting" },
                new Node { Name = "Invoice Setting", Url = "/InvoiceSettings" },
                new Node { Name = "Verification Setting", Url = "/VerificationSettings" },
                new Node { Name = "Wallet", Url = "/Wallets/Index" },
                new Node { Name = "Service List", Url = "/Services/Index" },
                new Node { Name = "Service Wise Wallet", Url = "/SeviceswiseWallet" },
                new Node { Name = "Service Subscription", Url = "/ServiceSubscriptionCharge" },
            }},
        };
    }
}
