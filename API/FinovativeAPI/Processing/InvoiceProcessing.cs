using DinkToPdf.Contracts;
using project.Response.Invoice;
using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Requests.Invoice;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Project.Processing
{
    public class InvoiceProcessing
    {
        private IConverter _converter;

        public InvoiceProcessing()
        {
        }

        public InvoiceProcessing(IConverter converter)
        {
            _converter = converter;
        }

        private readonly List<string> ServicewiseSaleRemoveColumns = new List<string> { "ServiceID", "OperatorID", "Total", "GSTTotal", "InvoiceTotal" };

        internal string SelectByType(SelectByTypeRequest mainRequest, out string failureMessage)
        {
            string path = string.Empty;

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            int UserID = mainRequest.UserID;

            if (!new UserBal().IsValidForInvoice(UserID, out failureMessage))
                return failureMessage;

            if (mainRequest.Type.EqualsIgnoreCase(InvoiceTypes.ServiceWiseSale))
                path = GenerateServicewiseSaleInvoice(mainRequest, out failureMessage);

            if (mainRequest.Type.EqualsIgnoreCase(InvoiceTypes.OperatorWiseSale))
                path = GenerateOperatorWiseSaleInvoice(mainRequest, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return path;

            if (mainRequest.IsDownload)
            {
                failureMessage = StatusType.Ok;
                return path;
            }

            if (mainRequest.IsSend)
                path = SendInvoiceMail(mainRequest.UserID, mainRequest.Type, path);

            failureMessage = StatusType.Ok;
            return Company.WebAPIURL + path;
        }

        private string GenerateOperatorWiseSaleInvoice(SelectByTypeRequest mainRequest, out string failureMessage)
        {
            failureMessage = "";
            string filename = "";
            List<Project.Response.Invoice.OperatorWiseSaleViewModel> listItem = null;

            try
            {
                listItem = new TransactionDAL().SelectOperatorwiseSale(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return "";
            }

            if ((listItem == null || listItem.Count <= 0))
            {
                failureMessage = Messages.DataNotFound;
                return filename;
            }
            decimal Total = 0, InvoiceTotal = 0, GSTTotal = 0;
            foreach (Project.Response.Invoice.OperatorWiseSaleViewModel item in listItem)
            {
                Total += item.TaxableAmount;
                GSTTotal += item.CGST;
                GSTTotal += item.SGST;
            }

            InvoiceTotal = Total + GSTTotal;

            string TotalInWords = NumberToWords.ConvertAmount(Convert.ToDouble(Total));

            string ServicewiseSaleInvoiceHTML = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SampleInvoice));

            if (string.IsNullOrEmpty(ServicewiseSaleInvoiceHTML)) return filename;

            InvoiceSetting settings = new InvoiceSettingDal().Select();

            if (settings == null) return "";

            string Title = "TAX INVOICE";

            string InvoiceNo = GetInvoiceNumber(settings.CompanyPrefix, mainRequest.UserID, mainRequest.Year, mainRequest.Month, "OP");

            string listItemInString = ConvertListToHTMLTableForOperatorWise(listItem, ServicewiseSaleRemoveColumns);

            UserMaster receiver = new UserDAL().SelectByIDFromCache(mainRequest.UserID);

            ServicewiseSaleInvoiceHTML = GenerateInVoiceHTML(ServicewiseSaleInvoiceHTML, InvoiceNo,
                listItemInString, settings, Title, Total, InvoiceTotal, GSTTotal, receiver, TotalInWords);

            //try
            //{
            //    filename = new PDFProcessing(_converter).GeneratePDF(Title, ServicewiseSaleInvoiceHTML, InvoiceNo, "Invoice");
            //}
            //catch (Exception ex)
            //{
            //    new Logger().Write(ex);
            //}
            failureMessage = StatusType.Ok;
            // return filename;
            return ServicewiseSaleInvoiceHTML;
        }

        private string SendInvoiceMail(int userID, string Type, string path)
        {
            string EmailID = new UserDAL().SelectEmailID(userID);
            if (string.IsNullOrEmpty(EmailID))
                return UserMessage.EmailNotAdded;

            Util util = new Util();
            string subject = Type + "-";
            subject += util.GetLeadingZeroNumber(DateTime.Now.Year, 4) + "-";
            subject += util.GetLeadingZeroNumber(DateTime.Now.Month, 2);

            MessageProcessing.SendEmail(EmailID, "Hi Please check attachment", subject, path);

            return InvoiceMessages.SentSuccess;
        }

        private string GenerateServicewiseSaleInvoice(SelectByTypeRequest mainRequest, out string failureMessage)
        {
            failureMessage = "";
            string filename = "";
            List<Project.Response.Invoice.ServicewiseSaleViewModel> listItem = null;

            try
            {
                listItem = new TransactionDAL().SelectServicewiseSale(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = TransactionMessage.RecordNotFound;
            if ((listItem == null || listItem.Count <= 0)) return filename;

            decimal Total = 0, InvoiceTotal = 0, GSTTotal = 0;
            foreach (Project.Response.Invoice.ServicewiseSaleViewModel item in listItem)
            {
                Total += item.TaxableAmount;
                GSTTotal += item.CGST;
                GSTTotal += item.SGST;
            }

            InvoiceTotal = Total + GSTTotal;

            string TotalInWords = NumberToWords.ConvertAmount(Convert.ToDouble(Total));

            string ServicewiseSaleInvoiceHTML = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SampleInvoice));

            if (string.IsNullOrEmpty(ServicewiseSaleInvoiceHTML)) return filename;

            InvoiceSetting settings = new InvoiceSettingDal().Select();

            if (settings == null) return "";

            string Title = "TAX INVOICE";

            string InvoiceNo = GetInvoiceNumber(settings.CompanyPrefix, mainRequest.UserID, mainRequest.Year, mainRequest.Month, "SE");

            UserMaster receiver = new UserDAL().SelectByIDFromCache(mainRequest.UserID);

            string listItemInString = ConvertListToHTMLTableForServiceWise(listItem, ServicewiseSaleRemoveColumns);

            ServicewiseSaleInvoiceHTML = GenerateInVoiceHTML(ServicewiseSaleInvoiceHTML, InvoiceNo, listItemInString, settings,
                Title, Total, InvoiceTotal, GSTTotal, receiver, TotalInWords);

            //temporary comment
            //try
            //{
            //    filename = new PDFProcessing(_converter).GeneratePDF(Title, ServicewiseSaleInvoiceHTML, InvoiceNo, "Invoice");
            //}
            //catch (Exception ex)
            //{
            //    new Logger().Write(ex);
            //}

            failureMessage = StatusType.Ok;
            //return Company.WebAPIURL + filename;
            return ServicewiseSaleInvoiceHTML;
        }

        private string GenerateInVoiceHTML(string InvoiceHTML, string InvoiceNo, string listItemInString, InvoiceSetting settings,
            string Title, decimal Total, decimal InvoiceTotal, decimal GSTTotal, UserMaster receiver, string TotalInWords)
        {
            InvoiceHTML = InvoiceHTML.Replace("[Logo]", Company.WebAPIURL + settings.LogoURL);
            InvoiceHTML = InvoiceHTML.Replace("[TableInString]", listItemInString);
            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-NAME]", settings.CompanyName);
            InvoiceHTML = InvoiceHTML.Replace("[Address]", settings.Address);
            InvoiceHTML = InvoiceHTML.Replace("[Title]", Title);
            InvoiceHTML = InvoiceHTML.Replace("[DATE]", DateTime.Today.Date.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture));
            InvoiceHTML = InvoiceHTML.Replace("[MONTH]", DateTime.Today.Date.ToString(Messages.OnlyMonthFormat, CultureInfo.InvariantCulture));
            InvoiceHTML = InvoiceHTML.Replace("[YEAR]", DateTime.Today.Date.ToString(Messages.OnlyYearFormat, CultureInfo.InvariantCulture));
            InvoiceHTML = InvoiceHTML.Replace("[INVOICENO]", InvoiceNo);
            InvoiceHTML = InvoiceHTML.Replace("[Note]", settings.Note);
            InvoiceHTML = InvoiceHTML.Replace("[BANK-NAME]", settings.BankName);
            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-GSTIN]", settings.GSTIN);
            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-State]", settings.State);

            InvoiceHTML = InvoiceHTML.Replace("[GSTIN]", settings.GSTIN);
            InvoiceHTML = InvoiceHTML.Replace("[TOTAL-INVOICE-IN-WORD]", TotalInWords);

            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-FIRMNAME]", receiver.FirmName);
            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-ADDRESS]", receiver.Address);

            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-CIN]", string.IsNullOrEmpty(receiver.CIN) ? "N/A:" : receiver.Address);

            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-PAN]", string.IsNullOrEmpty(receiver.PANNo) ? "N/A:" : receiver.PANNo);

            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-GSTIN]", string.IsNullOrEmpty(receiver.GSTNo) ? "N/A:" : receiver.GSTNo);

            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-FIRMNAME]", string.IsNullOrEmpty(receiver.FirmName) ? "N/A:" : receiver.FirmName);

            InvoiceHTML = InvoiceHTML.Replace("[RECEIVER-ADDRESS]", string.IsNullOrEmpty(receiver.Address) ? "N/A:" : receiver.Address);

            InvoiceHTML = InvoiceHTML.Replace("[CONSIGNEE-CIN]", string.IsNullOrEmpty(receiver.CIN) ? "N/A:" : receiver.Address);

            InvoiceHTML = InvoiceHTML.Replace("[CONSIGNEE-PAN]", string.IsNullOrEmpty(receiver.PANNo) ? "N/A:" : receiver.PANNo);

            InvoiceHTML = InvoiceHTML.Replace("[CONSIGNEE-GSTIN]", string.IsNullOrEmpty(receiver.GSTNo) ? "N/A:" : receiver.GSTNo);

            InvoiceHTML = InvoiceHTML.Replace("[CONSIGNEE-FIRMNAME]", string.IsNullOrEmpty(receiver.FirmName) ? "N/A:" : receiver.FirmName);

            InvoiceHTML = InvoiceHTML.Replace("[CONSIGNEE-ADDRESS]", string.IsNullOrEmpty(receiver.Address) ? "N/A:" : receiver.Address);

            string ReverseRechargeApplicable = receiver.IsReverseRechargeApplicable ? "YES" : "NO";

            InvoiceHTML = InvoiceHTML.Replace("[IS-REVERSE-CHARGE-APPLICABLE]", ReverseRechargeApplicable);

            InvoiceHTML = InvoiceHTML.Replace("[ACCOUNT-TYPE]", settings.AccountType);
            InvoiceHTML = InvoiceHTML.Replace("[ACCOUNTNO]", settings.AccountNo);
            InvoiceHTML = InvoiceHTML.Replace("[BRANCH-NAME]", settings.BranchName);
            InvoiceHTML = InvoiceHTML.Replace("[IFSC]", settings.IFSC);

            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-PAN]", settings.PAN);
            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-CIN]", settings.CIN);
            InvoiceHTML = InvoiceHTML.Replace("[WEBSITE_Name]", settings.WebsiteName);
            InvoiceHTML = InvoiceHTML.Replace("[EmailID]", settings.Email);
            InvoiceHTML = InvoiceHTML.Replace("[SUPPORT-EMAIL]", settings.Email);

            InvoiceHTML = InvoiceHTML.Replace("[WEBSITE]", settings.WebsiteName);
            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-FULL-ADDRESS]", settings.FullAddress);

            InvoiceHTML = InvoiceHTML.Replace("[InvoiceTotal]", InvoiceTotal.ToString());
            InvoiceHTML = InvoiceHTML.Replace("[GSTTotal]", GSTTotal.ToString());
            InvoiceHTML = InvoiceHTML.Replace("[Total]", Total.ToString());

            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-DISTRICT]", settings.District);
            InvoiceHTML = InvoiceHTML.Replace("[COUNTRY]", settings.Country);

            InvoiceHTML = InvoiceHTML.Replace("[PIN-CODE]", settings.PinCode);
            InvoiceHTML = InvoiceHTML.Replace("[COMPANY-SHORT-ADDRESS]", settings.ShortAddress);

            InvoiceHTML = InvoiceHTML.Replace("[NOTE]", settings.Note);

            return InvoiceHTML;
        }

        internal string GetInvoiceNumber(string CompnayPrefix, int UserID, int Year, int Month, string Type)
        {
            Util util = new Util();
            string InvoiceNo = "[CompnayPrefix][Type][FourDigitUserID][FourDigitYear][TwoDigitMonth]";
            InvoiceNo = InvoiceNo.Replace("[CompnayPrefix]", CompnayPrefix);
            InvoiceNo = InvoiceNo.Replace("[Type]", Type);
            InvoiceNo = InvoiceNo.Replace("[FourDigitUserID]", util.GetLeadingZeroNumber(UserID, 4));
            InvoiceNo = InvoiceNo.Replace("[FourDigitYear]", util.GetLeadingZeroNumber(Year, 4));
            InvoiceNo = InvoiceNo.Replace("[TwoDigitMonth]", util.GetLeadingZeroNumber(Month, 2));

            return InvoiceNo;
        }

        private string ConvertListToHTMLTableForServiceWise<T>(List<T> models, List<string> RemoveColumns)
        {
            //Get all the properties of that model
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<table class='table-bordered table'> ");
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
                    sb.AppendLine("<th scope='col' class='text-center'>DESC OF GOODS/SERVICES</th>");
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

            //for Total
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td colspan='{ColSpan}'>  </td> ");

            sb.AppendLine("<td> Total:</td>");
            sb.AppendLine("<td class='text-right'> [Total]</td>");

            sb.AppendLine("</tr>");

            // for gst total
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td colspan='{ColSpan}'>  </td> ");

            sb.AppendLine("<td> GST Total:</td>");
            sb.AppendLine("<td class='text-right'> [GSTTotal]</td>");

            sb.AppendLine("</tr>");

            // for invoice  total
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td colspan='{ColSpan}'>  </td> ");

            sb.AppendLine("</td>");
            sb.AppendLine("<td> Invoice Total:</td>");
            sb.AppendLine("<td class='text-right'> [InvoiceTotal]</td>");

            sb.AppendLine("</tr>");

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        private string ConvertListToHTMLTableForOperatorWise<T>(List<T> models, List<string> RemoveColumns)
        {
            // creating a data table instance and typed it as our incoming model

            //Get all the properties of that model
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<table class='table-bordered table'> ");
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
                if (prop.Name.EqualsIgnoreCase("OperatorName"))
                {
                    sb.AppendLine("<th scope='col' class='text-center'>DESC OF GOODS/SERVICES</th>");
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

            foreach (T item in models)
            {
                sb.AppendLine("<tr>");
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);

                    if (RemoveColumns.Contains(Props[i].Name))
                        continue;

                    sb.AppendLine("<td>" + values[i] + "</td>");
                }
                sb.AppendLine("</tr>");
            }
            int ColSpan = (colCount - 2);

            //for Total
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td colspan='{ColSpan}'>  </td> ");

            sb.AppendLine("<td> Total:</td>");
            sb.AppendLine("<td> [Total]</td>");

            sb.AppendLine("</tr>");

            // for gst total
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td colspan='{ColSpan}'>  </td> ");

            sb.AppendLine("<td> GST Total:</td>");
            sb.AppendLine("<td> [GSTTotal]</td>");

            sb.AppendLine("</tr>");

            // for invoice  total
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td colspan='{ColSpan}'>  </td> ");

            sb.AppendLine("</td>");
            sb.AppendLine("<td> Invoice Total:</td>");
            sb.AppendLine("<td> [InvoiceTotal]</td>");

            sb.AppendLine("</tr>");

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        internal List<ReportViewModel> Select(SelectRequest mainRequest, out string failureMessage)
        {
            List<ReportViewModel> response = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            failureMessage = UserMessage.UserNotFound;
            try
            {
                response = new UserDAL().SelectForInvoice(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response == null) return response;

            foreach (ReportViewModel res in response)
            {
                res.ServiceWise.UserID = res.UserID;
                res.ServiceWise.Month = mainRequest.Month;
                res.ServiceWise.Year = mainRequest.Year;

                res.OperatorWise.UserID = res.UserID;
                res.OperatorWise.Month = mainRequest.Month;
                res.OperatorWise.Year = mainRequest.Year;
            }

            if (mainRequest.UserID == -99)
            {
                failureMessage = StatusType.Ok;
                return response;
            }
            failureMessage = StatusType.Ok;
            return response;
        }
    }
}