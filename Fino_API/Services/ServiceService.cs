using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Service;
using FintechApp.Responses.Service;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IOperatorRepository _operatorRepository;
        private readonly IWalletServiceMappingService _walletServiceMappingService;
        private readonly IWalletMasterRepository _walletRepository;
        private readonly IUserServiceAccessService _userServiceAccessService;

        public ServiceService(
            IServiceRepository serviceRepository,
            IOperatorRepository operatorRepository,
            IWalletServiceMappingService walletServiceMappingService,
            IWalletMasterRepository walletRepository,
            IUserServiceAccessService userServiceAccessService)
        {
            _serviceRepository = serviceRepository;
            _operatorRepository = operatorRepository;
            _walletServiceMappingService = walletServiceMappingService;
            _walletRepository = walletRepository;
            _userServiceAccessService = userServiceAccessService;
        }

        public async Task<ServiceResponse?> GetServiceById(int serviceId)
        {
            var service = await _serviceRepository.GetServiceById(serviceId);
            if (service == null) return null;
            return MapToResponse(service);
        }

        public async Task<List<ServiceResponse>> GetServices(GetServiceRequest request)
        {
            var services = await _serviceRepository.GetServices(request);
            return services.Select(MapToResponse).ToList();
        }

        public async Task<string> CreateService(CreateServiceRequest request)
        {
            var service = new ServiceMaster
            {
                Name = request.Name,
                ServiceCategory = request.ServiceCategory,
                ServiceImagePath = request.ServiceImagePath,
                UISequence = request.UISequence,
                HSNCode = request.HSNCode,
                HSNCodeForSelfInvoice = request.HSNCodeForSelfInvoice,
                Charge = request.Charge,
                MinBalance = request.MinBalance,
                IsActive = request.IsActive,
                IsDisplay = request.IsDisplay,
                IsDefault = request.IsDefault,
                IsOutside = request.IsOutside,
                IsBBPS = request.IsBBPS,
                IsSubscriptionAllow = request.IsSubscriptionAllow,
                IsGSTInclusive = request.IsGSTInclusive,
                ReportKind = (request.ReportKind ?? string.Empty).Trim().ToUpperInvariant(),
                IsDelete = false,
                IsDeleted = false,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _serviceRepository.Insert(service);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> UpdateService(UpdateServiceRequest request)
        {
            var service = await _serviceRepository.GetServiceById(request.ServiceID);
            if (service == null)
                return FintechApp.Messages.ServiceMessages.ServiceNotFound;

            switch (request.FieldName.ToLower())
            {
                case "name":
                    service.Name = request.NewValue;
                    break;
                case "servicecategory":
                    service.ServiceCategory = request.NewValue;
                    break;
                case "serviceimagepath":
                    service.ServiceImagePath = request.NewValue;
                    break;
                case "uisequence":
                    if (!int.TryParse(request.NewValue, out int uiSequence))
                        return FintechApp.Messages.ServiceMessages.InvalidValue;
                    service.UISequence = uiSequence;
                    break;
                case "hsncode":
                    service.HSNCode = request.NewValue;
                    break;
                case "hsncodeforselfinvoice":
                    service.HSNCodeForSelfInvoice = request.NewValue;
                    break;
                case "charge":
                    if (!decimal.TryParse(request.NewValue, out decimal charge))
                        return FintechApp.Messages.ServiceMessages.InvalidValue;
                    service.Charge = charge;
                    break;
                case "minbalance":
                    if (!decimal.TryParse(request.NewValue, out decimal minBalance))
                        return FintechApp.Messages.ServiceMessages.InvalidValue;
                    service.MinBalance = minBalance;
                    break;
                case "isactive":
                    if (!bool.TryParse(request.NewValue, out bool isActive))
                        return FintechApp.Messages.ServiceMessages.InvalidValue;
                    service.IsActive = isActive;
                    break;
                case "isdisplay":
                    if (!bool.TryParse(request.NewValue, out bool isDisplay))
                        return FintechApp.Messages.ServiceMessages.InvalidValue;
                    service.IsDisplay = isDisplay;
                    break;
                case "isdefault":
                    if (!bool.TryParse(request.NewValue, out bool isDefault))
                        return FintechApp.Messages.ServiceMessages.InvalidValue;
                    service.IsDefault = isDefault;
                    break;
                case "isoutside":
                    if (!bool.TryParse(request.NewValue, out bool isOutside))
                        return FintechApp.Messages.ServiceMessages.InvalidValue;
                    service.IsOutside = isOutside;
                    break;
                case "isbbps":
                    if (!bool.TryParse(request.NewValue, out bool isBBPS))
                        return FintechApp.Messages.ServiceMessages.InvalidValue;
                    service.IsBBPS = isBBPS;
                    break;
                case "issubscriptionallow":
                    if (!bool.TryParse(request.NewValue, out bool isSubscriptionAllow))
                        return FintechApp.Messages.ServiceMessages.InvalidValue;
                    service.IsSubscriptionAllow = isSubscriptionAllow;
                    break;
                case "isgstinclusive":
                    if (!bool.TryParse(request.NewValue, out bool isGSTInclusive))
                        return FintechApp.Messages.ServiceMessages.InvalidValue;
                    service.IsGSTInclusive = isGSTInclusive;
                    break;
                case "reportkind":
                    string kind = (request.NewValue ?? string.Empty).Trim().ToUpperInvariant();
                    if (kind.Length > 0 && !ServiceKinds.All.Contains(kind))
                        return FintechApp.Messages.ServiceMessages.InvalidValue;
                    service.ReportKind = kind; // empty = auto-detect from name/category
                    break;
                default:
                    return FintechApp.Messages.ServiceMessages.InvalidFieldName;
            }

            service.UpdatedateTime = DateTime.UtcNow;

            int result = await _serviceRepository.Update(service);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> DeleteService(int serviceId)
        {
            var service = await _serviceRepository.GetServiceById(serviceId);
            if (service == null)
                return FintechApp.Messages.ServiceMessages.ServiceNotFound;

            service.IsDelete = true;
            service.IsDeleted = true;
            service.UpdatedateTime = DateTime.UtcNow;

            int result = await _serviceRepository.Delete(service);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<List<ServiceDropdownResponse>> GetServiceDropdown()
        {
            return await _serviceRepository.GetDropDownServices();
        }

        // includeHidden=true (admin) returns everything; false (retailer/others) returns only
        // active + displayable services with their active operators.
        public async Task<List<ServiceCatalogResponse>> GetServiceCatalog(bool includeHidden, int userID = 0)
        {
            var services = await _serviceRepository.GetAllServices();
            var operators = await _operatorRepository.GetAllOperators();
            List<Responses.WalletServiceMapping.WalletServiceMappingResponse> walletMappings;
            try { walletMappings = await _walletServiceMappingService.GetAll(); }
            catch { walletMappings = new(); }

            var visibleServices = services.Where(s => !s.IsDelete);
            if (!includeHidden)
                visibleServices = visibleServices.Where(s => s.IsActive && s.IsDisplay);

            var operatorsByService = operators
                .Where(o => o.IsDelete != true && (includeHidden || o.IsActive))
                .GroupBy(o => o.ServiceID)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Latest wallet mapping per service wins.
            var walletByService = walletMappings
                .GroupBy(m => m.ServiceID)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.WalletServiceMappingID).First());

            // Cascade: disabling a wallet disables every service that debits from it. For the
            // customer-facing catalog (non-admin), drop services whose mapped wallet is inactive.
            // Services with no wallet mapping are unaffected. Admin (includeHidden) still sees all.
            if (!includeHidden)
            {
                var activeWalletIds = (await _walletRepository.Select()).Select(w => w.WalletID).ToHashSet();
                visibleServices = visibleServices.Where(s =>
                    !walletByService.TryGetValue(s.ServiceID, out var wm) || activeWalletIds.Contains(wm.WalletID));
            }

            // Per-user grants from the Add / Edit User services grid: "Off" services are dropped
            // here, "Lock" ones stay listed but are flagged so the tile can show as locked. The
            // platform KYC gate rides along in the same context: a service the user may have but
            // can't use until their KYC is approved stays on the grid, flagged with its reason.
            var access = userID > 0
                ? await _userServiceAccessService.GetContext(userID)
                : ServiceAccessContext.Unrestricted;
            if (access.Grants != null)
                visibleServices = visibleServices.Where(s =>
                    UserServiceAccessService.Resolve(access, s.ServiceID) != ServiceAccess.NotGranted);

            return visibleServices
                .GroupBy(s => string.IsNullOrWhiteSpace(s.ServiceCategory) ? "Other Services" : s.ServiceCategory.Trim())
                .Select(g => new ServiceCatalogResponse
                {
                    Category = g.Key,
                    Services = g
                        .OrderBy(s => s.UISequence)
                        .ThenBy(s => s.Name)
                        .Select(s => new CatalogServiceItem
                        {
                            ServiceID = s.ServiceID,
                            Name = s.Name,
                            ServiceImagePath = s.ServiceImagePath,
                            UISequence = s.UISequence,
                            Charge = s.Charge,
                            MinBalance = s.MinBalance,
                            IsActive = s.IsActive,
                            IsDisplay = s.IsDisplay,
                            IsBBPS = s.IsBBPS,
                            ReportKind = s.ReportKind,
                            WalletID = walletByService.TryGetValue(s.ServiceID, out var wm) ? wm.WalletID : null,
                            WalletName = walletByService.TryGetValue(s.ServiceID, out var wm2) ? wm2.WalletName : string.Empty,
                            OperatorCount = operatorsByService.TryGetValue(s.ServiceID, out var opsCount) ? opsCount.Count : 0,
                            IsLocked = UserServiceAccessService.Resolve(access, s.ServiceID) != ServiceAccess.Granted,
                            LockReason = UserServiceAccessService.LockReason(UserServiceAccessService.Resolve(access, s.ServiceID)),
                            Operators = operatorsByService.TryGetValue(s.ServiceID, out var ops)
                                ? ops.OrderBy(o => o.UISequence).ThenBy(o => o.OperatorName).Select(o => new CatalogOperatorItem
                                {
                                    OperatorID = o.OperatorID,
                                    OperatorName = o.OperatorName,
                                    OperatorImagePath = o.OperatorImagePath,
                                    Opcode = o.Opcode,
                                    IsActive = o.IsActive,
                                }).ToList()
                                : new List<CatalogOperatorItem>(),
                        }).ToList(),
                })
                .OrderBy(c => c.Services.Min(s => s.UISequence))
                .ThenBy(c => c.Category)
                .ToList();
        }

        private static ServiceResponse MapToResponse(ServiceMaster s)
        {
            return new ServiceResponse
            {
                ServiceID = s.ServiceID,
                Name = s.Name,
                ServiceCategory = s.ServiceCategory,
                ServiceImagePath = s.ServiceImagePath,
                UISequence = s.UISequence,
                HSNCode = s.HSNCode,
                HSNCodeForSelfInvoice = s.HSNCodeForSelfInvoice,
                Charge = s.Charge,
                MinBalance = s.MinBalance,
                IsActive = s.IsActive,
                IsDisplay = s.IsDisplay,
                IsDefault = s.IsDefault,
                IsOutside = s.IsOutside,
                IsBBPS = s.IsBBPS,
                IsSubscriptionAllow = s.IsSubscriptionAllow,
                IsGSTInclusive = s.IsGSTInclusive,
                DateTime = s.DateTime,
                UpdatedateTime = s.UpdatedateTime
            };
        }
    }
}