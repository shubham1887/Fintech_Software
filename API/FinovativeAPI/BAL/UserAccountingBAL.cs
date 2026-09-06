using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.TransactionReport;
using Project.Response.TransactionReport;
using Project.Response.UserAccounting;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class UserAccountingBAL
    {
        internal Project.Response.TransactionReport.UserWiseAcountingWrapper UserwiseAccounting(Project.Requests.TransactionReport.UserwiseAccountingRequest selectRequest, int UserID, out string failureMessage)
        {
            Project.Response.TransactionReport.UserWiseAcountingWrapper report = new Project.Response.TransactionReport.UserWiseAcountingWrapper();
            failureMessage = selectRequest.IsValidModalState();
            isExcelOrPDF(selectRequest);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return report;

            List<Project.Response.TransactionReport.UserwiseAccountingViewModel> list = new List<Project.Response.TransactionReport.UserwiseAccountingViewModel>();

            try
            {
                list = new UserAccountingDAL().UserwiseAccounting(selectRequest);

                UserwiseAccountingViewModel rangewise = new UserwiseAccountingViewModel();
                string FirmName = "";
                string UserMobile = "";
                string Role = "";
                string WalletName = "";
                decimal OpeningBal = 0;
                decimal Purchase = 0;
                decimal TotalOldAmountAdjustment = 0;
                decimal TotalNewAmountAdjustment = 0;
                decimal TotalCrAmt = 0;
                decimal Sale = 0;
                decimal CommsionAmount = 0;
                decimal Surcharge = 0;
                int ManualUpdateCount = 0;
                decimal ClosingBalance = 0;
                decimal ExpClosingBal = 0;
                decimal Difference = 0;

                foreach (UserwiseAccountingViewModel item in list)
                {
                    OpeningBal += item.OpeningBal;
                    Purchase += item.Purchase;
                    TotalOldAmountAdjustment += item.TotalOldAmountAdjustment;
                    TotalNewAmountAdjustment += item.TotalNewAmountAdjustment;
                    TotalCrAmt += item.TotalCrAmt;
                    Sale += item.Sale;
                    CommsionAmount += item.CommsionAmount;
                    Surcharge += item.Surcharge;
                    ManualUpdateCount += item.ManualUpdateCount;
                    ClosingBalance += item.ClosingBalance;
                    ExpClosingBal += item.ExpClosingBal;
                    Difference += item.Difference;
                }
                rangewise.FirmName = "Total";
                rangewise.OpeningBal = OpeningBal;
                rangewise.Purchase = Purchase;
                rangewise.TotalOldAmountAdjustment = TotalOldAmountAdjustment;
                rangewise.TotalNewAmountAdjustment = TotalNewAmountAdjustment;
                rangewise.TotalCrAmt = TotalCrAmt;
                rangewise.Sale = Sale;
                rangewise.CommsionAmount = CommsionAmount;
                rangewise.Surcharge = Surcharge;
                rangewise.ManualUpdateCount = ManualUpdateCount;
                rangewise.ClosingBalance = ClosingBalance;
                rangewise.ExpClosingBal = ExpClosingBal;
                rangewise.Difference = Difference;

                list.Add(rangewise);
                if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
                {
                    report.Count = list.Count();
                }
                else
                {
                    report.Count = GetUserAccountingCount(selectRequest, UserID);
                }
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return report;
            }
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return report;

            list = list.AsQueryable()
                       .WhereDynamic(selectRequest.SearchText).ToList();

            list = list.AsQueryable()
                       .OrderByDynamic(selectRequest.SortColumnName, selectRequest.SortDirection)
                       .ToList();

            if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
            {
                report.IsDownload = true;
            }

            if (report == null)
            {
                failureMessage = Messages.DataNotFound;
                return report;
            }

            report.list = list;

            failureMessage = StatusType.Ok;
            return report;
        }
        internal static bool isExcelOrPDF(Project.Requests.TransactionReport.UserwiseAccountingRequest request)
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
        internal OldAmountAdjustmentReportWrapper SelectForFilterByOldAmountAdjustment(UserWiseAccountingCompRequest txReportRequest, KeyDataModel keydata, out string failureMessage)
        {
            OldAmountAdjustmentReportWrapper wrapper = new OldAmountAdjustmentReportWrapper();

            failureMessage = txReportRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<Project.Response.UserAccounting.OldAmountAdjustmentViewModel> list = new List<Project.Response.UserAccounting.OldAmountAdjustmentViewModel>();
            try
            {
                list = new UserAccountingDAL().SelectForFilterByOldAmountAdjustment(txReportRequest, keydata.UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }


            OldAmountAdjustmentViewModel rangewise = new OldAmountAdjustmentViewModel();

            decimal Amount = 0;
            decimal DebitAmount = 0;
            decimal Commission = 0;
            foreach (OldAmountAdjustmentViewModel item in list)
            {
                Amount += item.Amount;
                DebitAmount += item.DebitAmount;
                Commission += item.Commission;
            }
            rangewise.DOC = "Total";
            rangewise.DebitAmount = DebitAmount;
            rangewise.Commission = Commission;
            rangewise.Amount = Amount;

            list.Add(rangewise);
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;
            list = list.AsQueryable().WhereDynamic(txReportRequest.Search).ToList();

            wrapper.Count = list.Count;
            if (!txReportRequest.IsExcelDownload)
            {
                list = list.AsQueryable().SkipAndTakeCount(txReportRequest.SkipCount, txReportRequest.TakeCount).ToList();
            }
            wrapper.IsExcelDownload = txReportRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }


        internal OldAmountAdjustmentReportWrapper SelectForFilterByNewAmountAdjustment(UserWiseAccountingCompRequest txReportRequest, KeyDataModel keydata, out string failureMessage)
        {
            OldAmountAdjustmentReportWrapper wrapper = new OldAmountAdjustmentReportWrapper();

            failureMessage = txReportRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<NewAmountAdjustmentViewModel> list = new List<NewAmountAdjustmentViewModel>();
            try
            {
                list = new UserAccountingDAL().SelectForFilterByNewAmountAdjustment(txReportRequest, keydata.UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }


            NewAmountAdjustmentViewModel rangewise = new NewAmountAdjustmentViewModel();


            decimal Amount = 0;
            decimal DebitAmount = 0;
            decimal Commission = 0;
            foreach (NewAmountAdjustmentViewModel item in list)
            {
                Amount += item.Amount;
                DebitAmount += item.DebitAmount;
                Commission += item.Commission;
            }
            rangewise.DOC = "Total";
            rangewise.DebitAmount = DebitAmount;
            rangewise.Commission = Commission;
            rangewise.Amount = Amount;

            list.Add(rangewise);
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;
            list = list.AsQueryable().WhereDynamic(txReportRequest.Search).ToList();

            wrapper.Count = list.Count;
            if (!txReportRequest.IsExcelDownload)
            {
                list = list.AsQueryable().SkipAndTakeCount(txReportRequest.SkipCount, txReportRequest.TakeCount).ToList();
            }
            wrapper.IsExcelDownload = txReportRequest.IsExcelDownload;
            wrapper.NewAmountAdjustmentlist = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal ManualSuccessFailWrapper SelectForFilterByManualUpdate(UserWiseAccountingCompRequest mainRequest)
        {
            ManualSuccessFailWrapper wrapper = new ManualSuccessFailWrapper();
            List<ManualSuccessFailViewModel> list = new List<ManualSuccessFailViewModel>();
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                list = new UserAccountingDAL().SelectForFilterByManualUpdate(mainRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;
            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            wrapper.Count = list.Count;
            if (!mainRequest.IsExcelDownload)
            {
                list = list.AsQueryable().SkipAndTakeCount(mainRequest.SkipCount, mainRequest.TakeCount).ToList();
            }
            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        private int GetUserAccountingCount(UserwiseAccountingRequest selectRequest, int UserID)
        {
            List<UserMaster> usersList = new UserDAL().Select();
            List<UserMaster> listcount = new List<UserMaster>();

            listcount = (from item in usersList
                         select item).ToList();

            if (selectRequest.UserTypeID != 0 && selectRequest.UserID != 0)
            {
                listcount = (from item in listcount
                             where item.UserTypeID != (int)UserType.Distributer &&
                                   item.UserTypeID != (int)UserType.SuperDistributer
                             select item).ToList();
            }

            if (UserID != Company.AdminUserID)
            {
                listcount = (from item in listcount
                             where item.UserID == UserID
                             select item).ToList();
            }

            return listcount.Count;
        }
    }
}
