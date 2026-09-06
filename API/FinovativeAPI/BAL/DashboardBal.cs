using Project.DAL;
using Project.Processing;
using Project.Response.Dashboard;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class DashboardBal
    {
        internal async Task<(string, DashboardViewModel)> Select(int UserID, string Date)
        {
            string failureMessage = "No data found";
            DashboardViewModel response = new DashboardViewModel();

            if (UserID != Company.AdminUserID)
            {
                failureMessage = Message.UserMessage.InvalidUser;
                return (failureMessage, response);
            }
            int WalletID = 0;
            var transectionReportTask = new TransactionReportDAL().SelectForAdminDashborad(Date, WalletID);
            var fundReportTask = new FundTransactionDal().SelectTransferForDashboard(UserID, Date);
            var complaintTask = new ComplaintMasterDal().SelectDashboradComplaint(Date);
            var KycTask = new UserDocumentMasterDal().SelectKycPendingCount();
            var targetTask = new UserDAL().SelectUserTarget(UserID);

            await Task.WhenAll(transectionReportTask, fundReportTask, complaintTask, KycTask, targetTask);

            Response.TransactionReport.DashboardViewModel Transectionreport = transectionReportTask.Result;
            Response.Fund.DashboardFundViewModel Fundreport = fundReportTask.Result;
            Response.ComplaintResponse.DashboardComplaintViewModel complaintreport = complaintTask.Result;
            Response.KYC.KycCountViewModel kycreport = KycTask.Result;
            Response.User.TargetViewModel targetreport = targetTask.Result;

            try
            {
                response.TotalSuccessAmount = Transectionreport?.TotalSuccessAmount ?? 0;
                response.TotalAdminCommission = Transectionreport?.TotalAdminCommission ?? 0;
                response.TotalAdminSurcharge = Math.Abs(Transectionreport?.TotalAdminSurcharge ?? 0);

                response.AdminCommissionProfit = Transectionreport?.AdminCommissionProfit ?? 0;
                response.AdminSurchargeProfit = Transectionreport?.AdminSurchargeProfit ?? 0;

                response.RofferComm = Transectionreport?.RofferComm ?? 0;
                response.RofferTotal = Transectionreport?.RofferTotal ?? 0;
                response.RofferPer = Transectionreport?.RofferPer ?? "0";

                response.TotalFailedAmount = Transectionreport?.TotalFailedAmount ?? 0;
                response.TransferAmount = (Fundreport?.TransferAmount ?? 0) + (Fundreport?.PGTransferAmount ?? 0);
                response.TransferCount = Fundreport?.TransferCount ?? 0;
                response.ComplaintAmount = complaintreport?.ComplaintAmount ?? 0;
                response.ComplaintCount = complaintreport?.ComplaintCount ?? 0;
                response.KycCount = kycreport?.KycCount ?? 0;
                response.TotalTarget = targetreport?.TotalTarget ?? 0;
                response.RemainsTarget = (targetreport?.TotalTarget ?? 0) - (Fundreport?.TransferAmount ?? 0);

                response.PrePaidSuccess = Transectionreport?.PrePaidSuccess ?? 0;
                response.PrePaidFail = Transectionreport?.PrePaidFail ?? 0;

                response.DthSuccess = Transectionreport?.DthSuccess ?? 0;
                response.DthFail = Transectionreport?.DthFail ?? 0;

                response.PostPaidSuccess = Transectionreport?.PostPaidSuccess ?? 0;
                response.PostPaidFail = Transectionreport?.PostPaidFail ?? 0;

                response.DMTSuccess = Transectionreport?.DMTSuccess ?? 0;
                response.DMTFail = Transectionreport?.DMTFail ?? 0;

                response.AEPSSuccess = Transectionreport?.AEPSSuccess ?? 0;
                response.AEPSFail = Transectionreport?.AEPSFail ?? 0;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return (failureMessage, response);
        }

        internal async Task<(string failureMessage, DashboardViewModel list)> SelectForRetailer(int UserID, string Date)
        {
            string failureMessage = "No data found";
            DashboardViewModel response = new DashboardViewModel();
            var transectionReportTask = new TransactionReportDAL().SelectForRetailer(UserID, Date);
            var fundReportTask = new FundTransactionDal().SelectFundForRetailer(UserID, Date);
            await Task.WhenAll(transectionReportTask, fundReportTask);

            Response.TransactionReport.DashboardViewModel Transectionreport = transectionReportTask.Result;
            Response.Fund.DashboardFundViewModel Fundreport = fundReportTask.Result;

            try
            {
                response.TotalSuccessAmount = Transectionreport.TotalSuccessAmount;
                response.TotalFailedAmount = Transectionreport.TotalFailedAmount;
                response.PurchaseAmount = Fundreport.PurchaseAmount;
                response.PurchaseCount = Fundreport.PurchaseCount;
                response.TotalCommission = Transectionreport.TotalCommission;
                response.Surcharge = Transectionreport.Surcharge;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return (failureMessage, response);
        }

        internal async Task<(string failureMessage, DashboardViewModel list)> SelectForDistributor(int UserID, string Date)
        {
            string failureMessage = "No data found";
            DashboardViewModel response = new DashboardViewModel();
            var transectionReportTask = new TransactionReportDAL().SelectForDistributorDashborad(UserID, Date);
            var fundReportTask = new FundTransactionDal().SelectTransferForDistributor(UserID, Date);
            await Task.WhenAll(transectionReportTask, fundReportTask);

            Response.TransactionReport.DashboardViewModel Transectionreport = transectionReportTask.Result;
            Response.Fund.DashboardFundViewModel Fundreport = fundReportTask.Result;
            try
            {
                response.TotalSuccessAmount = Transectionreport.TotalSuccessAmount;
                response.TotalFailedAmount = Transectionreport.TotalFailedAmount;

                response.TransferAmount = Fundreport.TransferAmount;
                response.TransferCount = Fundreport.TransferCount;

                response.PurchaseAmount = Fundreport.PurchaseAmount;
                response.PurchaseCount = Fundreport.PurchaseCount;

                response.TotalCommission = Transectionreport.TotalCommission;
                response.Surcharge = Transectionreport.Surcharge;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return (failureMessage, response);
        }
    }
}