using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.DayBookReport;
using Project.Response.DayBookReport;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class DayBookReportBal
    {
        internal UserDaybookWrapper Select(SelectRequest mainRequest, int UserID, out string failureMessage)
        {
            UserDaybookWrapper wrapper = new UserDaybookWrapper();
            List<ViewModel> report = new List<ViewModel>();
            failureMessage = mainRequest.IsValidModalState();
            isExcelOrPDF(mainRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (UserTypeID == (int)UserType.Support)
                UserID = Company.AdminUserID;

            try
            {
                TransactionReportDAL ObjDal = new TransactionReportDAL();
                report = ObjDal.SelectForDayBook(mainRequest, UserID);

                if (report == null || report.Count <= 0)
                {
                    failureMessage = Messages.DataNotFound;
                    return wrapper;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            int TotalCount = 0;
            decimal TotalAmount = 0;
            int SuccessCount = 0;
            decimal SuccessAmount = 0;
            int FailedCount = 0;
            decimal FailedAmount = 0;
            int PendingCount = 0;
            decimal PendingAmount = 0;
            decimal RetailerCommission = 0;
            decimal DistributerCommission = 0;
            decimal SuperDistributerCommission = 0;
            decimal Surcharge = 0;

           

            foreach (ViewModel item in report)
            {
                TotalCount += item.TotalCount;
                TotalAmount += item.TotalAmount;
                SuccessCount += item.SuccessCount;
                SuccessAmount += item.SuccessAmount;
                FailedCount += item.FailedCount;
                FailedAmount += item.FailedAmount;
                PendingCount += item.PendingCount;
                PendingAmount += item.PendingAmount;

                if ((UserTypeID == (int)UserType.Retailer) || (UserTypeID == (int)UserType.APIUser) || (UserTypeID == (int)UserType.Client))
                {
                    RetailerCommission += item.RetailerCommission;
                    DistributerCommission = 0;
                    SuperDistributerCommission = 0;
                }
                if (UserTypeID == (int)UserType.Distributer)
                {
                    RetailerCommission += item.DistributerCommission;
                    DistributerCommission = 0;
                    SuperDistributerCommission = 0;
                }

                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    RetailerCommission += item.SuperDistributerCommission;
                    DistributerCommission = 0;
                    SuperDistributerCommission = 0;
                }
                if (UserTypeID == (int)UserType.Admin)
                {
                    RetailerCommission += item.RetailerCommission;
                    DistributerCommission += item.DistributerCommission;
                    SuperDistributerCommission += item.SuperDistributerCommission;
                }
                Surcharge += item.Surcharge;
            }
            ViewModel totalitem = new ViewModel();
            totalitem.Operator = "Total";
            totalitem.TotalCount = TotalCount;
            totalitem.TotalAmount = TotalAmount;
            totalitem.SuccessCount = SuccessCount;
            totalitem.SuccessAmount = SuccessAmount;
            totalitem.FailedCount = FailedCount;
            totalitem.FailedAmount = FailedAmount;
            totalitem.PendingCount = PendingCount;
            totalitem.PendingAmount = PendingAmount;
            totalitem.RetailerCommission = RetailerCommission;
            totalitem.DistributerCommission = DistributerCommission;
            totalitem.SuperDistributerCommission = SuperDistributerCommission;
            totalitem.Surcharge = Surcharge;

            report.Add(totalitem);
            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }
         
            wrapper.list = report;

            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.DayBookReport.SelectRequest request)
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
        internal ProviderDaybookWrapper ProviderDaybook(ProviderDayBookRequest mainRequest, int UserID, out string failureMessage)
        {
            ProviderDaybookWrapper response = new ProviderDaybookWrapper();

            List<ProviderDayBookViewModel> report = new List<ProviderDayBookViewModel>();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;
            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);

            if (user.UserTypeID != (int)UserType.Admin)
            {
                failureMessage = UserMessage.InvalidUser;
                return response;
            }
            try
            {
                TransactionReportDAL ObjDal = new TransactionReportDAL();
                report = ObjDal.ProviderDaybook(mainRequest);

                if (report == null || report.Count <= 0)
                {
                    failureMessage = Messages.DataNotFound;
                    return response;
                }

                int TotalHits = 0;
                decimal TotalAmount = 0;
                int SuccessHits = 0;
                decimal SuccessAmount = 0;
                int FailedHits = 0;
                decimal FailedAmount = 0;
                int PendingHits = 0;
                decimal PendingAmount = 0;
                decimal AdminCommission = 0;
                decimal RetailerCommission = 0;
                decimal DistributerCommission = 0;
                decimal SuperDistributerCommission = 0;
                decimal ProviderCommission = 0;
                decimal Profit = 0;

                foreach (ProviderDayBookViewModel item in report)
                {
                    TotalHits += item.TotalHits;
                    TotalAmount += item.TotalAmount;
                    SuccessHits += item.SuccessHits;
                    SuccessAmount += item.SuccessAmount;
                    FailedHits += item.FailedHits;
                    FailedAmount += item.FailedAmount;
                    PendingHits += item.PendingHits;
                    PendingAmount += item.PendingAmount;
                    AdminCommission += item.AdminCommission;
                    RetailerCommission += item.RetailerCommission;
                    DistributerCommission += item.DistributerCommission;
                    SuperDistributerCommission += item.SuperDistributerCommission;
                    ProviderCommission += item.ProviderCommission;
                    Profit += item.Profit;

                    if (item.SuccessAmount == 0 || item.Profit == 0)
                    {
                        continue;
                    }
                    item.Percentage = (item.Profit / item.SuccessAmount) * 100;
                }
                ProviderDayBookViewModel totalitem = new ProviderDayBookViewModel();
                totalitem.ProviderName = "";
                totalitem.Operator = "Total";
                totalitem.TotalHits = TotalHits;
                totalitem.TotalAmount = TotalAmount;
                totalitem.SuccessHits = SuccessHits;
                totalitem.SuccessAmount = SuccessAmount;
                totalitem.FailedHits = FailedHits;
                totalitem.FailedAmount = FailedAmount;
                totalitem.PendingHits = PendingHits;
                totalitem.PendingAmount = PendingAmount;
                totalitem.AdminCommission = AdminCommission;
                totalitem.ProviderCommission = ProviderCommission;
                totalitem.RetailerCommission = RetailerCommission;
                totalitem.DistributerCommission = DistributerCommission;
                totalitem.SuperDistributerCommission = SuperDistributerCommission;
                totalitem.Percentage = (Profit / SuccessAmount) * 100;
                totalitem.Profit = Profit;
                report.Add(totalitem);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            response.list = report;
            response.IsExcelDownload = mainRequest.IsExcelDownload;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal UserDaybookWrapper SelectByProviderID(SelectRequest mainRequest, int UserID, out string failureMessage)
        {
            UserDaybookWrapper wrapper = new UserDaybookWrapper();
            List<ViewModel> report = new List<ViewModel>();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                TransactionReportDAL ObjDal = new TransactionReportDAL();
                report = ObjDal.SelectByProviderID(mainRequest, UserID);

                if (report == null || report.Count <= 0)
                {
                    failureMessage = Messages.DataNotFound;
                    return wrapper;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = report;

            failureMessage = StatusType.Ok;
            return wrapper;
        }
    }
}