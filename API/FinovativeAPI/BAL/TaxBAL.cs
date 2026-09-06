using DinkToPdf.Contracts;
using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.Tax;
using Project.Response.Tax;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Project.BAL
{
    public class TaxBAL
    {
        private readonly List<string> GSTRemoveColumnsPrepaidAndUtility = new List<string> { "Discount", "GSTType", "GSTTaxableValue", "GSTNo", "InvoiceNo", "State", "GSTTotal", "InvoiceTotal", "InvoiceMonth", "FirmName" };
        private readonly List<string> GSTRemoveColumnsDMT = new List<string> { "Discount", "GSTType", "DiscountAmount", "GSTNo", "InvoiceNo", "State", "GSTTotal", "InvoiceTotal", "InvoiceMonth", "FirmName" };

        private IConverter _converter;

        public TaxBAL()
        {
        }

        public TaxBAL(IConverter converter)
        {
            _converter = converter;
        }

        internal TDSReportWrapper SelectTDS(TDSReportRequest selectRequest, int userID, out string failureMessage)
        {
            TDSReportWrapper wrapper = new TDSReportWrapper();
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);
            isExcelOrPDF(selectRequest);
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<TDSReportViewModel> list = new List<TDSReportViewModel>();
            try
            {
                list = new TransactionReportDAL().SelectTDSByUserID(selectRequest, userID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;

            wrapper.Count = list.Count;
            if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.Tax.TDSReportRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }
            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        internal GSTDetailReportWrapper SelectGSTDetailReportForAdmin(GSTDetailReportRequest selectRequest, int userID, out string failureMessage)
        {
            GSTDetailReportWrapper wrapper = new GSTDetailReportWrapper();
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<GSTDetailReportViewModel> list = new List<GSTDetailReportViewModel>();
            try
            {
                list = new TransactionReportDAL().SelectGSTDetailReportForAdmin(selectRequest, userID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;

            decimal SuccessAmt = 0;
            int SuccessCount = 0;
            decimal GST = 0;
            decimal Commission = 0;

            foreach (GSTDetailReportViewModel item in list)
            {
                SuccessAmt += item.SuccessAmt;
                SuccessCount += item.SuccessCount;
                GST += item.GST;
                Commission += item.Commission;
            }

            GSTDetailReportViewModel totalItem = new GSTDetailReportViewModel();
            totalItem.FirmName = "Total";
            totalItem.SuccessAmt = SuccessAmt;
            totalItem.SuccessCount = SuccessCount;
            totalItem.GST = GST;
            totalItem.Commission = Commission;
            list.Add(totalItem);

            wrapper.Count = list.Count;
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal GSTDetailWrapper SelectGSTDetail(GSTDetailRequest selectRequest, int userID, out string failureMessage)
        {
            GSTDetailWrapper wrapper = new GSTDetailWrapper();
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
                selectRequest.UserID = userID;

            int selectUserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);
            if (selectUserTypeID == (int)UserType.Distributer || selectUserTypeID == (int)UserType.SuperDistributer)
                selectRequest.InvoiceType = TaxInfo.SelfInvoice;

            if (selectRequest.InvoiceType == null || selectRequest.InvoiceType.Length <= 1)
            {
                if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser || UserTypeID == (int)UserType.Client)
                    selectRequest.InvoiceType = TaxInfo.InVoice;
            }

            failureMessage = selectRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<GSTDetailViewModel> list = new List<GSTDetailViewModel>();

            if (selectRequest.UserID > 0 || !Util.isRoleAdminOrSupport(UserTypeID))
            {
                bool IsGenerated = new InvoiceNumberHistoryBal().CheckIFInvoiceGeneratedForthisMonth(selectRequest);
                if (!IsGenerated)
                {
                    int SelectedUserID = selectRequest.UserID;
                    int LoginUserID = userID;
                    string InvoiceType = selectRequest.InvoiceType;

                    selectRequest.UserID = 0;
                    selectRequest.InvoiceType = TaxInfo.SelfInvoice;
                    userID = Company.AdminUserID;
                    GetGSTDetailsForALL(selectRequest, list, userID, out failureMessage);

                    selectRequest.InvoiceType = TaxInfo.InVoice;
                    GetGSTDetailsForALL(selectRequest, list, userID, out failureMessage);
                    selectRequest.InvoiceType = InvoiceType;
                    selectRequest.UserID = SelectedUserID;
                    userID = LoginUserID;
                }

                list = GetGSTDetailsForALL(selectRequest, list, userID, out failureMessage);
            }

            if (selectRequest.UserID <= 0)
            {
                list = GetGSTDetailsForALL(selectRequest, list, userID, out failureMessage);
            }

            list = list.AsQueryable().WhereDynamic(selectRequest.Search).ToList();
            wrapper.Count = list.Count;
            if (!selectRequest.IsExcelDownload)
            {
                list = list.Skip(selectRequest.SkipCount).Take(selectRequest.TakeCount).ToList();
            }
            else
            {
                list = list.ToList();
            }

            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            wrapper.list = list;

            if (list == null || list.Count <= 0) { failureMessage = Messages.DataNotFound; return wrapper; }
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal GSTDetailWrapper SelectGSTDetailByTransaction(GSTDetailRequest selectRequest, int userID, out string failureMessage)
        {
            GSTDetailWrapper wrapper = new GSTDetailWrapper();
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
                selectRequest.UserID = userID;

            int selectUserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);
            if (selectUserTypeID == (int)UserType.Distributer || selectUserTypeID == (int)UserType.SuperDistributer)
                selectRequest.InvoiceType = TaxInfo.SelfInvoice;

            if (selectRequest.InvoiceType == null || selectRequest.InvoiceType.Length <= 1)
            {
                if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser || UserTypeID == (int)UserType.Client)
                    selectRequest.InvoiceType = TaxInfo.InVoice;
            }

            failureMessage = selectRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<GSTDetailViewModel> list = new List<GSTDetailViewModel>();

            if (selectRequest.UserID > 0 || !Util.isRoleAdminOrSupport(UserTypeID))
            {
                bool IsGenerated = new InvoiceNumberHistoryBal().CheckIFInvoiceGeneratedForthisMonth(selectRequest);
                if (!IsGenerated)
                {
                    int SelectedUserID = selectRequest.UserID;
                    int LoginUserID = userID;
                    string InvoiceType = selectRequest.InvoiceType;

                    selectRequest.UserID = 0;
                    selectRequest.InvoiceType = TaxInfo.SelfInvoice;
                    userID = Company.AdminUserID;
                    GetGSTDetailsByTransactionForALL(selectRequest, list, userID, out failureMessage);

                    selectRequest.InvoiceType = TaxInfo.InVoice;
                    GetGSTDetailsByTransactionForALL(selectRequest, list, userID, out failureMessage);
                    selectRequest.InvoiceType = InvoiceType;
                    selectRequest.UserID = SelectedUserID;
                    userID = LoginUserID;
                }

                list = GetGSTDetailsByTransactionForALL(selectRequest, list, userID, out failureMessage);
            }

            if (selectRequest.UserID <= 0)
            {
                list = GetGSTDetailsByTransactionForALL(selectRequest, list, userID, out failureMessage);
            }

            list = list.AsQueryable().WhereDynamic(selectRequest.Search).ToList();
            wrapper.Count = list.Count;
            if (!selectRequest.IsExcelDownload)
            {
                list = list.Skip(selectRequest.SkipCount).Take(selectRequest.TakeCount).ToList();
            }
            else
            {
                list = list.ToList();
            }

            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            wrapper.list = list;

            if (list == null || list.Count <= 0) { failureMessage = Messages.DataNotFound; return wrapper; }
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal List<GSTDetailViewModel> GetGSTDetailsForALL(GSTDetailRequest selectRequest, List<GSTDetailViewModel> list, int userID, out string failureMessage)
        {
            try
            {
                list = new TaxDal().SelectGSTDetail(selectRequest, userID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }

            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return list;

            list = GetInvoiceNumberSequentially(selectRequest, list);

            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<GSTDetailViewModel> GetGSTDetailsByTransactionForALL(GSTDetailRequest selectRequest, List<GSTDetailViewModel> list, int userID, out string failureMessage)
        {
            try
            {
                list = new TaxDal().SelectGSTDetailByTransaction(selectRequest, userID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }

            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return list;

            list = GetInvoiceNumberSequentially(selectRequest, list);

            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<GSTDetailViewModel> GetInvoiceNumberSequentially(GSTDetailRequest selectRequest, List<GSTDetailViewModel> list)
        {
            InvoiceSetting settings = new InvoiceSettingDal().Select();

            string FinancialYear = GetFinancialYear(selectRequest.Year, selectRequest.Month);

            int IncrementValue = new InvoiceNumberHistoryDal().SelectLastIncrementValue(selectRequest, Convert.ToInt32(FinancialYear));

            IncrementValue += 1;
            foreach (GSTDetailViewModel item in list)
            {
                item.ServiceWise.UserID = item.UserID;
                item.ServiceWise.Month = selectRequest.Month;
                item.ServiceWise.Year = selectRequest.Year;
                item.ServiceWise.ServiceCategory = item.ServiceCategory;
                string InvoiceNo = GetInvoiceNumber(settings.CompanyPrefix, item.UserID, selectRequest.Year, selectRequest.Month, IncrementValue, selectRequest.InvoiceType);
                InvoiceNo = new InvoiceNumberHistoryBal().InsertAndGetInvoiceNumberFromHistory(selectRequest, item.UserID, InvoiceNo, item.ServiceCategory, IncrementValue, Convert.ToInt32(FinancialYear));
                item.ServiceWise.InvoiceNo = InvoiceNo;
                item.InvoiceNo = InvoiceNo;
                IncrementValue++;
            }

            return list;
        }

        internal string SelectGSTCategoryWise(SelectByCategoryRequest selectRequest, out string failureMessage)
        {
            failureMessage = "";
            string filename = "";
            GSTDetailWrapper wrapper = new GSTDetailWrapper();
            failureMessage = selectRequest.IsValidModalState();

            int UserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            List<GSTDetailInvoiceViewModel> list = new List<GSTDetailInvoiceViewModel>();
            try
            {
                list = new TaxDal().SelectGSTCategoryWise(selectRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return failureMessage;
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0) return failureMessage;

            decimal Total = 0, TaxableTotal = 0, InvoiceTotal = 0, InvoiceValue = 0, GSTTotal = 0, GSTTotalRoundOff = 0;

            string GSTType = GSTTypes.Inclusion;
            foreach (GSTDetailInvoiceViewModel item in list)
            {
                if (item.ServiceName.EqualsIgnoreCase("REMITTANCE"))
                {
                    item.GST = 0;
                    item.CGST = 0;
                    item.IGST = 0;
                    item.SGST = 0;
                    item.Taxable = 0;
                }
                if (!item.ServiceName.EqualsIgnoreCase("REMITTANCE"))
                {
                    item.CCF = 0M;
                }
                GSTTotal += item.GST;
                TaxableTotal += item.Taxable;
                GSTType = item.GSTType;

                if (item.ServiceName.EqualsIgnoreCase("Money Transfer"))
                {
                    item.ServiceName = "Payout charge";
                    if (UserTypeID == (int)UserType.Distributer || UserTypeID == (int)UserType.SuperDistributer)
                        item.ServiceName = "Commission (Payout)";
                }
                if (item.ServiceName.EqualsIgnoreCase("UPITransfer"))
                {
                    item.ServiceName = "UPI";
                    //if (UserTypeID == (int)UserType.Distributer || UserTypeID == (int)UserType.SuperDistributer)
                    //    item.ServiceName = "UPI";
                }

            }

            InvoiceValue = GSTTotal + TaxableTotal;

            GSTTotalRoundOff = GSTTotal;
            InvoiceTotal = Total + GSTTotal;

            string InvoiceTotalinwords = NumberToWords.ConvertAmount(Convert.ToDouble(InvoiceValue));
            string TotalInWords = NumberToWords.ConvertAmount(Convert.ToDouble(GSTTotalRoundOff));

            string TaxableTotalinwords = "";
            if (GSTType.EqualsIgnoreCase(GSTTypes.Inclusion))
            {
                GSTType = "Incl.";
                TaxableTotalinwords = NumberToWords.ConvertAmount(Convert.ToDouble(TaxableTotal));
            }

            if (GSTType.EqualsIgnoreCase(GSTTypes.Exclusion))
            {
                GSTType = "Excl.";
                TaxableTotalinwords = NumberToWords.ConvertAmount(Convert.ToDouble(TaxableTotal + GSTTotal));
            }

            string GSTInvoiceHTML = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.GSTInvoice));

            if (string.IsNullOrEmpty(GSTInvoiceHTML)) return filename;
            InvoiceSetting settings = new InvoiceSettingDal().Select();

            if (settings == null) return "";
            string Title = "GST INVOICE";

            string InvoiceNo = selectRequest.InvoiceNo;

            UserMaster receiver = new UserDAL().SelectByIDFromCache(selectRequest.UserID);

            StateMaster receverstate = new StateDAL().SelectByID(receiver.StateID);

            UserMaster owner = new UserDAL().SelectByIDFromCache(Company.AdminUserID);

            string listItemInString = "";

            if (selectRequest.ServiceCategory.EqualsIgnoreCase(TaxInfo.DMT))
                listItemInString = ConvertListToHTMLTableForServiceWise(list, GSTRemoveColumnsDMT, TaxableTotal);

            if (!selectRequest.ServiceCategory.EqualsIgnoreCase(TaxInfo.DMT))
                listItemInString = ConvertListToHTMLTableForServiceWise(list, GSTRemoveColumnsPrepaidAndUtility, TaxableTotal);

            GSTInvoiceHTML = GenerateInVoiceHTML(GSTInvoiceHTML, InvoiceNo, listItemInString, settings,
                Title, Total, InvoiceTotal, GSTTotal, GSTTotalRoundOff, InvoiceValue, receiver, TotalInWords, owner, receverstate.Name, GSTType, selectRequest, TaxableTotalinwords, InvoiceTotalinwords);

            failureMessage = StatusType.Ok;
            return GSTInvoiceHTML;
        }

        internal string SelectGSTCategoryWiseV2(SelectByCategoryRequest selectRequest, out string failureMessage)
        {
            failureMessage = "";
            string filename = "";
            GSTDetailWrapper wrapper = new GSTDetailWrapper();
            failureMessage = selectRequest.IsValidModalState();

            int UserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            List<GSTDetailInvoiceViewModel> list = new List<GSTDetailInvoiceViewModel>();
            try
            {
                list = new TaxDal().SelectGSTCategoryWiseV2(selectRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return failureMessage;
            }

            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0) return failureMessage;

            decimal Total = 0, TaxableTotal = 0, InvoiceTotal = 0, InvoiceValue = 0, GSTTotal = 0, GSTTotalRoundOff = 0;

            string GSTType = GSTTypes.Inclusion;
            foreach (GSTDetailInvoiceViewModel item in list)
            {
                GSTTotal += item.GST;
                TaxableTotal += item.Taxable;
                GSTType = item.GSTType;
            }

            InvoiceValue = GSTTotal + TaxableTotal;

            GSTTotalRoundOff = GSTTotal;

            InvoiceTotal = Total + GSTTotal;

            string InvoiceTotalinwords = NumberToWords.ConvertAmount(Convert.ToDouble(InvoiceValue));

            string TotalInWords = NumberToWords.ConvertAmount(Convert.ToDouble(GSTTotalRoundOff));

            string TaxableTotalinwords = "";

            if (GSTType.EqualsIgnoreCase(GSTTypes.Inclusion))
            {
                GSTType = "Incl.";
                TaxableTotalinwords = NumberToWords.ConvertAmount(Convert.ToDouble(TaxableTotal));
            }

            if (GSTType.EqualsIgnoreCase(GSTTypes.Exclusion))
            {
                GSTType = "Excl.";
                TaxableTotalinwords = NumberToWords.ConvertAmount(Convert.ToDouble(TaxableTotal + GSTTotal));
            }

            string GSTInvoiceHTML = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.GSTInvoice));

            if (string.IsNullOrEmpty(GSTInvoiceHTML)) return filename;

            InvoiceSetting settings = new InvoiceSettingDal().Select();

            if (settings == null) return "";

            string Title = "GST INVOICE";

            string InvoiceNo = selectRequest.InvoiceNo;

            UserMaster receiver = new UserDAL().SelectByIDFromCache(selectRequest.UserID);

            StateMaster receverstate = new StateDAL().SelectByID(receiver.StateID);

            UserMaster owner = new UserDAL().SelectByIDFromCache(Company.AdminUserID);

            string listItemInString = "";

            if (selectRequest.ServiceCategory.EqualsIgnoreCase(TaxInfo.P2ABankingRCM))
                listItemInString = ConvertListToHTMLTableForServiceWise(list, GSTRemoveColumnsDMT, TaxableTotal);

            if (!selectRequest.ServiceCategory.EqualsIgnoreCase(TaxInfo.P2ABankingRCM))
                listItemInString = ConvertListToHTMLTableForServiceWise(list, GSTRemoveColumnsPrepaidAndUtility, TaxableTotal);

            GSTInvoiceHTML = GenerateInVoiceHTML(GSTInvoiceHTML, InvoiceNo, listItemInString, settings,
                Title, Total, InvoiceTotal, GSTTotal, GSTTotalRoundOff, InvoiceValue, receiver, TotalInWords, owner, receverstate.Name, GSTType, selectRequest, TaxableTotalinwords, InvoiceTotalinwords);

            failureMessage = StatusType.Ok;
            return GSTInvoiceHTML;
        }

        internal string GetInvoiceNumber(string CompnayPrefix, int UserID, int Year, int Month, int SequenceNumber, string InvoiceType)
        {
            if (CompnayPrefix.Length > 2)
            {
                CompnayPrefix = CompnayPrefix.Substring(0, 2);
            }
            if (InvoiceType.EqualsIgnoreCase(TaxInfo.SelfInvoice))
                CompnayPrefix = CompnayPrefix + "C";

            if (InvoiceType.EqualsIgnoreCase(TaxInfo.InVoice))
                CompnayPrefix = CompnayPrefix + "P";

            Util util = new Util();

            string InvoiceNo = "[CompnayPrefix][FourDigitYear][FourDigitSequenceNumber]";
            InvoiceNo = InvoiceNo.Replace("[CompnayPrefix]", CompnayPrefix);
            InvoiceNo = InvoiceNo.Replace("[FourDigitUserID]", util.GetLeadingZeroNumber(UserID, 4));
            InvoiceNo = InvoiceNo.Replace("[FourDigitYear]", GetFinancialYear(Year, Month));
            InvoiceNo = InvoiceNo.Replace("[FourDigitSequenceNumber]", util.GetLeadingZeroNumber(SequenceNumber, 5));
            return InvoiceNo;
        }

        internal string GetFinancialYear(int Year, int Month)
        {
            string FinancialYearstring = "";
            DateTime dateTime = new DateTime(Year, Month, 1);
            string FinancialYear = (dateTime.Month >= 4 ? dateTime.AddYears(1).ToString("yy") : dateTime.ToString("yy"));
            string LastYear = (Convert.ToInt32(FinancialYear) - 1).ToString();
            FinancialYearstring = LastYear + FinancialYear;
            return FinancialYearstring;
        }

        private string GenerateInVoiceHTML(string InvoiceHTML, string InvoiceNo, string listItemInString, InvoiceSetting settings,
                string Title, decimal Total, decimal InvoiceTotal, decimal GSTTotal, decimal GSTTotalRoundOff, decimal InvoiceValue,
                UserMaster receiver, string TotalInWords, UserMaster owner, string receverstate, string GSTType, SelectByCategoryRequest selectRequest, string TaxableTotalinwords, string InvoiceTotalinwords)
        {
            string receiverFirmName = "";
            string receiverAddress = "";
            string receiverPincode = "";
            string receiverCIN = "";
            string receiverPANNo = "";
            string receiverGSTNo = "";
            string receiverAliasName = "";
            string receiverUserID = "";
            bool receiverIsReverseRechargeApplicable = false;
            string ownerPANNo = "";
            string ownerGSTNo = "";
            string ownerFirmName = "";
            string ownerAddress = "";
            string CompanyName = "";
            string settingsPAN = "";
            string settingsCIN = "";
            string settingsGSTIN = "";
            string settingsState = "";
            string settingsPinCode = "";
            string settingsShortAddress = "";
            string settingDistrict = "";
            string settingsEmail = "";
            string settingsBankName = "";
            string settingsAccountType = "Current Account";
            string settingsIFSC = "";
            string settingsAccountNo = "";
            string settingsBranchName = "";
            string settingsFullAddress = "";
            string LogoURL = Company.WebAPIURL + settings.LogoURL;
            string SignatureURL = Company.WebAPIURL + settings.SignatureURL;

            if (receiver.UserTypeID == (int)UserType.Retailer || receiver.UserTypeID == (int)UserType.APIUser)
            {
                receiverFirmName = receiver.FirmName;
                receiverAddress = receiver.Address;
                receiverPincode = receiver.Pincode;
                receiverCIN = receiver.CIN;
                receiverPANNo = receiver.PANNo;
                receiverGSTNo = receiver.GSTNo;
                receiverAliasName = receiver.AliasName;
                receiverUserID = receiver.UserID.ToString();
                receiverIsReverseRechargeApplicable = receiver.IsReverseRechargeApplicable;

                ownerPANNo = owner.PANNo;
                ownerGSTNo = owner.GSTNo;
                ownerFirmName = owner.FirmName;
                ownerAddress = owner.Address;
                CompanyName = settings.CompanyName;
                settingsPAN = settings.PAN;
                settingsCIN = settings.CIN;
                settingsGSTIN = settings.GSTIN;
                settingsState = settings.State;
                settingsPinCode = settings.PinCode;
                settingsShortAddress = settings.ShortAddress;
                settingDistrict = settings.District;
                settingsEmail = settings.Email;
                settingsBankName = settings.BankName;
                settingsAccountType = settings.AccountType;
                settingsIFSC = settings.IFSC;
                settingsAccountNo = settings.AccountNo;
                settingsBranchName = settings.BranchName;
                settingsFullAddress = settings.FullAddress;
                LogoURL = LogoURL;
                SignatureURL = SignatureURL;
            }
            if (receiver.UserTypeID == (int)UserType.Distributer || receiver.UserTypeID == (int)UserType.SuperDistributer)
            {
                StateMaster state = new StateDAL().SelectByID(receiver.StateID);

                receiverFirmName = settings.CompanyName;
                receiverAddress = owner.Address;
                receiverPincode = owner.Pincode;
                receiverCIN = owner.CIN;
                receiverPANNo = owner.PANNo;
                receiverGSTNo = owner.GSTNo;
                receiverAliasName = receiver.AliasName;
                receiverUserID = owner.UserID.ToString();
                receiverIsReverseRechargeApplicable = owner.IsReverseRechargeApplicable;

                ownerPANNo = receiver.PANNo;
                ownerGSTNo = receiver.GSTNo;
                ownerFirmName = receiver.FirmName;
                ownerAddress = receiver.Address;
                settingsFullAddress = receiver.Address;

                CompanyName = receiver.FirmName;

                settingsPAN = receiver.PANNo;
                settingsCIN = receiver.CIN;
                settingsGSTIN = receiver.GSTNo;
                settingsState = state.Name;
                settingsPinCode = receiver.Pincode;
                settingsShortAddress = receiver.Address;
                settingsEmail = receiver.EmailID;

                LogoURL = "";
                SignatureURL = "";

                settingDistrict = "";
            }

            InvoiceHTML = InvoiceHTML.Replace("[Logo]", LogoURL);
            InvoiceHTML = InvoiceHTML.Replace("[SignatureURL]", SignatureURL);

            InvoiceHTML = InvoiceHTML.Replace("[TableInString]", listItemInString);

            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-NAME]", CompanyName);
            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-NAME-ADMIN]", settings.CompanyName);
            InvoiceHTML = InvoiceHTML.Replace("[Address]", settings.Address);
            InvoiceHTML = InvoiceHTML.Replace("[Title]", Title);
            InvoiceHTML = InvoiceHTML.Replace("[DATE]", DateTime.Today.Date.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture));

            DateTime Date = new DateTime(selectRequest.Year, selectRequest.Month, 1);

            DateTime last = Date.LastOfMonth();
            int LastDayOfMonth = Convert.ToInt32(last.ToString("dd"));

            string month = LastDayOfMonth.ToString() + ',' + Date.ToString("MMMM");
            InvoiceHTML = InvoiceHTML.Replace("[MONTH]", month);
            InvoiceHTML = InvoiceHTML.Replace("[YEAR]", selectRequest.Year.ToString());
            InvoiceHTML = InvoiceHTML.Replace("[INVOICENO]", InvoiceNo);
            InvoiceHTML = InvoiceHTML.Replace("[Note]", settings.Note);

            InvoiceHTML = InvoiceHTML.Replace("[BANK-NAME]", settingsBankName);
            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-GSTIN]", settingsGSTIN);
            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-GSTIN-ADMIN]", settings.GSTIN);

            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-State]", settingsState);

            InvoiceHTML = InvoiceHTML.Replace("[GSTIN]", settingsGSTIN);
            InvoiceHTML = InvoiceHTML.Replace("[TOTAL-INVOICE-IN-WORD]", TotalInWords);

            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-FIRMNAME]", receiverFirmName);
            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-ADDRESS]", receiverAddress);

            InvoiceHTML = InvoiceHTML.Replace("[RECEIVERPIN-CODE]", receiverPincode);
            if (receiverCIN == null || receiverCIN.Length <= 4)
            {
                InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-CIN]", "");
                InvoiceHTML = InvoiceHTML.Replace("[CONSIGNEE-CIN]", "");
            }
            else
            {
                InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-CIN]", "CIN: " + receiverCIN);
                InvoiceHTML = InvoiceHTML.Replace("[CONSIGNEE-CIN]", "CIN: " + receiverCIN);
            }
            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-PAN]", string.IsNullOrEmpty(receiverPANNo) ? "N/A:" : receiverPANNo);

            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-GSTIN]", string.IsNullOrEmpty(receiverGSTNo) ? "N/A:" : receiverGSTNo);

            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-FIRMNAME]", string.IsNullOrEmpty(receiverFirmName) ? "N/A:" : receiverFirmName);

            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-ADDRESS]", string.IsNullOrEmpty(receiverAddress) ? "N/A:" : receiverAddress);

            InvoiceHTML = InvoiceHTML.Replace("[CONSIGNEE-PAN]", string.IsNullOrEmpty(receiverPANNo) ? "N/A:" : ownerPANNo);

            InvoiceHTML = InvoiceHTML.Replace("[CONSIGNEE-GSTIN]", string.IsNullOrEmpty(receiverGSTNo) ? "N/A:" : ownerGSTNo);

            InvoiceHTML = InvoiceHTML.Replace("[CONSIGNEE-FIRMNAME]", string.IsNullOrEmpty(receiverFirmName) ? "N/A:" : ownerFirmName);

            InvoiceHTML = InvoiceHTML.Replace("[CONSIGNEE-ADDRESS]", string.IsNullOrEmpty(receiverAddress) ? "N/A:" : ownerAddress);

            string ReverseRechargeApplicable = receiverIsReverseRechargeApplicable ? "YES" : "NO";

            InvoiceHTML = InvoiceHTML.Replace("[IS-REVERSE-CHARGE-APPLICABLE]", ReverseRechargeApplicable);

            InvoiceHTML = InvoiceHTML.Replace("[ACCOUNT-TYPE]", settingsAccountType);
            InvoiceHTML = InvoiceHTML.Replace("[ACCOUNTNO]", settingsAccountNo);
            InvoiceHTML = InvoiceHTML.Replace("[BRANCH-NAME]", settingsBranchName);
            InvoiceHTML = InvoiceHTML.Replace("[IFSC]", settingsIFSC);

            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-PAN]", settingsPAN);
            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-PAN-ADMIN]", settings.PAN);

            if (settings.CIN == null || settings.CIN.Length <= 4)
                InvoiceHTML = InvoiceHTML.Replace("[COMPANY-CIN-ADMIN]", "");
            else
                InvoiceHTML = InvoiceHTML.Replace("[COMPANY-CIN-ADMIN]", "CIN: " + settings.CIN);

            if (settingsCIN == null || settingsCIN.Length <= 4)
                InvoiceHTML = InvoiceHTML.Replace("[COMPANY-CIN]", "");
            else
                InvoiceHTML = InvoiceHTML.Replace("[COMPANY-CIN]", "CIN: " + settingsCIN);

            InvoiceHTML = InvoiceHTML.Replace("[WEBSITE_Name]", settings.WebsiteName);
            InvoiceHTML = InvoiceHTML.Replace("[EmailID]", settingsEmail);
            InvoiceHTML = InvoiceHTML.Replace("[SUPPORT-EMAIL]", settingsEmail);

            InvoiceHTML = InvoiceHTML.Replace("[WEBSITE]", settings.WebsiteName);
            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-FULL-ADDRESS]", settingsFullAddress);

            //  InvoiceHTML = InvoiceHTML.Replace("[InvoiceTotal]", InvoiceTotal.ToString());

            InvoiceHTML = InvoiceHTML.Replace("[TotalInvoiceValueInWord]", InvoiceTotalinwords);
            InvoiceHTML = InvoiceHTML.Replace("[TotalGSTValueInWord]", TotalInWords);

            InvoiceHTML = InvoiceHTML.Replace("[AmountChargeableInWords]", TaxableTotalinwords);
            InvoiceHTML = InvoiceHTML.Replace("[GSTTotal]", GSTTotal.ToString());
            InvoiceHTML = InvoiceHTML.Replace("[GSTTotalRoundOff]", GSTTotalRoundOff.ToString());
            //InvoiceHTML = InvoiceHTML.Replace("[TotalTaxable]", TaxableTotal.ToString());
            InvoiceHTML = InvoiceHTML.Replace("[InvoiceValue]", InvoiceValue.ToString());

            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-DISTRICT]", settingDistrict);
            InvoiceHTML = InvoiceHTML.Replace("[COUNTRY]", settings.Country);

            InvoiceHTML = InvoiceHTML.Replace("[PIN-CODE]", settingsPinCode);
            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-SHORT-ADDRESS]", settingsShortAddress);

            InvoiceHTML = InvoiceHTML.Replace("[NOTE]", settings.Note);
            InvoiceHTML = InvoiceHTML.Replace("[GSTType]", GSTType);
            InvoiceHTML = InvoiceHTML.Replace("[PlaceofSupply]", receverstate);
            InvoiceHTML = InvoiceHTML.Replace("[InvoiceSeller]", CompanyName);

            string DealerCode = "";

            if (receiverAliasName == null)
            {
                DealerCode = receiverUserID.ToString();
                receiverAliasName = "";
            }

            if (receiverAliasName.Length <= 2)
                DealerCode = receiverUserID.ToString();

            if (receiverAliasName.Length > 2)
                DealerCode = receiverAliasName.ToString();

            InvoiceHTML = InvoiceHTML.Replace("[DealerCode]", DealerCode);

            return InvoiceHTML;
        }

        private string ConvertListToHTMLTableForServiceWise<T>(List<T> models, List<string> RemoveColumns, decimal TaxableTotal)
        {
            //Get all the properties of that model
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<table class='table-bordered table innertable' style='width: 100%;  table-layout: fixed;'> ");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            // Adding Column name to our datatable

            int colCount = 0;
            foreach (PropertyInfo prop in Props)
            {
                if (RemoveColumns.Contains(prop.Name))
                {
                    continue;
                }
                colCount++;

                if (prop.Name.EqualsIgnoreCase("ServiceName"))
                {
                    sb.AppendLine("<th scope='col' class='text-center'>SERVICE(S)</th>");
                }
                else if (prop.Name.EqualsIgnoreCase("CashBack"))
                {
                    sb.AppendLine("<th scope='col'  class='text-center'>Comm</th>");
                }
                else if (prop.Name.EqualsIgnoreCase("IGST"))
                {
                    sb.AppendLine("<th scope='col' class='text-center'>IGST @18%</th>");
                }
                else if (prop.Name.EqualsIgnoreCase("CGST"))
                {
                    sb.AppendLine("<th scope='col' class='text-center'>CGST @9%</th>");
                }
                else if (prop.Name.EqualsIgnoreCase("SGST"))
                {
                    sb.AppendLine("<th scope='col' class='text-center'>SGST @9%</th>");
                }
                else if (prop.Name.EqualsIgnoreCase("Transaction"))
                {
                    sb.AppendLine("<th scope='col'  class='text-center'>Transaction Value</th>");
                }
                else if (prop.Name.EqualsIgnoreCase("Taxable"))
                {
                    sb.AppendLine("<th scope='col'  class='text-center'>Taxable Amount</th>");
                }

                else
                {
                    sb.AppendLine("<th scope='col' class='text-center'>" + prop.Name + "</th>");
                }
            }
            //Setting column names as Property names

            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");

            sb.AppendLine("<tbody>");
            int ColSpan = (colCount - 2);
            int ColSpan1 = (colCount - 6);
            // Adding Row and its value to our dataTable

            foreach (T item in models)
            {
                sb.AppendLine("<tr >");
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);

                    if (RemoveColumns.Contains(Props[i].Name))
                        continue;

                    sb.AppendLine("<td class='text-right'>" + values[i] + "</td>");
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tbody>");

            sb.AppendLine("<tfoot>");

            // for  total

            //sb.AppendLine("<tr>");
            //sb.AppendLine($"<td colspan='{ColSpan1}'>  </td> ");
            //sb.AppendLine("<td style='font-weight: bold;'>Total Taxable: </td>");
            //sb.AppendLine("<td class='text-right'  style='font-weight: bold;'  >[TotalTaxable]</td>");

            //sb.AppendLine($"<td colspan='{2}'>  </td> ");

            //sb.AppendLine("<td style='font-weight: bold;'>GST ([GSTType]): </td>");
            //sb.AppendLine("<td class='text-right'  style='font-weight: bold;'  >[GSTTotal]</td>");

            //sb.AppendLine("</tr>");

            //  for round total

            sb.AppendLine("<tr>");

            sb.AppendLine($"<td  colspan='{ColSpan}'>  <span style=' font-style: italic;' > Total GST Value In Word:--- </span><span style='font-weight: bold;' >[TotalGSTValueInWord] </span></td> ");

            sb.AppendLine("<td style='font-weight: bold;'> GST:</td>");
            sb.AppendLine("<td class='text-right'  style='font-weight: bold;'  > [GSTTotalRoundOff]</td>");

            sb.AppendLine("</tr>");

            //  for invoice total

            sb.AppendLine("<tr>");

            sb.AppendLine($"<td colspan='{ColSpan}'><span style=' font-style: italic;' > Total Invoice Value In Word:---</span><span style='font-weight: bold;' > [TotalInvoiceValueInWord]</span> </td> ");

            sb.AppendLine("<td style='font-weight: bold;'>Invoice Value:</td>");
            sb.AppendLine("<td class='text-right'  style='font-weight: bold;'  > [InvoiceValue]</td>");

            sb.AppendLine("</tr>");

            sb.AppendLine("</tfoot>");

            sb.AppendLine("</table>");
            return sb.ToString();
        }
    }
}