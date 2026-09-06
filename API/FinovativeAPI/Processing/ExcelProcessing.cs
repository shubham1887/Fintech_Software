using ClosedXML.Excel;
using Project.DAL;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Project.Processing
{
    internal class RenameColumn
    {
        public string OldName { get; set; }
        public string NewName { get; set; }
    }

    public class ExcelProcessing
    {
        private readonly List<string> RemoveColumnsDMTRetailer = new List<string> { "APIUserRequestID", "DistributerCommission", "ComplaintStatus", "ProviderName", "SuperDistributerCommission", "GST", "AdminCommission", "Date", "DistributorName", "SuperDistributorName" };
        private readonly List<string> RemoveColumnsRechargeRetailer = new List<string> { "APIUserRequestID", "DistributorName", "DistributerCommission", "SuperDistributerCommission", "GST", "AdminCommission", "ResponseType", "ROfferCommission", "ProviderName" };
        private readonly List<string> RemoveColumnsUPIRetailer = new List<string> { "Date", "UserMobile", "SenderNo", "SenderName", "BeneficaryNameByBank", "TDS", "CashBack", "BankName", "IFSC", "DistributorName", "Bank", "APIUserRequestID", "DistributerCommission", "SuperDistributerCommission", "AdminCommission", "ProviderName", "GST" };
        private readonly List<string> RemoveColumnsPaymentGatewayRetailer = new List<string> { "DOC", "Response", "GatewayCharge", "TotalAmount", "ProviderName", "DistributerCommission", "SuperDistributerCommission", "SuperDistributorName" };

        private readonly List<string> RemoveColumnsForClient = new List<string> { };
        private readonly List<string> RemoveColumnsRechargeClient = new List<string> { "CommissionInfo", "DistributorName", "ProviderName", "APIUserRequestID", "RetailerCommission", "DistributerCommission", "SuperDistributerCommission", "GST", "AdminCommission", "ResponseType", "ROfferCommission" };
        private readonly List<string> RemoveColumnsComplaintClient = new List<string> { "ProviderName", "SupportNumber", "ProviderRequestID", "StringAmount" };
        private readonly List<string> RemoveColumnsDayBookClient = new List<string> { "SuperDistributerCommission", "DistributerCommission", "AdminCommission" };
        private readonly List<string> RemoveColumnsPaymentGatewayClient = new List<string> { "Response", "DOC", "ProviderName", "DistributerCommission", "SuperDistributerCommission", "SuperDistributorName", "DistributorName", "ProviderName" };

        private readonly List<string> RemoveColumnsForRetailerAndAPIUser = new List<string> { "ServiceID", "ProviderName", "ComplaintStatus", "SuperDistributorName", "DistributerCommission", "SuperDistributerCommission", "GST", "AdminCommission", "CallbackResponse", "ProviderID", "DCommission", "SCommission", "DistributerGST", "DistributerTDS", "SuperDistributerGST", "SuperDistributerTDS" };
        private readonly List<string> ProviderlogRemoveColumns = new List<string> { "DistributerCommission", "SuperDistributerCommission", "ProviderID", "Logo", "ServiceID", "CallbackResponse", "ConsumerNumber", "ProviderRequestID", "TransactionID", "ProviderReply" };

        private readonly List<string> ProviderCallbacklogRemoveColumns = new List<string> { "Request", "Response", "CallbackResponse", "ConsumerNumber", "ProviderRequestID", "ServiceID", "ProviderID", "TransactionID" };

        private readonly List<string> ReferalReportRemoveColumns = new List<string> { "ReferralRegistrationID", "CreatedByUserID", "BenefitUtilizedDOC", "IsBenefitUtilizeStr" };
        private readonly List<string> AccountOpeningReportRemoveColumns = new List<string> { "DistributerCommission", "SuperDistributerCommission", "ProviderID", "Logo", "SuperDistributorName", "DistributorName", "ConsumerNumber", "SenderName", "BeneficaryNameByBank", "SenderNo", "BeneName", "Bank", "ComplaintStatus", "ReferenceNo", "PaymentType", "AmountInfo", "BalanceInfo", "ProviderInfo", "APIBalInfo", "Name", "Operator", "TxnID", "ResponseType", "ProviderRefNo" };

        private readonly List<string> RemoveColumnsForAdmin = new List<string> { };

        private readonly List<string> RemoveColumnsRechargeReportAdmin = new List<string> { "DebitAmount", "Logo", "Through", "ResponseType" };
        private readonly List<string> RemoveColumnsUserDownlineAdmin = new List<string> { "Balance", "HeaderSummary" };

        private readonly List<string> RemoveColumnsWalleTopUpAdmin = new List<string> {"Date", "UserMobile", "PaymentType", "Operator", "Bank", "ComplaintStatus", "APIUserRequestID", "Circle", "RetailerCommission", "ResponseType", "DistributerCommission", "SuperDistributerCommission", "Through", "ProviderClosingBalance", "GST",
        "ProviderCommission","Name","TxnID","AmountInfo","BalanceInfo","ProviderInfo","APIBalInfo","AdminCommission","AdminProfit","ProviderRefNo","ROfferCommission","BeneficaryNameByBank","CommissionInfo"};

        private readonly List<string> RemoveColumnsDistributer = new List<string> { "SuperDistributerCommission", "Balance", "ProviderID", "Logo", "APIUserRequestID", "ProviderName", "ProviderCommission", "AdminCommission", "AdminProfit", "GST", "ProviderRefNo", "SCommission", "GST", "TDS", "SuperDistributerGST", "SuperDistributerTDS", "SuperDistributorName" };

        private readonly List<string> RemoveColumnsSuperDistributer = new List<string> { "ProviderID", "Logo", "APIUserRequestID", "ProviderName", "ProviderCommission", "AdminCommission", "AdminProfit", "GST", "ProviderRefNo", "GST", "TDS", "DistributerGST", "DistributerTDS" };

        private readonly List<string> SlabRemoveColumns = new List<string> { "FirmName", "SlabDetailID", "SlabName", "FromAmount","ToAmount", "UserID", "IsActive", "RealRetailerCommissionCategory", "StrRetailerComm", "ServiceID", "OfflineProductMasterID", "PackageName", "SERVICE", "OperatorID","SlabMasterID ",
        "Logo","OPERATOR","IsDelete","StrSuperDistributorComm","StrDistributorComm","StrClientComm","RealRetailerComm","RealRetailerCommType","RealDistributorComm","RealDistributorCommType","RealDistributorCommissionCategory","RealSuperDistributorComm",
        "RealSuperDistributorCommType","RealSuperDistributorCommissionCategory", "RealClientComm","RealClientCommType","RealClientCommissionCategory","SlabMasterID"};

        private readonly List<string> MessageSettingRemoveColumns = new List<string> { "MessageSettingID", "IsMail", "EmailMessage" };

        private readonly List<string> UserFilterRemoveColumns = new List<string> { "CreatedUserID", "UserCreationMasterID", "ClosingBalance" };

        private readonly List<string> RechargeReportRemoveColumns = new List<string> {"UserID", "OperatorID", "ServiceID","ProviderRefNo","ProviderCommission","OriginalDOC","Type",
            "TransactionMasterID", "ProviderID","SenderNo","SenderName", "BeneficaryNameByBank",  "BeneName", "Bank" ,"PaymentType", "WalletID","ProviderClosingBalance", "IFSC",
            "BankName","Name","Operator","ReportPath","AmountInfo","BalanceInfo","Date","ProviderInfo","APIBalInfo","TxnID","UpdatedDOC","Description","SuperDistributorName"};
        
        
        private readonly List<string> BBPSReportRemoveColumns = new List<string> {"UserID", "OperatorID", "ServiceID","ProviderRefNo","ProviderCommission","OriginalDOC","Type",
            "TransactionMasterID", "ProviderID","SenderNo","SenderName", "BeneficaryNameByBank",  "BeneName", "Bank" ,"PaymentType", "WalletID","ProviderClosingBalance", "IFSC",
            "BankName","Name","Operator","ReportPath","AmountInfo","BalanceInfo","Date","ProviderInfo","APIBalInfo","TxnID","UpdatedDOC","Description","SuperDistributorName","BBPSLogo","Longitude","Latitude","OrignalFirmName",
        "Circle","ProcTime","IPAddress","SenderNo","SenderName","BeneName","BeneficaryNameByBank","BankName","IFSC","Bank","APIUserRequestID","ProviderName","BalanceInfo","ProviderInfo","APIBalInfo","AdminCommission","AdminProfit","ResponseType","ROfferCommission","Response",
        "Logo","FirmName","CommissionInfo","ComplaintStatus","IsBBPS"};



        private readonly List<string> DMTReportRemoveColumns = new List<string> {"UserID", "OperatorID", "ServiceID","Date",
           "ReportPath","AdminProfit","ResponseType","APIBalInfo","ProviderRefNo","Operator","UpdatedDOC","ProviderCommission","Circle","AmountInfo","BalanceInfo","TransactionMasterID","ProcTime","WalletID", "Logo" ,"ProviderClosingBalance",  "CommissionInfo",

           "ROfferCommission","ProviderInfo","TxnID","Name","Description","Longitude","Latitude","SuccessCount","ProviderID","IPAddress","CashBack","SuperDistributorName","DistributorName","APIUserRequestID","DistributerGST","DistributerTDS",
        "SuperDistributerGST","SuperDistributerTDS","RetailerCommPercentage","AdminCommission","Through","Remark"};

        private readonly List<string> SenderReportRemoveColumns = new List<string> {"UserID","IsOTPVerified", "OperatorID", "ServiceID", "APIUserRequestID",
        "IsOTPVerifiedStr","IsNameVerifiedStr",  "GUID","BeneficiaryID","SenderID", "ReportPath","AdminProfit","ResponseType","APIBalInfo","ProviderRefNo","Operator","UpdatedDOC","ProviderCommission","Circle","AmountInfo","BalanceInfo","TransactionMasterID", "ProviderID", "ProviderRequestID","ProviderName","ProcTime","WalletID", "Logo" ,"ProviderClosingBalance",  "CommissionInfo" };

        private readonly List<string> SaleReportRemoveColumns = new List<string> {"UserID","Commission","PendingAmount","PendingCount", "OperatorID", "ServiceID", "APIUserRequestID",
            "TransactionMasterID", "ProviderID", "ProviderRequestID","ProviderName","ProcTime","WalletID", "Logo" ,"ProviderClosingBalance",  "CommissionInfo" };

        private readonly List<string> GSTReportRemoveColumns = new List<string> {"UserID","StateID","SequenceNumber","UserTypeID", "OperatorID", "ServiceID", "APIUserRequestID",
            "TransactionMasterID", "ProviderID", "ProviderRequestID","ProviderName","ProcTime","WalletID", "Logo" ,"ProviderClosingBalance",  "CommissionInfo","MobileNumber","ServiceName","GST","ServiceCategory" };

        private readonly List<string> PayoutReportRemoveColumns = new List<string> {"UserID", "OperatorID", "ServiceID", "APIUserRequestID",
            "TransactionMasterID","AmountInfo","BalanceInfo","ProviderInfo","APIBalInfo","AdminProfit","ResponseType","ProviderRefNo","SuperDistributorName","DistributorName","Circle","ProviderCommission","ProviderID", "ProviderRequestID","ProviderName","ProcTime","WalletID", "Logo" ,"ProviderClosingBalance",  "CommissionInfo",
        "UserMobile","UpdatedDOC","Date","Name","Operator","TxnID","ROfferCommission"};

        private readonly List<string> SmartPayReportRemoveColumns = new List<string> {"UserID", "OperatorID", "ServiceID", "APIUserRequestID",
            "TransactionMasterID","AmountInfo","BalanceInfo","ProviderInfo","APIBalInfo","AdminProfit","ResponseType","ProviderRefNo","SuperDistributorName","DistributorName","Circle","ProviderCommission","ProviderID", "ProviderRequestID","ProcTime","WalletID", "Logo" ,"ProviderClosingBalance",  "CommissionInfo",
        "UserMobile","UpdatedDOC","Date","Name","Operator","TxnID","ROfferCommission"};

        private readonly List<string> KYCReportRemoveColumns = new List<string> { "DocumentType", "DocumentNo", "Document", "WhatsAppNumber", "CompactResponse", "Label", "CompactResponse", "UserDocumentID", "SampleImageURL", "UserID" };

        private readonly List<string> IncomeReportRemoveColumns = new List<string> {"UserID", "OperatorID", "ServiceID", "APIUserRequestID",
            "TransactionMasterID","Commission","ProviderID", "ProviderRequestID","ProviderName","ProcTime","WalletID", "Logo" ,"ProviderClosingBalance",  "CommissionInfo" };

        private readonly List<string> DayBookReportRemoveColumns = new List<string> {"UserID", "OperatorID", "ServiceID", "APIUserRequestID",
            "TransactionMasterID", "ProviderID", "ProviderRequestID","ProcTime","WalletID", "Logo" ,"ProviderClosingBalance",  "CommissionInfo" };

        private readonly List<string> ComplaintReportRemoveColumns = new List<string> { "Service", "Logo", "NewConsumerNumber", "SupportNumber", "ProviderRequestID", "StringAmount", "ComplaintMasterID", "TransactionMasterID", "UserID", "UpdatedDOC", "OperatorID" };

        private readonly List<string> CommonRemoveColumns = new List<string> { "UserID", "AmountCreditDebit", "DOCType", "TransactionDetailID", "WalletID", "OperatorID", "ServiceID", "ProviderID", "TransactionID", "UpdatedDOC", "Logo" };

        
        
        
        
        private readonly List<string> AepsRemoveColumns = new List<string> { "UserID", "AdminProfit", "ProviderCommission", "SuperDistributorName", "DistributorName", "SuperDistributerComm", "DistributerComm", "TransactionDetailID", "WalletID", "OperatorID", "ServiceID", "ProviderID", "VirtualBalanceID", "UpdatedDOC", "Logo", "ProviderName",
        "Through","IPAddress","ComplaintStatus","UserNumber","Latitude","Longitude","CustomerNumber","TransactionMasterID","FirmName"};


        /*private readonly List<string> AepsRemoveColumns = new List<string> { "UserID", "AdminProfit", "ProviderCommission", "SuperDistributorName", "DistributorName", "SuperDistributerComm", "DistributerComm", "TransactionDetailID", "WalletID", "OperatorID", "ServiceID", "ProviderID", "VirtualBalanceID", "UpdatedDOC", "Logo", "IPAddress", "ComplaintStatus",
              "DistributerGST","DistributerTDS","SuperDistributerGST","SuperDistributerTDS","Latitude","Longitude","CustomerNumber","ProviderName","BankName","Through"};
*/



        private readonly List<string> VoucherReportColumns = new List<string> { "VoucherMasterID", "VoucherTypeID", "Status", "DOCInt","DOC", "Date", "UpdatedDOC","Date","SubmitedBy", "ApproveBy", "Is_Delete" };
        private readonly List<string> VoucherColumns = new List<string> { "VoucherMasterID", "VoucherTypeID", "DOCInt","DOC", "Date", "UpdatedDOC","Date","SubmitedBy", "ApproveBy", "ApproveByName", "Is_Delete" };
        
        private readonly List<string> AccountRemoveColumns = new List<string> { "UserID", "ProviderRefNo", "PayMode", "GST", "TDS", "CommissionCreditDebit", "TransactionAmount", "AssignedUserID", "TransactionDetailID", "WalletID", "OperatorID", "ServiceID", "ProviderID", "VirtualBalanceID", "UpdatedDOC", "Logo" };

        private readonly List<string> FundRemoveColumns = new List<string> { "UserID", "SenderName", "SenderFirmName", "SenderID", "TransactionDetailID", "WalletID", "ReceiptPath", "ReceiverID", "SenderBalance", "ProviderID",
         "Through", "AdminRemark", "ReceiverName" ,"ReceiverFirmName", "ReceiverMobile", "SenderMobile", "TxnID"};

        private readonly List<string> UserRemoveColumns = new List<string> {"UserID", "UserTypeID", "ParentID", "StateID", "ProfileImagePath","WalletIDforEcollection","StaticQRWalletID", "TotalRechargeBalance","TotalDMTBalance","TotalAEPSBalance",
            "UserServiceList", "Balances","BalanceCappings","Details", "IsKYCApproved", "IsLoginOTP", "IsWeb", "IsApp", "IsInvoiceApplicable", "AllowRealAPI", "IsChannel", "IsTwoFA","ProviderID", "WalletID", "IsActive","ResponseType",
        "ParentName","SuperParentName"};

        private readonly List<string> OfflineBookRemoveColumns = new List<string> { "UserID", "TransactionDetailID", "OfflineBookingDetailsID", "OfflineProductMasterID", "ProviderID", "WalletID" };
        private readonly List<string> PaymentGatewayRemoveColumns = new List<string> { "PaymentGatewayID", "ParentID", "Sender", "SuperDistributerUserID", "DistributerUserID", "TotalCount", "CompanyName", "UserID", "WalletID", "ProviderID", "WalletID" };
        private readonly List<string> PurchaseRemoveColumns = new List<string> { "LapuPartyMasterID", "LapuTransactionDetailID", "OperatorID" };

        private readonly List<string> CommiRemoveColumns = new List<string> { "UserID","ParentID", "FirmName", "IsActive", "RealRetailerComm", "RealRetailerCommType", "RealRetailerCommissionCategory" ,
        "RealDistributorComm","RealDistributorCommType","RealDistributorCommissionCategory","RealSuperDistributorComm","RealSuperDistributorCommType","RealSuperDistributorCommissionCategory",
        "RealClientComm","RealClientCommType","RealClientCommissionCategory","IsDelete", "OperatorID", "ServiceID","SlabMasterID","SlabDetailID","RetailerComm"
            ,"RetailerCommType","RetailerCommissionCategory","DistributorComm","DistributorCommType","DistributorCommissionCategory","SuperDistributorComm","SuperDistributorCommType","SuperDistributorCommissionCategory","ClientComm",
        "ClientCommType","ClientCommissionCategory","PackageName","OfflineProductMasterID","StrDistributorComm","StrSuperDistributorComm","StrClientComm"};

        private readonly List<string> CommissionHoldReportColumns = new List<string> { "Year", "Month", "AdminRemark", "UserRemark", "UserID", "RedeemcommissionID", "WalletID", "WalletID", "UpdatedDOC" };
        private readonly List<string> SubscriptionChargeRemoveColumns = new List<string> { "UserTypeID", "PaymentDate", "IsAutoDebit", "ServiceAndUserwiseSubscriptionID", "UserID", "ServiceID", "WalletID", "WalletID", "UpdatedDOC" };
        private readonly List<string> SubscriptionPaymentRemoveColumns = new List<string> { "IsAutoDebitStr", "UserType", "DOC", "UserTypeID", "IsAutoDebit", "ServiceAndUserwiseSubscriptionID", "UserID", "ServiceID", "WalletID", "WalletID", "UpdatedDOC" };
        private readonly List<string> GrowthLossAnalysisRemoveColumns = new List<string> { "DematGrowthLossMasterID", "ServiceSubscriptionPlanMasterID", "APIProviderId", "ServiceID", "ServiceAndUserwiseSubscriptionID", "UserID", "DOCInt", "WalletID", "UpdatedDOC" };
        private readonly List<string> OpratorWiseUserWiseRemoveColumns = new List<string> { "MobileSaleID","DTHSaleID","PaymentGatewaySaleID","AepsSaleID","MAtmID","MoneyTransferID","ElectricityID",
            "UserID","ServiceID","ElectricityID","PostPaidID", "CreditCardID","GeneralInsuranceID","CMSID","FasTagID","BroadbandID","GasCylinderBookingID","WaterBillID","PipedGasID","PayoutID","POSID","UPITransferID","ExpressMoneyTransferID"
        };

        private readonly List<string> OperatorWiseUserRemoveColumns = new List<string> { "TransactionMasterID", "UserMobile", "ConsumerNumber", "ROfferCommission","DistributerCommission", "SuperDistributerCommission", "ProviderName", "BeneName", "OpeningBalance","DebitAmount","ClosingBalance",
            "ReferenceNo","Description","ProviderRequestID","Date","IFSC","PaymentType","SenderNo","SenderName","BeneficaryNameByBank","CommissionInfo","ServiceID","ProviderID","WalletID","ResponseType",
            "TDS","CashBack","BankName","IPAddress","ProcTime","SuperDistributorName","DistributorName","Logo","Bank","ComplaintStatus","APIUserRequestID","Circle","Through","ProviderClosingBalance",
            "GST","DistributerGST","DistributerTDS","SuperDistributerGST","SuperDistributerTDS","ProviderCommission","UpdatedDOC","Name","Operator","TxnID","AmountInfo","BalanceInfo","ProviderInfo","APIBalInfo","AdminCommission","AdminProfit","ProviderRefNo","Remark","Latitude","Longitude"};
        private readonly List<string> OnboardingUserRemoveColumns = new List<string> { "OnboardingUserID", "ReferenceID", "SupportUserID", "SupportVerify", "AdminVerify", "SlabID", "list" };
        private readonly List<RenameColumn> renameColumnsList = new List<RenameColumn>
        {
            new RenameColumn{OldName="DOC",NewName="Date" },
            new RenameColumn{OldName="OperatorName",NewName="OPERATOR" },
            new RenameColumn{OldName="ServiceName",NewName="SERVICE" },
            new RenameColumn{OldName="DiscountAmount",NewName="Commission" },
            new RenameColumn{OldName="%",NewName="Percentage" },
            new RenameColumn{OldName="VirtualBalanceID",NewName="TransactionID" },
            new RenameColumn{OldName="LastYearMonth",NewName="YearMonth" },
            new RenameColumn{OldName="Purchase",NewName="Fund Transfer" },
            new RenameColumn{OldName="TotalOldAmountAdjustment",NewName="Old Amount Adjustment" },
            new RenameColumn{OldName="TotalNewAmountAdjustment",NewName="New Amount Adjustment" },
            new RenameColumn{OldName="TotalCrAmt",NewName="Total Cr Amt" },
            new RenameColumn{OldName="TotalDrAmt",NewName="Total Dr Amt" },
            new RenameColumn{OldName="ClosingBalance",NewName="Closing Balance" },
            new RenameColumn{OldName="ExpClosingBal",NewName="Expected Closing Balance" },
            new RenameColumn{OldName="SupportStatus",NewName="Maker Status" },
            new RenameColumn{OldName="AdminStatus",NewName="Checker Status" },
            

        };
        
        private readonly List<RenameColumn> renameColumnsListForClient = new List<RenameColumn>
        {
            new RenameColumn{OldName="RetailerCommission",NewName="Commission" },
        };

        internal static DataTable ConvertListToDataTable<T>(List<T> models)
        {
            // creating a data table instance and typed it as our incoming model

            DataTable dataTable = new DataTable("ExcelTable");

            //Get all the properties of that model
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Adding Column name to our datatable
            foreach (PropertyInfo prop in Props)
            {
                string colName = prop.Name;

                if (dataTable.Columns.Contains(colName))
                {
                    colName = colName + " ";
                }
                //Setting column names as Property names
                if (prop.PropertyType == typeof(decimal))
                {
                    dataTable.Columns.Add(colName, typeof(decimal));
                }
                else if (prop.PropertyType == typeof(int))
                {
                    dataTable.Columns.Add(colName, typeof(int));
                }
                else if (prop.PropertyType == typeof(float))
                {
                    dataTable.Columns.Add(colName, typeof(float));
                }
                else
                {
                    dataTable.Columns.Add(colName);
                }
            }
            // Adding Row and its value to our dataTable
            foreach (T item in models)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }

                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        private void RemoveColumns(DataTable dt, List<string> columns)
        {
            if (columns.Count <= 0) return;

            //Remove columns
            foreach (string col in columns)
            {
                if (dt.Columns.Contains(col)) dt.Columns.Remove(col);
            }
        }

        private void RenameColumns(DataTable dt, int UserID)
        {
            if (renameColumnsList == null || renameColumnsList.Count <= 0) return;
            //Rename columns
            foreach (RenameColumn c in renameColumnsList)
            {
                if (dt.Columns.Contains(c.OldName))
                    dt.Columns[c.OldName].ColumnName = c.NewName;
            }

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserTypeID == (int)UserType.Client)
            {
                foreach (RenameColumn c in renameColumnsListForClient)
                {
                    if (dt.Columns.Contains(c.OldName))
                        dt.Columns[c.OldName].ColumnName = c.NewName;
                }
            }
        }

        private void RemoveColumnsByUserType(DataTable dt, int UserID, string ExcelName = "")
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<string> columns = new List<string>();
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
                columns = RemoveColumnsForRetailerAndAPIUser;

            if (UserTypeID == (int)UserType.Distributer)
                columns = RemoveColumnsDistributer;

            if (UserTypeID == (int)UserType.SuperDistributer)
                columns = RemoveColumnsSuperDistributer;

            if (UserTypeID == (int)UserType.Retailer)
                columns = filterColumnsReportwise(ExcelName, columns);

            if (UserTypeID == (int)UserType.Client)
                columns = filterColumnsReport(ExcelName);

            if (UserTypeID == (int)UserType.Admin)
                columns = filterColumnsForAdminReport(ExcelName);

            try
            {
                if (columns.Count > 0)
                    RemoveColumns(dt, columns);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        private List<string> filterColumnsForAdminReport(string ExcelName)
        {
            List<string> columns;

            if (ExcelName.ToLower().Contains("Recharge_Report".ToLower()))
                columns = RemoveColumnsRechargeReportAdmin;
            else if (ExcelName.ToLower().Contains("User_Downline".ToLower()))
                columns = RemoveColumnsUserDownlineAdmin;
            else if (ExcelName.ToLower().Contains("WalleTopUp_Report".ToLower()))
                columns = RemoveColumnsWalleTopUpAdmin;
            else
                columns = RemoveColumnsForAdmin;

            return columns;
        }

        private List<string> filterColumnsReportwise(string ExcelName, List<string> columns)
        {
            if (ExcelName.ToLower().Contains("Recharge".ToLower()))
                columns.AddRange(RemoveColumnsRechargeRetailer);
            else if (ExcelName.ToLower().Contains("DMT".ToLower()))
                columns.AddRange(RemoveColumnsDMTRetailer);
            else if (ExcelName.ToLower().Contains("UPITransfer_Report".ToLower()))
                columns.AddRange(RemoveColumnsUPIRetailer);
            else if (ExcelName.ToLower().Contains("PaymentGateway_Report".ToLower()))
                columns.AddRange(RemoveColumnsPaymentGatewayRetailer);
            else
                columns.AddRange(RemoveColumnsForRetailerAndAPIUser);

            return columns;
        }

        private List<string> filterColumnsReport(string ExcelName)
        {
            List<string> columns;

            if (ExcelName.ToLower().Contains("Recharge".ToLower()))
                columns = RemoveColumnsRechargeClient;
            else if (ExcelName.ToLower().Contains("Complaint_Report".ToLower()))
                columns = RemoveColumnsComplaintClient;
            else if (ExcelName.ToLower().Contains("DayBook".ToLower()))
                columns = RemoveColumnsDayBookClient;
            else if (ExcelName.ToLower().Contains("PaymentGateway".ToLower()))
                columns = RemoveColumnsPaymentGatewayClient;
            else
                columns = RemoveColumnsForClient;

            return columns;
        }

        private string cleanName(string name)
        {
            try
            {
                string cleanedName = Regex.Replace(name, @"[^A-Z0-9a-z]", "_");
                cleanedName = Regex.Replace(cleanedName, @"\s+", " ");
                return cleanedName;
            }
            catch (Exception)
            {
            }
            return name;
        }

        private List<string> filterColumns(string ExcelName)
        {
            List<string> columns;

            //PaymentGateway_Report

            if (ExcelName.ToLower().Contains("Recharge".ToLower()))
                columns = RechargeReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("DMT".ToLower()))
                columns = DMTReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("Payout".ToLower()))
                columns = PayoutReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("UPI".ToLower()))
                columns = DMTReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("AccountOpening".ToLower()))
                columns = AccountOpeningReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("Account".ToLower()))
                columns = AccountRemoveColumns;
            else if (ExcelName.ToLower().Contains("Ledger".ToLower()))
                columns = CommonRemoveColumns;
            else if (ExcelName.ToLower().Contains("UserWiseServiceWiseSale_Report".ToLower()))
                columns = OpratorWiseUserWiseRemoveColumns;
            else if (ExcelName.ToLower().Contains("OnBoardingUserList".ToLower()))
                columns = OnboardingUserRemoveColumns;
            else if (ExcelName.ToLower().Contains("User".ToLower()))
                columns = UserRemoveColumns;
            else if (ExcelName.ToLower().Contains("Fund".ToLower()))
                columns = FundRemoveColumns;
            else if (ExcelName.ToLower().Contains("E_Collection".ToLower()))
                columns = CommonRemoveColumns;
            else if (ExcelName.ToLower().Contains("ProviderFailure".ToLower()))
                columns = CommonRemoveColumns;
            else if (ExcelName.ToLower().Contains("Lapu".ToLower()))
                columns = CommonRemoveColumns;
            else if (ExcelName.ToLower().Contains("OfflineBook".ToLower()))
                columns = OfflineBookRemoveColumns;
            else if (ExcelName.ToLower().Contains("VirtualBal".ToLower()))
                columns = CommonRemoveColumns;
            else if (ExcelName.ToLower().Contains("PaymentGateway".ToLower()))
                columns = PaymentGatewayRemoveColumns;
            else if (ExcelName.ToLower().Contains("APIUser".ToLower()))
                columns = CommonRemoveColumns;
            else if (ExcelName.ToLower().Contains("Purchase".ToLower()))
                columns = PurchaseRemoveColumns;
            else if (ExcelName.ToLower().Contains("CommissionHold_Report".ToLower()))
                columns = CommissionHoldReportColumns;
            else if (ExcelName.ToLower().Contains("Comm".ToLower()))
                columns = CommiRemoveColumns;
            else if (ExcelName.ToLower().Contains("AEPS".ToLower()))
                columns = AepsRemoveColumns;
            else if (ExcelName.ToLower().Contains("Voucher".ToLower()))
                columns = VoucherColumns;
            else if (ExcelName.ToLower().Contains("Voucher_Report".ToLower()))
                columns = VoucherReportColumns;
            else if (ExcelName.ToLower().Contains("Micro".ToLower()))
                columns = CommonRemoveColumns;
            else if (ExcelName.ToLower().Contains("POS".ToLower()))
                columns = CommonRemoveColumns;
            else if (ExcelName.ToLower().Contains("PendingProvider".ToLower()))
                columns = RechargeReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("DayBook".ToLower()))
                columns = DayBookReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("Income".ToLower()))
                columns = IncomeReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("GST".ToLower()))
                columns = GSTReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("Sale".ToLower()))
                columns = SaleReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("Sender".ToLower()))
                columns = SenderReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("Bene".ToLower()))
                columns = SenderReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("ReferralRegistration".ToLower()))
                columns = ReferalReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("Providerlog".ToLower()))
                columns = ProviderlogRemoveColumns;
            else if (ExcelName.ToLower().Contains("Complaint_Report".ToLower()))
                columns = ComplaintReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("Slab".ToLower()))
                columns = SlabRemoveColumns;
            else if (ExcelName.ToLower().Contains("Message_Setting".ToLower()))
                columns = MessageSettingRemoveColumns;
            else if (ExcelName.ToLower().Contains("User_Filter".ToLower()))
                columns = UserFilterRemoveColumns;
            else if (ExcelName.ToLower().Contains("ProviderCallbacklog".ToLower()))
                columns = ProviderCallbacklogRemoveColumns;
            else if (ExcelName.ToLower().Contains("SmartPay_Report".ToLower()))
                columns = SmartPayReportRemoveColumns;
            else if (ExcelName.ToLower().Contains("KYC_Report".ToLower()))
                columns = KYCReportRemoveColumns;

            else if (ExcelName.ToLower().Contains("SubscriptionCharge_Report".ToLower()))
                columns = SubscriptionChargeRemoveColumns;

            else if (ExcelName.ToLower().Contains("SubscriptionPayment_Report".ToLower()))
                columns = SubscriptionPaymentRemoveColumns;

            else if (ExcelName.ToLower().Contains("GrowthLossAnalysis_Report".ToLower()))
                columns = GrowthLossAnalysisRemoveColumns;

            else if (ExcelName.ToLower().Contains("OperatorWiseReport".ToLower()))
                columns = OperatorWiseUserRemoveColumns;
            else if (ExcelName.ToLower().Contains("BBPS".ToLower()))
                columns = BBPSReportRemoveColumns;
            

            else
                columns = CommonRemoveColumns;

            return columns;
        }

        public string GenerateExcel<T>(string ExcelName, List<T> models, int UserID, string FolderName = "TransactionExcels")
        {
            if (models.Count <= 0) return "";

            ExcelName = cleanName(ExcelName);

            DataTable dt = ConvertListToDataTable(models);

            if (dt == null || dt.Columns.Count <= 0) return "";

            List<string> columns = filterColumns(ExcelName);

            RemoveColumnsByUserType(dt, UserID, ExcelName);
            try
            {
                RemoveColumns(dt, columns);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                RenameColumns(dt, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            string DocType = "Excel";

            string savepath = Path.Combine(Directory.GetCurrentDirectory(), @"Media/[FOLDER_NAME]");

            if (DocType.Contains("Excel")) savepath = savepath.Replace("[FOLDER_NAME]", FolderName);

            Directory.CreateDirectory(savepath);

            using XLWorkbook wb = new XLWorkbook();
            var wsDep = wb.Worksheets.Add(dt);
            wsDep.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            wsDep.Row(1).Style.Font.Bold = true;
            wsDep.Columns().AdjustToContents();

            wsDep.Table("Table1").Theme = XLTableTheme.None;

            string FileName = ExcelName + "_" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".xlsx";

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                wb.SaveAs(MyMemoryStream);
                try
                {
                    using FileStream fstream = new FileStream(savepath + "\\" + FileName, FileMode.Create);
                    MyMemoryStream.WriteTo(fstream);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            return Company.WebAPIURL + "app-docs/" + FolderName + "/" + FileName;
        }

        public string GeneratePdf<T>(string pdfName, List<T> tableDataList, int userID, string folderName = "TransactionExcels")
        {
            if (tableDataList == null || tableDataList.Count == 0)
                return "";

            pdfName = cleanName(pdfName);

            DataTable dt = ConvertListToDataTable(tableDataList);

            if (dt == null || dt.Columns.Count <= 0) return "";

            List<string> columns = filterColumns(pdfName);

            RemoveColumnsByUserType(dt, userID, pdfName);
            try
            {
                RemoveColumns(dt, columns);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                RenameColumns(dt, userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            string DocType = "PDF";

            string savePath = Path.Combine(Directory.GetCurrentDirectory(), $"Media/{folderName}");

            if (DocType.Contains("PDF")) savePath.Replace("[FOLDER_NAME]", folderName);

            Directory.CreateDirectory(savePath);

            string fileName = $"{pdfName}_{Guid.NewGuid().ToString("N").Substring(0, 10)}.pdf";
            string filePath = Path.Combine(savePath, fileName);

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    using (var writer = new PdfWriter(fs, new WriterProperties().AddUAXmpMetadata()).SetSmartMode(true))
                    {
                        using (var pdf = new PdfDocument(writer))
                        {
                            var document = new Document(pdf);

                            // Set landscape orientation
                            document = new Document(new PdfDocument(writer), iText.Kernel.Geom.PageSize.A0.Rotate());

                            int numberOfRows = dt.Rows.Count;
                            int numberOfColumns = dt.Columns.Count;

                            // Specify the maximum number of rows per page
                            int rowsPerPage = 20;

                            // Calculate the total number of pages needed
                            int totalNumberOfPages = (int)Math.Ceiling((double)numberOfRows / rowsPerPage);

                            // Create a table with the specified number of columns
                            Table table = new Table(numberOfColumns);

                            // Add column headers
                            foreach (DataColumn column in dt.Columns)
                            {
                                table.AddCell(new Cell().Add(new Paragraph(column.ColumnName)));
                            }

                            int rowIndex = 0;

                            // Add data rows
                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (var item in row.ItemArray)
                                {
                                    // Handle null values
                                    string cellValue = item != null ? item.ToString() : "";
                                    table.AddCell(new Cell().Add(new Paragraph(cellValue)));
                                }

                                rowIndex++;

                                // Check if a new page is needed
                                if (rowIndex % rowsPerPage == 0 && rowIndex < numberOfRows)
                                {
                                    document.Add(table);
                                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                                    table = new Table(numberOfColumns);

                                    // Add column headers for the new page
                                    foreach (DataColumn column in dt.Columns)
                                    {
                                        table.AddCell(new Cell().Add(new Paragraph(column.ColumnName)));
                                    }
                                }
                            }

                            // Add the last table to the document                     
                            document.Add(table);

                            document.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                // Log additional details if needed
            }
            return Company.WebAPIURL + $"app-docs/{folderName}/{fileName}";
        }

        internal DataTable GetDataTable(string filePath)
        {
            DataTable dt = new DataTable();

            if (string.IsNullOrEmpty(filePath)) return dt;

            using (XLWorkbook workBook = new XLWorkbook(filePath))
            {
                //Read the first Sheet from Excel file.
                IXLWorksheet workSheet = workBook.Worksheet(1);

                //Loop through the Worksheet rows.
                bool firstRow = true;
                foreach (IXLRow row in workSheet.Rows())
                {
                    //Use the first row to add columns to DataTable.
                    if (firstRow)
                    {
                        foreach (IXLCell cell in row.Cells())
                            dt.Columns.Add(cell.Value.ToString());
                        firstRow = false;
                    }
                    else
                    {
                        //Add rows to DataTable.
                        dt.Rows.Add();
                        int i = 0;
                        foreach (IXLCell cell in row.Cells(1, dt.Columns.Count)) // added because if cell value null or blank then take as it is
                        {
                            dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                            i++;
                        }
                    }
                }
            }
            return dt;
        }
    }
}