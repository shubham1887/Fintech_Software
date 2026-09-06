using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.LapuTransactionDetail;
using Project.Response.LapuTransactionDetail;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class LapuTransactionDetailBal
    {
    
        public string Insert(InsertRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.LapuTransactionDetailMessages.InsertFail;
            LapuTransactionDetail laputransactionMaster = new LapuTransactionDetail
            {
                UserID = (int)UserType.Admin,
                OperatorID = mainRequest.OperatorID,
                ProivderID = 0,
                LapuMasterID = 0,
                LapuPartyMasterID = mainRequest.LapuPartyMasterID,
                LapuNo = mainRequest.LapuNo,
                Status = StatusType.Success,
                ConsumerNumber = "",
                Amount = mainRequest.Amount,
                OpeningBalance = 0,
                ClosingBalance = 0,
                Commission = 0,
                DOCType = DOCType.Purchase,

                Description = "",
                TxnDate = Convert.ToDateTime(mainRequest.PurchaseDate)
            };

            int Result = -1;
            try
            {
                Result = new LapuTransactionDetailDal().Insert(laputransactionMaster);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return Message.LapuTransactionDetailMessages.InsertSuccess;
        }

        internal ReportWrapper Select(SelectUsageRequest selectRequest, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            failureMessage = Messages.DataNotFound;
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            List<PurchaseViewModel> viewModel = null;
            try
            {
                viewModel = new LapuTransactionDetailDal().Select(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return wrapper;
            }
            if (viewModel == null || viewModel.Count <= 0)
                return wrapper;
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            wrapper.list = viewModel;

            return wrapper;
        }

        internal string Update(UpdateRequest updateRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            LapuTransactionDetail model = null;
            try
            {
                model = new LapuTransactionDetailDal().SelectByID(updateRequest.LapuTransactionDetailID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (model == null)
                return Message.LapuTransactionDetailMessages.InvalidLapudetails;

            model.Amount = updateRequest.Amount;
            model.LapuTransactionDetailID = updateRequest.LapuTransactionDetailID;

            failureMessage = Message.LapuTransactionDetailMessages.UpdateFailure;
            int Result = -1;
            try
            {
                Result = new LapuTransactionDetailDal().Update(model);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
            {
                return failureMessage;
            }

            failureMessage = StatusType.Ok;
            return Message.LapuTransactionDetailMessages.UpdateSuccess;
        }

        internal List<UsageViewModel> SelectUsageReport(SelectUsageRequest selectRequest, out string failureMessage)
        {
            List<UsageViewModel> report = new List<UsageViewModel>();

            try
            {
                report = new LapuTransactionDetailDal().SelectUsageReport(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return report;
        }
    }
}