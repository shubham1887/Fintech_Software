using Project.DAL;
using Project.Processing;
using Project.Response.TodaySummary;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class TodaySummaryBal
    {
        internal async Task<(string, List<TodaySummaryViewModel>)> Select(int userID, string Date)
        {
            string failureMessage = "No data found";

            List<TodaySummaryViewModel> report = new List<TodaySummaryViewModel>();

            if (userID != Company.AdminUserID)
            {
                failureMessage = Message.UserMessage.InvalidUser;
                return (failureMessage, report);
            }
            int WalletID = 0;
            var transectionReportTask = new TransactionReportDAL().SelectForAdminDashborad(Date, WalletID);
            var fundReportTask = new FundTransactionDal().SelectTransferForDashboard(userID, Date);
            var userBalanceHistory = new UserBalanceHistoryDAL().SelectOpeningBalance(Date);
            var serviceCharge = new UserServiceChargeDal().TotalCharge(Date);
            var UserClosingBalance = new UserDAL().SelectUserCurrentBalance(Date, WalletID);
            await Task.WhenAll(transectionReportTask, fundReportTask, userBalanceHistory, serviceCharge, UserClosingBalance);

            Response.TransactionReport.DashboardViewModel Transectionreport = transectionReportTask.Result;
            Response.Fund.DashboardFundViewModel Fundreport = fundReportTask.Result;

            decimal opBalace = userBalanceHistory.Result;
            decimal TotalserviceCharge = serviceCharge.Result;
            decimal ClosingBalance = UserClosingBalance.Result;
            decimal TotalSuccessAmountwithoutAEPSandMATMPOS = Transectionreport.TotalSuccessAmount - (Transectionreport.AEPSSuccess + Transectionreport.MATMSuccess + Transectionreport.POSSuccess);

            decimal TotalCreditAmount = (opBalace + Fundreport.TransferAmount + Fundreport.PGTransferAmount + Fundreport.ECollectionAmount + Transectionreport.TotalCommission + Transectionreport.PastFail + Transectionreport.AEPSSuccess + Transectionreport.MATMSuccess + Transectionreport.POSSuccess);
            decimal TotalDebitAmount = (TotalSuccessAmountwithoutAEPSandMATMPOS + Fundreport.ReverseAmount + Transectionreport.Surcharge + TotalserviceCharge + Transectionreport.TotalTDS);//+ Transectionreport.PastSuccess
            decimal ExpectedCloseBal = TotalCreditAmount - TotalDebitAmount;
            decimal Difference = ClosingBalance - ExpectedCloseBal;
            TodaySummaryViewModel balanceItem = new TodaySummaryViewModel();
            balanceItem.Type = "Opening";
            balanceItem.TotalAmount = opBalace;
            report.Add(balanceItem);

            try
            {
                TodaySummaryViewModel transferItem = new TodaySummaryViewModel();
                transferItem.Type = "Fund Transfered";
                transferItem.TotalAmount = Fundreport?.TransferAmount ?? 0;
                report.Add(transferItem);

                TodaySummaryViewModel AEPSItem = new TodaySummaryViewModel();
                AEPSItem.Type = "AEPS";
                AEPSItem.TotalAmount = Transectionreport?.AEPSSuccess ?? 0;
                report.Add(AEPSItem);

                TodaySummaryViewModel MATMItem = new TodaySummaryViewModel();
                MATMItem.Type = "MATM";
                MATMItem.TotalAmount = Transectionreport?.MATMSuccess ?? 0;
                report.Add(MATMItem);

                TodaySummaryViewModel POSItem = new TodaySummaryViewModel();
                POSItem.Type = "POS";
                POSItem.TotalAmount = Transectionreport?.POSSuccess ?? 0;
                report.Add(POSItem);

                TodaySummaryViewModel CMSItem = new TodaySummaryViewModel();
                CMSItem.Type = "CMS";
                CMSItem.TotalAmount = Transectionreport?.CMSSuccess ?? 0;
                report.Add(CMSItem);

                TodaySummaryViewModel pgtransferItem = new TodaySummaryViewModel();
                pgtransferItem.Type = "PG-Transfered";
                pgtransferItem.TotalAmount = Fundreport?.PGTransferAmount ?? 0;
                report.Add(pgtransferItem);

                TodaySummaryViewModel etransferItem = new TodaySummaryViewModel();
                etransferItem.Type = "E-Collection";
                etransferItem.TotalAmount = Fundreport?.ECollectionAmount ?? 0;
                report.Add(etransferItem);

                TodaySummaryViewModel reverseItem = new TodaySummaryViewModel();
                reverseItem.Type = "Fund Deducted";
                reverseItem.TotalAmount = Fundreport?.ReverseAmount ?? 0;
                report.Add(reverseItem);

                TodaySummaryViewModel totalItem = new TodaySummaryViewModel();
                totalItem.Type = "Success";
                totalItem.TotalAmount = Transectionreport?.TotalSuccessAmount ?? 0;
                report.Add(totalItem);

                TodaySummaryViewModel FailItem = new TodaySummaryViewModel();
                FailItem.Type = "Refund";
                FailItem.TotalAmount = Transectionreport?.TotalFailedAmount ?? 0;
                report.Add(FailItem);

                TodaySummaryViewModel commiItem = new TodaySummaryViewModel();
                commiItem.Type = "Commission";
                commiItem.TotalAmount = Transectionreport?.TotalCommission ?? 0;
                report.Add(commiItem);

                TodaySummaryViewModel tdsItem = new TodaySummaryViewModel();
                tdsItem.Type = "TDS";
                tdsItem.TotalAmount = Transectionreport?.TotalTDS ?? 0;
                report.Add(tdsItem);

                TodaySummaryViewModel surchargeItem = new TodaySummaryViewModel();
                surchargeItem.Type = "Surcharge";
                surchargeItem.TotalAmount = Transectionreport?.Surcharge ?? 0;
                report.Add(surchargeItem);

                TodaySummaryViewModel chargeItem = new TodaySummaryViewModel();
                chargeItem.Type = "Other Charge";
                chargeItem.TotalAmount = TotalserviceCharge;
                report.Add(chargeItem);

                TodaySummaryViewModel totalpending = new TodaySummaryViewModel();
                totalpending.Type = "Pending ";
                totalpending.TotalAmount = Transectionreport?.TotalProcessAmount ?? 0;
                report.Add(totalpending);

                TodaySummaryViewModel pastSuccess = new TodaySummaryViewModel();
                pastSuccess.Type = "Past Success";
                pastSuccess.TotalAmount = Transectionreport?.PastSuccess ?? 0;
                report.Add(pastSuccess);

                TodaySummaryViewModel pastFail = new TodaySummaryViewModel();
                pastFail.Type = "Past Refund";
                pastFail.TotalAmount = Transectionreport?.PastFail ?? 0;
                report.Add(pastFail);

                TodaySummaryViewModel BalItem = new TodaySummaryViewModel();
                BalItem.Type = "ClosingBalance";
                BalItem.TotalAmount = ClosingBalance;
                report.Add(BalItem);

                TodaySummaryViewModel ExpItem = new TodaySummaryViewModel();
                ExpItem.Type = "Expected Closing";
                ExpItem.TotalAmount = ExpectedCloseBal;
                report.Add(ExpItem);

                TodaySummaryViewModel diffItem = new TodaySummaryViewModel();
                diffItem.Type = "Difference";
                diffItem.TotalAmount = Difference + Transectionreport.TotalProcessAmount;
                report.Add(diffItem);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return (failureMessage, report);
        }

        internal async Task<(string, List<TodaySummaryViewModel>)> SelectForRetailer(int UserID, string Date)
        {
            string failureMessage = "No data found";
            List<TodaySummaryViewModel> report = new List<TodaySummaryViewModel>();
            int CashFreeAiUserID = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.CashFreeAiUserID), 0);

            var transectionReportTask = new TransactionReportDAL().SelectForRetailer(UserID, Date);
            var fundReportTask = new FundTransactionDal().SelectFundForRetailer(UserID, Date);
            var userBalanceHistory = new UserBalanceHistoryDAL().SelectOpeningBalanceByUserID(UserID, Date);
            var serviceCharge = new UserServiceChargeDal().TotalChargeByUserID(UserID, Date);
            var UserClosingBalance = new UserDAL().SelectCurrentBalanceByUserID(UserID, Date);

            await Task.WhenAll(transectionReportTask, fundReportTask, userBalanceHistory, serviceCharge, UserClosingBalance);

            Response.TransactionReport.DashboardViewModel Transectionreport = transectionReportTask.Result;
            Response.Fund.DashboardFundViewModel Fundreport = fundReportTask.Result;
            decimal opBalace = userBalanceHistory.Result;
            decimal TotalserviceCharge = serviceCharge.Result;
            decimal ClosingBalance = UserClosingBalance.Result;
            decimal TotalCreditAmount = (opBalace + Fundreport.PurchaseAmount + Transectionreport.TotalCommission + Transectionreport.PastFail);
            decimal TotalDebitAmount = (Transectionreport.TotalSuccessAmount + Transectionreport.Surcharge + TotalserviceCharge + Transectionreport.PastSuccess + Fundreport.ReverseAmount);
            decimal ExpectedCloseBal = TotalCreditAmount - TotalDebitAmount;
            decimal Difference = ClosingBalance - ExpectedCloseBal;
            TodaySummaryViewModel balanceItem = new TodaySummaryViewModel();
            balanceItem.Type = "Opening";
            balanceItem.TotalAmount = opBalace;
            report.Add(balanceItem);
            try
            {
                TodaySummaryViewModel transferItem = new TodaySummaryViewModel();
                transferItem.Type = "Fund Purchased";
                transferItem.TotalAmount = Fundreport.PurchaseAmount;
                report.Add(transferItem);

                TodaySummaryViewModel PGtransferItem = new TodaySummaryViewModel();
                PGtransferItem.Type = "PG Purchased";
                PGtransferItem.TotalAmount = Fundreport.PGPurchaseAmount;
                report.Add(PGtransferItem);

                TodaySummaryViewModel reverseItem = new TodaySummaryViewModel();
                reverseItem.Type = "Reversed";
                reverseItem.TotalAmount = Fundreport.ReverseAmount;
                report.Add(reverseItem);

                TodaySummaryViewModel totalItem = new TodaySummaryViewModel();
                totalItem.Type = "Success";
                totalItem.TotalAmount = Transectionreport.TotalSuccessAmount;
                report.Add(totalItem);

                if ( UserID != CashFreeAiUserID)
                {
                    TodaySummaryViewModel AEPSItem = new TodaySummaryViewModel();
                    AEPSItem.Type = "AEPS";
                    AEPSItem.TotalAmount = Transectionreport.AEPSSuccess;
                    report.Add(AEPSItem);
                }

                TodaySummaryViewModel FailItem = new TodaySummaryViewModel();
                FailItem.Type = "Refund";
                FailItem.TotalAmount = Transectionreport.TotalFailedAmount;
                report.Add(FailItem);

                TodaySummaryViewModel commiItem = new TodaySummaryViewModel();
                commiItem.Type = "Commission";
                commiItem.TotalAmount = Transectionreport.TotalCommission;
                report.Add(commiItem);

                TodaySummaryViewModel surchargeItem = new TodaySummaryViewModel();
                surchargeItem.Type = "Surcharge";
                surchargeItem.TotalAmount = Transectionreport.Surcharge;
                report.Add(surchargeItem);

                TodaySummaryViewModel chargeItem = new TodaySummaryViewModel();
                chargeItem.Type = "Other Charge";
                chargeItem.TotalAmount = TotalserviceCharge;
                report.Add(chargeItem);

                TodaySummaryViewModel totalpending = new TodaySummaryViewModel();
                totalpending.Type = "Pending ";
                totalpending.TotalAmount = Transectionreport.TotalProcessAmount;
                report.Add(totalpending);

                TodaySummaryViewModel pastSuccess = new TodaySummaryViewModel();
                pastSuccess.Type = "Past Success";
                pastSuccess.TotalAmount = Transectionreport.PastSuccess;
                report.Add(pastSuccess);

                TodaySummaryViewModel pastFail = new TodaySummaryViewModel();
                pastFail.Type = "Past Refund";
                pastFail.TotalAmount = Transectionreport.PastFail;
                report.Add(pastFail);

                TodaySummaryViewModel BalItem = new TodaySummaryViewModel();
                BalItem.Type = "ClosingBalance";
                BalItem.TotalAmount = ClosingBalance;
                report.Add(BalItem);

                TodaySummaryViewModel ExpItem = new TodaySummaryViewModel();
                ExpItem.Type = "Expected Closing";
                ExpItem.TotalAmount = ExpectedCloseBal;
                report.Add(ExpItem);

                TodaySummaryViewModel diffItem = new TodaySummaryViewModel();
                diffItem.Type = "Difference";
                diffItem.TotalAmount = Difference;
                report.Add(diffItem);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return (failureMessage, report);
        }

        internal async Task<(string, List<TodaySummaryViewModel>)> SelectForDistributor(int UserID, string Date)
        {
            string failureMessage = "No data found";

            List<TodaySummaryViewModel> report = new List<TodaySummaryViewModel>();

            var transectionReportTask = new TransactionReportDAL().SelectForDistributorDashborad(UserID, Date);
            var fundReportTask = new FundTransactionDal().SelectTransferForDistributor(UserID, Date);
            var userBalanceHistory = new UserBalanceHistoryDAL().SelectOpeningBalanceByUserID(UserID, Date);
            var serviceCharge = new UserServiceChargeDal().TotalChargeByUserID(UserID, Date);
            var UserClosingBalance = new UserDAL().SelectCurrentBalanceByUserID(UserID, Date);

            await Task.WhenAll(transectionReportTask, fundReportTask, userBalanceHistory, serviceCharge, UserClosingBalance);

            Response.TransactionReport.DashboardViewModel Transectionreport = transectionReportTask.Result;
            Response.Fund.DashboardFundViewModel Fundreport = fundReportTask.Result;
            decimal opBalace = userBalanceHistory.Result;
            decimal TotalserviceCharge = serviceCharge.Result;
            decimal ClosingBalance = UserClosingBalance.Result;
            decimal TotalCreditAmount = (opBalace + Fundreport.PurchaseAmount + Fundreport.DownlineReverseAmount + Transectionreport.TotalCommission);
            decimal TotalDebitAmount = (Fundreport.TransferAmount + Fundreport.ReverseAmount + Transectionreport.Surcharge + TotalserviceCharge);
            decimal ExpectedCloseBal = TotalCreditAmount - TotalDebitAmount;
            decimal Difference = ClosingBalance - ExpectedCloseBal;
            TodaySummaryViewModel balanceItem = new TodaySummaryViewModel();
            balanceItem.Type = "Opening";
            balanceItem.TotalAmount = opBalace;
            report.Add(balanceItem);

            try
            {
                TodaySummaryViewModel transferItem = new TodaySummaryViewModel();
                transferItem.Type = "Fund Transfered";
                transferItem.TotalAmount = Fundreport.TransferAmount;
                report.Add(transferItem);

                TodaySummaryViewModel purchaseItem = new TodaySummaryViewModel();
                purchaseItem.Type = "Purchased";
                purchaseItem.TotalAmount = Fundreport.PurchaseAmount;
                report.Add(purchaseItem);

                TodaySummaryViewModel deductItem = new TodaySummaryViewModel();
                deductItem.Type = "Fund Deducted";
                deductItem.TotalAmount = Fundreport.DownlineReverseAmount;
                report.Add(deductItem);

                TodaySummaryViewModel reverseItem = new TodaySummaryViewModel();
                reverseItem.Type = "Reversed";
                reverseItem.TotalAmount = Fundreport.ReverseAmount;
                report.Add(reverseItem);

                TodaySummaryViewModel commiItem = new TodaySummaryViewModel();
                commiItem.Type = "Commission";
                commiItem.TotalAmount = Transectionreport.TotalCommission;
                report.Add(commiItem);

                TodaySummaryViewModel surchargeItem = new TodaySummaryViewModel();
                surchargeItem.Type = "Surcharge";
                surchargeItem.TotalAmount = Transectionreport.Surcharge;
                report.Add(surchargeItem);

                TodaySummaryViewModel chargeItem = new TodaySummaryViewModel();
                chargeItem.Type = "Other Charge";
                chargeItem.TotalAmount = TotalserviceCharge;
                report.Add(chargeItem);

                TodaySummaryViewModel BalItem = new TodaySummaryViewModel();
                BalItem.Type = "Closing Balance";
                BalItem.TotalAmount = ClosingBalance;
                report.Add(BalItem);

                TodaySummaryViewModel ExpItem = new TodaySummaryViewModel();
                ExpItem.Type = "Expected Closing";
                ExpItem.TotalAmount = ExpectedCloseBal;
                report.Add(ExpItem);

                TodaySummaryViewModel diffItem = new TodaySummaryViewModel();
                diffItem.Type = "Difference";
                diffItem.TotalAmount = Difference;
                report.Add(diffItem);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return (failureMessage, report);
        }
    }
}