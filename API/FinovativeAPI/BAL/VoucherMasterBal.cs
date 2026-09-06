using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.VoucherMaster;
using Project.Response.VoucherMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class VoucherMasterBal
    {
        internal string Insert(Project.Requests.VoucherMaster.InsertRequest insertRequest, int UserID)
        {
            string failureMessage = insertRequest.IsValidModalState();


            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            Project.Model.VoucherMaster voucherMaster = new Project.Model.VoucherMaster();

            voucherMaster.VoucherTypeID = insertRequest.VoucherTypeID;
            voucherMaster.SelectDate = insertRequest.Date;
            voucherMaster.Amount = insertRequest.Amount;
            if (insertRequest.VoucherTypeID == (int)StaticModel.VoucherType.Receipt)
            {
                voucherMaster.UserID = insertRequest.UserID;
                voucherMaster.CrGroup = insertRequest.Mode;
                voucherMaster.CrName = insertRequest.CrName;
                voucherMaster.DrGroup = "Sundry Debitor";
                voucherMaster.DrName = insertRequest.DrName;
            }

            if (insertRequest.VoucherTypeID == (int)StaticModel.VoucherType.Contra)
            {
                voucherMaster.CrGroup = insertRequest.CrGroup;
                voucherMaster.CrName = insertRequest.CrName;
                voucherMaster.DrGroup = insertRequest.DrGroup;
                voucherMaster.DrName = insertRequest.DrName;
            }
            if (insertRequest.VoucherTypeID == (int)StaticModel.VoucherType.Payment)
            {
                voucherMaster.CrGroup = "Sundry Creditor";
                voucherMaster.CrName = insertRequest.CrName;
                voucherMaster.DrGroup = insertRequest.Mode;
                voucherMaster.DrName = insertRequest.DrName;
            }
            voucherMaster.Remark = insertRequest.Remark;
            voucherMaster.Status = StaticModel.StatusType.Process;
            voucherMaster.SubmitedBy = UserID;
            int Result = new VoucherMasterDAL().Insert(voucherMaster);

            if (Result < 0)
            {
                return Message.VoucherMasterMessage.VoucherMasterFailed;
            }
            failureMessage = StaticModel.StatusType.Ok;
            return failureMessage;

        }
        internal string Update(Project.Requests.VoucherMaster.UpdateRequest updateRequest, int UserID)
        {
            decimal OS_OpeningBalance = 0;
            decimal OS_ClosingBalance = 0;
            string failureMessage = updateRequest.IsValidModalState();


            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            Project.Model.VoucherMaster voucherMaster = new VoucherMasterDAL().SelectByID(updateRequest.VoucherMasterID);
            if(voucherMaster == null)
            {
                return Message.VoucherMessage.DataNotFound;
            }
            voucherMaster.ApproveBy = UserID;
            voucherMaster.Status = updateRequest.Status;
            voucherMaster.UpdatedDOC = DateTime.Now;



            int Result = new VoucherMasterDAL().Update(voucherMaster);

            if (Result < 0)
            {
                return Message.VoucherMasterMessage.VoucherMasterFailed;
            }
            CashCollectionMaster ObjModel = new CashCollectionMasterDal().SelectByUserID(voucherMaster.UserID);
            if (ObjModel == null)
            {
                return failureMessage;
            }
            if (voucherMaster.VoucherTypeID == (int)StaticModel.VoucherType.Receipt && !updateRequest.Status.EqualsIgnoreCase(StaticModel.StatusType.Reject))
            {
                failureMessage = new VoucherDetailsBal().VoucherDetailsInsert(voucherMaster.UserID, "", voucherMaster.Amount, voucherMaster.Remark, "", AmountCreditDebitType.Debit);
                if (!failureMessage.EqualsIgnoreCase(StaticModel.StatusType.Ok))

                    return failureMessage;

            }
            failureMessage = StaticModel.StatusType.Ok;
            return failureMessage;

        }
        internal string Reverse(Project.Requests.VoucherMaster.SelectByIDForVouchertRequest deleteRequest)
        {
            
            string failureMessage = deleteRequest.IsValidModalState();

             
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            Project.Model.VoucherMaster voucherMaster = new VoucherMasterDAL().SelectByID(deleteRequest.VoucherMasterID);
            if (voucherMaster == null)
            {
                return Message.VoucherMessage.DataNotFound;
            }
            voucherMaster.Remark = "Reverse Amount From Admin";
            voucherMaster.Is_Delete = true;
            voucherMaster.UpdatedDOC = DateTime.Now;



            int Result = new VoucherMasterDAL().Update(voucherMaster);

            if (Result < 0)
            {
                return Message.VoucherMasterMessage.VoucherMasterFailed;
            }
            CashCollectionMaster ObjModel = new CashCollectionMasterDal().SelectByUserID(voucherMaster.UserID);
            if (ObjModel == null)
            {
                return failureMessage;
            }
            if (voucherMaster.VoucherTypeID == (int)StaticModel.VoucherType.Receipt)
            {
                failureMessage = new VoucherDetailsBal().VoucherDetailsInsert(voucherMaster.UserID, "", voucherMaster.Amount, voucherMaster.Remark, AmountCreditDebitType.Credit, "");
                if (!failureMessage.EqualsIgnoreCase(StaticModel.StatusType.Ok))

                    return failureMessage;

            }
            failureMessage = StaticModel.StatusType.Ok;
            return failureMessage;

        }
        internal Project.Response.VoucherMaster.VoucherMasterWrapper SelectForVoucherReport(Project.Requests.VoucherMaster.VoucherMasterSelectRequest selectRequest, out string failureMessage)
        {
            Project.Response.VoucherMaster.VoucherMasterWrapper wrapper = new Project.Response.VoucherMaster.VoucherMasterWrapper();

            List<Project.Response.VoucherMaster.ViewModel> report = new List<Project.Response.VoucherMaster.ViewModel>();

            failureMessage = selectRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                report = new VoucherMasterDAL().SelectForVoucherReport(selectRequest);
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
            VoucherSummaryViewModel VoucherSummary = new VoucherMasterDAL().HeaderSummary(selectRequest);
            wrapper.VoucherSummary = VoucherSummary;
            wrapper.list = report;
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;

            failureMessage = StatusType.Ok;
            return wrapper;
        }
     
        internal Project.Response.VoucherMaster.VoucherMasterWrapper VoucherMasterApproveReport(Project.Requests.VoucherMaster.VoucherMasterSelectRequest selectRequest, out string failureMessage)
        {
            Project.Response.VoucherMaster.VoucherMasterWrapper wrapper = new Project.Response.VoucherMaster.VoucherMasterWrapper();

            List<Project.Response.VoucherMaster.ViewModel> report = new List<Project.Response.VoucherMaster.ViewModel>();

            failureMessage = selectRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            try
            {
                report = new VoucherMasterDAL().VoucherMasterApproveReport(selectRequest);
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
        internal (Project.Response.VoucherMaster.ViewModel, string) SelectByIDForVoucherApprove(Project.Requests.VoucherMaster.SelectByIDForVouchertRequest selectRequest)
        {
            Project.Response.VoucherMaster.VoucherMasterWrapper wrapper = new Project.Response.VoucherMaster.VoucherMasterWrapper();

            Project.Response.VoucherMaster.ViewModel report = new Project.Response.VoucherMaster.ViewModel();

            string failureMessage = selectRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return (report, failureMessage);
            try
            {
                report = new VoucherMasterDAL().SelectByIDForVoucherApprove(selectRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return (report, failureMessage);
            }
            failureMessage = StatusType.Ok;
            return (report, failureMessage);
        }



    }
}
