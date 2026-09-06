using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.TransactionDetails;
using Project.Response.Fund;
using Project.Response.TransactionDetails;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class TransactionDetailsBal
    {

        private List<ViewModel> getCashCollectionDetail(ReportRequest txReportRequest, KeyDataModel keyDataModel, List<ViewModel> list)
        {
            string AccountStatmentWithCashCollection = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.AccountStatmentWithCashCollection), "No");
            if (!AccountStatmentWithCashCollection.EqualsIgnoreCase("Yes"))
            {
                return list;
            }

            Project.Requests.CashCollection.ReportRequest cashCollectionRequest = new Project.Requests.CashCollection.ReportRequest();
            cashCollectionRequest.FromDate = txReportRequest.FromDate;
            cashCollectionRequest.ToDate = txReportRequest.ToDate;
            cashCollectionRequest.UserID = txReportRequest.UserID;

            List<Project.Response.CashCollection.ViewModel> cashCollectionList = new CashCollectionDetailDal().SelectByFilter(cashCollectionRequest, keyDataModel.UserID);

            foreach (Project.Response.CashCollection.ViewModel item in cashCollectionList)
            {
                ViewModel newItem = new ViewModel();
                newItem.Amount = item.OutstandingAmount;
                newItem.TransactionAmount = item.OutstandingAmount;
                newItem.ClosingBalance = item.OutstandingClosing;
                newItem.OpeningBalance = item.OutstandingOpening;
                newItem.UserID = item.UserID;
                newItem.AssignedUserID = item.ParentID;

                newItem.TransactionDescription = item.ParentRemark;
                newItem.DOC = item.DOC;
                newItem.OriginalDOC = item.OriginalDOC;
                newItem.FirmName = item.FirmName;
                newItem.DOCType = DOCType.CASH_COLLECTION;
                newItem.AmountCreditDebit = item.Credit ?? item.Debit ?? "";

                newItem.PayMode = item.PayMode;
                newItem.ReferenceNo = item.UTRNumber;
                newItem.FirmName = item.FirmName;
                newItem.AssignedUserName = item.ParentFirmName;

                if (list == null)
                {
                    list = new List<ViewModel>();
                }
                list.Add(newItem);
            }

            list = list.OrderByDescending(vm => vm.OriginalDOC).ToList();
            return list;

        }
        internal ReportWrapper SelectForFilter(ReportRequest txReportRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(keyDataModel.UserID);
            if (UserTypeID == (int)UserType.Support)
                keyDataModel.UserID = Company.AdminUserID;

            ReportWrapper wrapper = new ReportWrapper();
            failureMessage = txReportRequest.IsValidModalState();
            List<ViewModel> list = new List<ViewModel>();

            isExcelOrPDF(txReportRequest);

            if (txReportRequest.TakeCount <= 0)
                txReportRequest.TakeCount = 100;
            try
            {
                list = new TransactionDetailsDAL().SelectForFilter(txReportRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            list = getCashCollectionDetail(txReportRequest, keyDataModel, list);


            failureMessage = Messages.DataNotFound;
            if (list == null || list.Count <= 0) return wrapper;
          
            wrapper.Count = list.Count;
            if (!Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload))
            {
                list = list.Skip(txReportRequest.SkipCount).Take(txReportRequest.TakeCount).ToList();
            }
            list = list.AsQueryable().WhereDynamic(txReportRequest.SearchText).ToList();
            string ShowCommission = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission), "Yes");
            if (ShowCommission.EqualsIgnoreCase("No") && keyDataModel.UserID != Company.AdminUserID)
            {
                foreach (ViewModel item in list)
                {
                    item.DiscountAmount = 0;
                }
            }

            if (Util.IsDownload(txReportRequest.IsExcelDownload, txReportRequest.IsPDFDownload))
            {
                wrapper.IsExcelDownload = true;
            }
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal static bool isExcelOrPDF(Requests.TransactionDetails.ReportRequest request)
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

        private bool isAPIUser(int UserID)
        {
            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);
            if (user.UserTypeID == (int)UserType.APIUser)
                return true;

            return false;
        }

        private bool isRechargeWallet(int WalletID)
        {
            if (WalletID == (int)WalletCode.Recharge)
            {
                return true;
            }
            return false;
        }

        internal bool IsAmountValid(decimal OpeningBalance, int UserID, int WalletID)
        {
            ClosingBalanceView viewBalance = null;

            if (isAPIUser(UserID) && isRechargeWallet(WalletID))
            {
                return true;
            }
            try
            {
                viewBalance = new TransactionDetailsDAL().IsAmountValid(UserID, WalletID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return true;
            }

            if (viewBalance == null)
                return true;

            if (!Company.IsLastBalanceChecking.EqualsIgnoreCase("No"))
            {
                decimal diff = Math.Abs(viewBalance.ClosingBalance - OpeningBalance);

                if (diff >= 0.9999M)
                {
                    new Logger().Info("Invalid data, error code RT-5D40", "IsAmountValid", $"Op bal {OpeningBalance}, Cl bal : {viewBalance.ClosingBalance}, diff {diff} for UserID: {UserID}");
                    return false;
                }
            }

            return true;
        }

        internal async Task<List<AdminAccountingViewModel>> SelectAdminAccounting(AdminAccountingRequest reportRequest)
        {
            await Task.Delay(1);
            List<AdminAccountingViewModel> list = new List<AdminAccountingViewModel>();
            try
            {
                list = await new TransactionDetailsDAL().SelectAdminAccounting(reportRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return list;
            }

            return list;
        }

        internal async Task<List<AdminAccountingViewModel>> SelectMarketAccounting(AdminAccountingRequest reportRequest)
        {
            await Task.Delay(1);
            List<AdminAccountingViewModel> listresponse = new List<AdminAccountingViewModel>();
            int From = Convert.ToInt32(Convert.ToDateTime(reportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            try
            {
                decimal ClosingBalance = 0;
                ClosingBalance = await new UserDAL().SelectUserCurrentBalance(reportRequest.FromDate, reportRequest.WalletID);
                decimal ReverseBal = 0;
                decimal opBal = 0;
                decimal WalletTransfer = 0;
                WalletTransfer = new FundTransactionDal().SelectWalletTransferWalletID(From, reportRequest.WalletID);
                FundTransferViewModel fund = new FundTransactionDal().SelectFundTransferByUserIDAndWalletID(From, Company.AdminUserID, reportRequest.WalletID);
                ReverseBal = new FundTransactionDal().SelectFundReverseByUserIDAndWalletID(From, Company.AdminUserID, reportRequest.WalletID);
                Response.TransactionReport.DashboardViewModel Transectionreport = await new TransactionReportDAL().SelectForAdminDashborad(reportRequest.FromDate, reportRequest.WalletID);
                opBal = new UserBalanceHistoryDAL().SelectOpeningBalanceAllByWallet(reportRequest.WalletID, reportRequest.FromDate);

                AdminAccountingViewModel response = new AdminAccountingViewModel();
                response.OpeningBalance = opBal;
                response.Transfer = fund?.Transfer ?? 0;
                response.PGTransfer = fund?.PGTransfer ?? 0;
                response.WalletTransfer = WalletTransfer;
                response.Reverse = ReverseBal;
                response.Commission = Transectionreport?.TotalCommission ?? 0;
                response.Surcharge = Transectionreport?.Surcharge ?? 0;
                response.Success = Transectionreport.TotalSuccessAmount;
                response.FailedAmount = Transectionreport.TotalFailedAmount;
                response.PastFail = Transectionreport.PastFail;
                decimal TotalCreditAmount = 0;
                decimal TotalDebitAmount = 0;
                if (reportRequest.WalletID != 0)
                {
                    if (reportRequest.WalletID == (int)WalletCode.AEPS)
                    {
                        TotalCreditAmount = (response.OpeningBalance + response.Commission + response.PastFail + response.Reverse + response.Success);
                        TotalDebitAmount = (response.Surcharge + WalletTransfer);
                    }
                    else
                    {
                        TotalCreditAmount = (response.OpeningBalance + response.Transfer + response.PGTransfer + response.Commission + response.PastFail + WalletTransfer);
                        TotalDebitAmount = (response.Success + response.Reverse + response.Surcharge);
                    }
                }
                else
                {
                    TotalCreditAmount = (response.OpeningBalance + response.Transfer + response.PGTransfer + response.Commission + response.PastFail);
                    TotalDebitAmount = (response.Success + response.Reverse + response.Surcharge) - (Transectionreport.AEPSSuccess);
                }

                response.ClosingBalance = ClosingBalance;
                response.ExpectedClosingBalance = TotalCreditAmount - TotalDebitAmount;
                response.Difference = response.ClosingBalance - response.ExpectedClosingBalance;

                listresponse.Add(response);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return listresponse;
            }
            return listresponse;
        }
    }
}