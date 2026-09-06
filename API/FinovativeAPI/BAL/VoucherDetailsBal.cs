using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class VoucherDetailsBal
    {
        public void InsertCashCollectionMaster(int UserID)
        {
            CashCollectionMaster ObjModel = new CashCollectionMaster
            {
                UserID = UserID,
                Outstanding = 0,
                MaxOutstanding = 999999
            };

            new CashCollectionMasterDal().Insert(ObjModel);
        }
        internal string VoucherDetailsInsert(int UserID, string Mode,decimal Amount,string Remark, string Credit,string Debit)
        {

            decimal OS_OpeningBalance = 0;
            decimal OS_ClosingBalance = 0;

            CashCollectionMaster ObjModel = new CashCollectionMasterDal().SelectByUserID(UserID);
            if (ObjModel == null)
            {
                new CashCollectionBal().InsertCashCollectionMaster(UserID);
                ObjModel = new CashCollectionMasterDal().SelectByUserID(UserID);
            }

   

            UserMaster user = new UserDAL().SelectByIDFromCache(ObjModel.UserID);

            if (Credit.EqualsIgnoreCase(AmountCreditDebitType.Credit))
            {

                OS_OpeningBalance = ObjModel.Outstanding;
                OS_ClosingBalance = OS_OpeningBalance + Amount;
                ObjModel.Outstanding = OS_ClosingBalance;

            }
            if (Debit.EqualsIgnoreCase(AmountCreditDebitType.Debit))
            {

                OS_OpeningBalance = ObjModel.Outstanding;
                OS_ClosingBalance = OS_OpeningBalance - Amount;
                ObjModel.Outstanding = OS_ClosingBalance;

            }

            string failureMessage = "";
           
            Project.Model.VoucherDetails VoucherDetails = new Project.Model.VoucherDetails
            {
                UserID = UserID,
                Mode = Mode,
                OutstandingOpeningBal = OS_OpeningBalance,
                Amount =Amount,
                OutstandingClossingBal = OS_ClosingBalance,
                Remark = Remark,
                Credit = Credit,
                Debit = Debit,
            };
            int Result = new VoucherDetailsDal().Insert(VoucherDetails);

            if (Result < 0)
            {

                return Message.VoucherMasterMessage.VoucherMasterFailed;
            }
            int UpdateObjModel = new CashCollectionMasterDal().Update(ObjModel);
            if (UpdateObjModel < 0)
            {
                return "";

            }
            failureMessage = StaticModel.StatusType.Ok;
            return failureMessage;

        }
        internal Project.Response.VoucherDetails.VoucherDetailsWrapper SelectVoucherDetailsReport(Project.Requests.VoucherDetails.VoucherDetailsSelectRequest selectRequest, out string failureMessage)
        {
            Project.Response.VoucherDetails.VoucherDetailsWrapper wrapper = new Project.Response.VoucherDetails.VoucherDetailsWrapper();

            List<Project.Response.VoucherDetails.ViewModel> report = new List<Project.Response.VoucherDetails.ViewModel>();

            failureMessage = selectRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                report = new VoucherDetailsDal().SelectVoucherDetailsReport(selectRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }
            wrapper.Count = report.Count;

            if (!selectRequest.IsExcelDownload)
            {
                report = report.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();
            }

            wrapper.list = report;
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

    }
}
