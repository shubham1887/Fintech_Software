using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Invoice;
using FintechApp.Responses.Invoice;
using FintechApp.StaticModels;
using System.Text;

namespace FintechApp.Services
{
    public class InvoiceService : IInvoiceService
    {
        private const int UserTypeID_Admin = 1;
        private const int UserTypeID_Support = 2;
        // DB-authoritative role ids (UserTypeCode / UserTypeMasters seed).
        private const int UserTypeID_SuperDist = UserTypeCode.SuperDistributor;
        private const int UserTypeID_Dist = UserTypeCode.Distributor;

        private readonly IUserRepository _userRepository;
        private readonly IUserTypeMasterRepository _userTypeMasterRepository;
        private readonly ITransactionMasterRepository _transactionMasterRepository;
        private readonly IOperatorRepository _operatorRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IInvoiceSettingRepository _invoiceSettingRepository;
        private readonly ISettingMasterRepository _settingMasterRepository;

        public InvoiceService(
            IUserRepository userRepository,
            IUserTypeMasterRepository userTypeMasterRepository,
            ITransactionMasterRepository transactionMasterRepository,
            IOperatorRepository operatorRepository,
            IServiceRepository serviceRepository,
            IInvoiceSettingRepository invoiceSettingRepository,
            ISettingMasterRepository settingMasterRepository)
        {
            _userRepository = userRepository;
            _userTypeMasterRepository = userTypeMasterRepository;
            _transactionMasterRepository = transactionMasterRepository;
            _operatorRepository = operatorRepository;
            _serviceRepository = serviceRepository;
            _invoiceSettingRepository = invoiceSettingRepository;
            _settingMasterRepository = settingMasterRepository;
        }

        public async Task<(string failureMessage, List<InvoiceReportViewModel>? data)> Select(SelectInvoiceRequest request)
        {
            // Shared scoped DbContext — await sequentially (no concurrent EF queries).
            var allUsers = await _userRepository.GetAllUsers();
            var allUserTypes = await _userTypeMasterRepository.GetAll();
            var userTypeDict = allUserTypes.ToDictionary(ut => ut.UserTypeID, ut => ut.UserType);

            var query = allUsers.Where(u => u.UserMasterID > 2 && u.IsInvoiceApplicable);

            if (request.UserTypeID > 0)
                query = query.Where(u => u.UserTypeID == request.UserTypeID);

            if (request.UserID > 0)
                query = query.Where(u => u.UserMasterID == request.UserID);

            var result = query.Select(u => new InvoiceReportViewModel
            {
                UserID = u.UserMasterID,
                FirmName = u.FirmName ?? string.Empty,
                MobileNumber = u.MobileNumber ?? string.Empty,
                EmailID = u.EmailID ?? string.Empty,
                GSTNo = u.GSTNo ?? string.Empty,
                UserType = u.UserTypeID.HasValue && userTypeDict.TryGetValue(u.UserTypeID.Value, out var ut) ? ut : string.Empty,
                ServiceWise = new SelectByTypeRequest
                {
                    Type = "ServiceWiseSale",
                    UserID = u.UserMasterID,
                    Month = request.Month,
                    Year = request.Year
                },
                OperatorWise = new SelectByTypeRequest
                {
                    Type = "OperatorWiseSale",
                    UserID = u.UserMasterID,
                    Month = request.Month,
                    Year = request.Year
                }
            }).ToList();

            if (result.Count == 0)
                return (Messages.Common.Message.DataNotFound, null);

            return (StatusType.Ok, result);
        }

        public async Task<(string failureMessage, string html)> SelectByType(SelectByTypeRequest request)
        {
            if (request.UserID <= 0 || request.Month <= 0 || request.Year <= 0 || string.IsNullOrEmpty(request.Type))
                return (Messages.Common.Message.InvalidRequest, string.Empty);

            var now = DateTime.Now;
            if (request.Year > now.Year || (request.Year == now.Year && request.Month >= now.Month))
                return ("Invoice can only be generated for past months.", string.Empty);

            var user = await _userRepository.GetUserById(request.UserID);
            if (user == null)
                return (Messages.UserMessages.UserNotFound, string.Empty);

            if (!user.IsInvoiceApplicable)
                return ("User is not eligible for invoice.", string.Empty);

            var nonGSTSetting = await _settingMasterRepository.GetByKey("InvoiceForNonGSTUser");
            bool allowNonGST = nonGSTSetting != null && nonGSTSetting.Value.Equals("Yes", StringComparison.OrdinalIgnoreCase);

            if (!allowNonGST && string.IsNullOrWhiteSpace(user.GSTNo))
                return ("User does not have a valid GST number.", string.Empty);

            string html;
            if (request.Type.Equals("ServiceWiseSale", StringComparison.OrdinalIgnoreCase))
                (_, html) = await GenerateServicewiseSaleInvoice(request, user);
            else if (request.Type.Equals("OperatorWiseSale", StringComparison.OrdinalIgnoreCase))
                (_, html) = await GenerateOperatorWiseSaleInvoice(request, user);
            else
                return (Messages.Common.Message.InvalidRequest, string.Empty);

            if (string.IsNullOrEmpty(html))
                return (Messages.Common.Message.DataNotFound, string.Empty);

            return (StatusType.Ok, html);
        }

        private async Task<(string failure, string html)> GenerateServicewiseSaleInvoice(
            SelectByTypeRequest request, FintechApp.Entities.UserMaster receiver)
        {
            var dateFrom = new DateTime(request.Year, request.Month, 1);
            var dateTo = new DateTime(request.Year, request.Month, DateTime.DaysInMonth(request.Year, request.Month));

            // Shared scoped DbContext — await sequentially (no concurrent EF queries).
            var allTxns = await _transactionMasterRepository.GetAll(0, StatusType.Success, "", dateFrom, dateTo, 1, int.MaxValue);
            var allOperators = await _operatorRepository.GetAllOperators();
            var allServices = await _serviceRepository.GetAllServices();
            var allSettings = await _invoiceSettingRepository.GetAll();
            var templateSetting = await _settingMasterRepository.GetByKey("SampleInvoice");

            var operatorDict = allOperators.ToDictionary(o => o.OperatorID);
            var serviceDict = allServices.ToDictionary(s => s.ServiceID);
            var settings = allSettings.FirstOrDefault();

            if (settings == null || templateSetting == null || string.IsNullOrEmpty(templateSetting.Value))
                return ("Invoice configuration not found.", string.Empty);

            var userTypeID = receiver.UserTypeID ?? 0;

            IEnumerable<FintechApp.Entities.TransactionMaster> filtered = allTxns;
            if (userTypeID != UserTypeID_Admin && userTypeID != UserTypeID_Support)
            {
                if (userTypeID == UserTypeID_SuperDist)
                    filtered = allTxns.Where(t => t.SuperDistributerUserID == request.UserID);
                else if (userTypeID == UserTypeID_Dist)
                    filtered = allTxns.Where(t => t.DistributerUserID == request.UserID);
                else
                    filtered = allTxns.Where(t => t.UserID == request.UserID);
            }

            var grouped = filtered
                .GroupBy(t => t.ServiceID)
                .Select(g =>
                {
                    var firstOpID = g.First().OperatorID;
                    var businessModel = operatorDict.TryGetValue(firstOpID, out var op) ? op.BusinessModel : string.Empty;
                    var rate = g.Sum(t => t.DebitAmount);
                    var taxable = businessModel == "P2A" ? 0m : (rate * 100) / 118;
                    var cgst = Math.Round(taxable * 9 / 100, 2);
                    var sgst = Math.Round(taxable * 9 / 100, 2);
                    return new ServicewiseSaleViewModel
                    {
                        ServiceID = g.Key,
                        OperatorID = firstOpID,
                        Type = businessModel,
                        Qty = g.Sum(t => t.Amount),
                        Rate = rate,
                        DiscountAmount = Math.Max(0, g.Sum(t => t.RetailerCommission)),
                        TaxableAmount = Math.Round(taxable, 2),
                        CGST = cgst,
                        SGST = sgst,
                        GSTTotal = cgst + sgst
                    };
                })
                .Where(x => serviceDict.ContainsKey(x.ServiceID))
                .Select(x =>
                {
                    var svc = serviceDict[x.ServiceID];
                    x.ServiceName = svc.Name;
                    x.HSN = svc.HSNCode;
                    return x;
                })
                .ToList();

            if (grouped.Count == 0)
                return (Messages.Common.Message.DataNotFound, string.Empty);

            decimal total = grouped.Sum(x => x.TaxableAmount);
            decimal gstTotal = grouped.Sum(x => x.GSTTotal);
            decimal invoiceTotal = total + gstTotal;

            string invoiceNo = GetInvoiceNumber(settings.CompanyPrefix, request.UserID, request.Year, request.Month, "SE");
            string tableHtml = BuildServicewiseTable(grouped);
            string html = GenerateInvoiceHtml(templateSetting.Value, invoiceNo, tableHtml, settings, receiver, total, gstTotal, invoiceTotal);

            return (StatusType.Ok, html);
        }

        private async Task<(string failure, string html)> GenerateOperatorWiseSaleInvoice(
            SelectByTypeRequest request, FintechApp.Entities.UserMaster receiver)
        {
            var dateFrom = new DateTime(request.Year, request.Month, 1);
            var dateTo = new DateTime(request.Year, request.Month, DateTime.DaysInMonth(request.Year, request.Month));

            // Shared scoped DbContext — await sequentially (no concurrent EF queries).
            var allTxns = await _transactionMasterRepository.GetAll(0, StatusType.Success, "", dateFrom, dateTo, 1, int.MaxValue);
            var allOperators = await _operatorRepository.GetAllOperators();
            var allSettings = await _invoiceSettingRepository.GetAll();
            var templateSetting = await _settingMasterRepository.GetByKey("SampleInvoice");

            var operatorDict = allOperators.ToDictionary(o => o.OperatorID);
            var settings = allSettings.FirstOrDefault();

            if (settings == null || templateSetting == null || string.IsNullOrEmpty(templateSetting.Value))
                return ("Invoice configuration not found.", string.Empty);

            var userTypeID = receiver.UserTypeID ?? 0;

            IEnumerable<FintechApp.Entities.TransactionMaster> filtered = allTxns;
            if (userTypeID != UserTypeID_Admin && userTypeID != UserTypeID_Support)
            {
                if (userTypeID == UserTypeID_SuperDist)
                    filtered = allTxns.Where(t => t.SuperDistributerUserID == request.UserID);
                else if (userTypeID == UserTypeID_Dist)
                    filtered = allTxns.Where(t => t.DistributerUserID == request.UserID);
                else
                    filtered = allTxns.Where(t => t.UserID == request.UserID);
            }

            var grouped = filtered
                .GroupBy(t => t.OperatorID)
                .Where(g => operatorDict.ContainsKey(g.Key))
                .Select(g =>
                {
                    var op = operatorDict[g.Key];
                    var rate = g.Sum(t => t.DebitAmount);
                    var taxable = op.BusinessModel == "P2A" ? 0m : (rate * 100) / 118;
                    var cgst = Math.Round(taxable * 9 / 100, 2);
                    var sgst = Math.Round(taxable * 9 / 100, 2);
                    return new OperatorWiseSaleViewModel
                    {
                        OperatorID = g.Key,
                        OperatorName = op.OperatorName,
                        Type = op.BusinessModel,
                        Qty = g.Sum(t => t.Amount),
                        Rate = rate,
                        DiscountAmount = Math.Max(0, g.Sum(t => t.RetailerCommission)),
                        TaxableAmount = Math.Round(taxable, 2),
                        CGST = cgst,
                        SGST = sgst,
                        GSTTotal = cgst + sgst
                    };
                })
                .ToList();

            if (grouped.Count == 0)
                return (Messages.Common.Message.DataNotFound, string.Empty);

            decimal total = grouped.Sum(x => x.TaxableAmount);
            decimal gstTotal = grouped.Sum(x => x.GSTTotal);
            decimal invoiceTotal = total + gstTotal;

            string invoiceNo = GetInvoiceNumber(settings.CompanyPrefix, request.UserID, request.Year, request.Month, "OP");
            string tableHtml = BuildOperatorwiseTable(grouped);
            string html = GenerateInvoiceHtml(templateSetting.Value, invoiceNo, tableHtml, settings, receiver, total, gstTotal, invoiceTotal);

            return (StatusType.Ok, html);
        }

        private string GetInvoiceNumber(string prefix, int userID, int year, int month, string type)
        {
            return prefix + type + userID.ToString("D4") + year.ToString("D4") + month.ToString("D2");
        }

        private string BuildServicewiseTable(List<ServicewiseSaleViewModel> items)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table class='table-bordered table'>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th scope='col' class='text-center'>DESC OF GOODS/SERVICES</th>");
            sb.AppendLine("<th scope='col' class='text-center'>HSN</th>");
            sb.AppendLine("<th scope='col' class='text-center'>Type</th>");
            sb.AppendLine("<th scope='col' class='text-center'>Qty</th>");
            sb.AppendLine("<th scope='col' class='text-center'>Unit</th>");
            sb.AppendLine("<th scope='col' class='text-center'>Rate</th>");
            sb.AppendLine("<th scope='col' class='text-center'>DiscountAmount</th>");
            sb.AppendLine("<th scope='col' class='text-center'>TaxableAmount</th>");
            sb.AppendLine("<th scope='col' class='text-center'>CGST</th>");
            sb.AppendLine("<th scope='col' class='text-center'>SGST</th>");
            sb.AppendLine("</tr></thead><tbody>");

            foreach (var item in items)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td class='text-right'>{item.ServiceName}</td>");
                sb.AppendLine($"<td class='text-right'>{item.HSN}</td>");
                sb.AppendLine($"<td class='text-right'>{item.Type}</td>");
                sb.AppendLine($"<td class='text-right'>{item.Qty}</td>");
                sb.AppendLine($"<td class='text-right'>{item.Unit}</td>");
                sb.AppendLine($"<td class='text-right'>{item.Rate}</td>");
                sb.AppendLine($"<td class='text-right'>{item.DiscountAmount}</td>");
                sb.AppendLine($"<td class='text-right'>{item.TaxableAmount}</td>");
                sb.AppendLine($"<td class='text-right'>{item.CGST}</td>");
                sb.AppendLine($"<td class='text-right'>{item.SGST}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("<tr><td colspan='8'> </td><td>Total:</td><td class='text-right'>[Total]</td></tr>");
            sb.AppendLine("<tr><td colspan='8'> </td><td>GST Total:</td><td class='text-right'>[GSTTotal]</td></tr>");
            sb.AppendLine("<tr><td colspan='8'> </td><td>Invoice Total:</td><td class='text-right'>[InvoiceTotal]</td></tr>");
            sb.AppendLine("</tbody></table>");
            return sb.ToString();
        }

        private string BuildOperatorwiseTable(List<OperatorWiseSaleViewModel> items)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table class='table-bordered table'>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th scope='col' class='text-center'>DESC OF GOODS/SERVICES</th>");
            sb.AppendLine("<th scope='col' class='text-center'>Type</th>");
            sb.AppendLine("<th scope='col' class='text-center'>Qty</th>");
            sb.AppendLine("<th scope='col' class='text-center'>Unit</th>");
            sb.AppendLine("<th scope='col' class='text-center'>Rate</th>");
            sb.AppendLine("<th scope='col' class='text-center'>DiscountAmount</th>");
            sb.AppendLine("<th scope='col' class='text-center'>TaxableAmount</th>");
            sb.AppendLine("<th scope='col' class='text-center'>CGST</th>");
            sb.AppendLine("<th scope='col' class='text-center'>SGST</th>");
            sb.AppendLine("</tr></thead><tbody>");

            foreach (var item in items)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{item.OperatorName}</td>");
                sb.AppendLine($"<td>{item.Type}</td>");
                sb.AppendLine($"<td>{item.Qty}</td>");
                sb.AppendLine($"<td>{item.Unit}</td>");
                sb.AppendLine($"<td>{item.Rate}</td>");
                sb.AppendLine($"<td>{item.DiscountAmount}</td>");
                sb.AppendLine($"<td>{item.TaxableAmount}</td>");
                sb.AppendLine($"<td>{item.CGST}</td>");
                sb.AppendLine($"<td>{item.SGST}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("<tr><td colspan='7'> </td><td>Total:</td><td>[Total]</td></tr>");
            sb.AppendLine("<tr><td colspan='7'> </td><td>GST Total:</td><td>[GSTTotal]</td></tr>");
            sb.AppendLine("<tr><td colspan='7'> </td><td>Invoice Total:</td><td>[InvoiceTotal]</td></tr>");
            sb.AppendLine("</tbody></table>");
            return sb.ToString();
        }

        private string GenerateInvoiceHtml(string template, string invoiceNo, string tableHtml,
            FintechApp.Entities.InvoiceSetting settings, FintechApp.Entities.UserMaster receiver,
            decimal total, decimal gstTotal, decimal invoiceTotal)
        {
            var html = template;
            html = html.Replace("[Logo]", settings.LogoURL);
            html = html.Replace("[TableInString]", tableHtml);
            html = html.Replace("[COMPANY-NAME]", settings.CompanyName);
            html = html.Replace("[Address]", settings.Address);
            html = html.Replace("[Title]", "TAX INVOICE");
            html = html.Replace("[DATE]", DateTime.Today.ToString("dd/MM/yyyy"));
            html = html.Replace("[MONTH]", DateTime.Today.ToString("MM"));
            html = html.Replace("[YEAR]", DateTime.Today.ToString("yyyy"));
            html = html.Replace("[INVOICENO]", invoiceNo);
            html = html.Replace("[Note]", settings.Note);
            html = html.Replace("[NOTE]", settings.Note);
            html = html.Replace("[BANK-NAME]", settings.BankName);
            html = html.Replace("[COMPANY-GSTIN]", settings.GSTIN);
            html = html.Replace("[COMPANY-State]", settings.State);
            html = html.Replace("[GSTIN]", settings.GSTIN);

            html = html.Replace("[RECEIVER-FIRMNAME]", string.IsNullOrEmpty(receiver.FirmName) ? "N/A" : receiver.FirmName);
            html = html.Replace("[RECEIVER-ADDRESS]", string.IsNullOrEmpty(receiver.Address) ? "N/A" : receiver.Address);
            html = html.Replace("[RECEIVER-CIN]", string.IsNullOrEmpty(receiver.CIN) ? "N/A" : receiver.CIN);
            html = html.Replace("[RECEIVER-PAN]", string.IsNullOrEmpty(receiver.PANNo) ? "N/A" : receiver.PANNo);
            html = html.Replace("[RECEIVER-GSTIN]", string.IsNullOrEmpty(receiver.GSTNo) ? "N/A" : receiver.GSTNo);
            html = html.Replace("[CONSIGNEE-FIRMNAME]", string.IsNullOrEmpty(receiver.FirmName) ? "N/A" : receiver.FirmName);
            html = html.Replace("[CONSIGNEE-ADDRESS]", string.IsNullOrEmpty(receiver.Address) ? "N/A" : receiver.Address);
            html = html.Replace("[CONSIGNEE-CIN]", string.IsNullOrEmpty(receiver.CIN) ? "N/A" : receiver.CIN);
            html = html.Replace("[CONSIGNEE-PAN]", string.IsNullOrEmpty(receiver.PANNo) ? "N/A" : receiver.PANNo);
            html = html.Replace("[CONSIGNEE-GSTIN]", string.IsNullOrEmpty(receiver.GSTNo) ? "N/A" : receiver.GSTNo);
            html = html.Replace("[IS-REVERSE-CHARGE-APPLICABLE]", "NO");

            html = html.Replace("[ACCOUNT-TYPE]", settings.AccountType);
            html = html.Replace("[ACCOUNTNO]", settings.AccountNo);
            html = html.Replace("[BRANCH-NAME]", settings.BranchName);
            html = html.Replace("[IFSC]", settings.IFSC);
            html = html.Replace("[COMPANY-PAN]", settings.PAN);
            html = html.Replace("[COMPANY-CIN]", settings.CIN);
            html = html.Replace("[WEBSITE_Name]", settings.WebsiteName);
            html = html.Replace("[WEBSITE]", settings.WebsiteName);
            html = html.Replace("[EmailID]", settings.Email);
            html = html.Replace("[SUPPORT-EMAIL]", settings.Email);
            html = html.Replace("[COMPANY-FULL-ADDRESS]", settings.FullAddress);
            html = html.Replace("[COMPANY-DISTRICT]", settings.District);
            html = html.Replace("[COUNTRY]", settings.Country);
            html = html.Replace("[PIN-CODE]", settings.PinCode);
            html = html.Replace("[COMPANY-SHORT-ADDRESS]", settings.ShortAddress);

            html = html.Replace("[Total]", total.ToString("F2"));
            html = html.Replace("[GSTTotal]", gstTotal.ToString("F2"));
            html = html.Replace("[InvoiceTotal]", invoiceTotal.ToString("F2"));
            html = html.Replace("[TOTAL-INVOICE-IN-WORD]", string.Empty);

            return html;
        }
    }
}
