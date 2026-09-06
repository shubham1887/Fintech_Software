using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.DMTBank;
using Project.Response.DMTBank;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class DMTBankBAL
    {
        private readonly DMTBankDAL ObjDal = new DMTBankDAL();

        internal List<ViewModel> Select()
        {
            List<ViewModel> list = ObjDal.Select();
            return list;
        }

        internal BankViewWrapper SelectAll(DMTBankReportRequest mainRequest, out string failureMessage)
        {
            BankViewWrapper wrapper = new BankViewWrapper();
            List<BankViewModel> viewModel = null;
            try
            {
                viewModel = new DMTBankDAL().SelectAll();
                if (viewModel == null)
                {
                    failureMessage = DMTBankMessage.NoBankFound;
                    return wrapper;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return wrapper;
            }
            wrapper.Count = viewModel.Count;
            viewModel = viewModel.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            if (!mainRequest.IsExcelDownload)
            {
                viewModel = viewModel.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = viewModel;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        public string ActiveDeactive(ActiveDeactiveRequest Request)
        {
            string failureMessage = Request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            DMTBank ObjModel;
            try
            {
                ObjModel = new DMTBankDAL().SelectByID(Request.DMTBankID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return Message.DMTBankMessage.NoBankFound;

            ObjModel.IsActive = Request.IsActive; int Result;
            try
            {
                Result = new DMTBankDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0)
                return Message.DMTBankMessage.UpdateFailure;

            return StatusType.Ok;
        }

        public string IMPSActiveDeactive(ActiveDeactiveRequest Request)
        {
            string failureMessage = Request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            DMTBank ObjModel;
            try
            {
                ObjModel = new DMTBankDAL().SelectByID(Request.DMTBankID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return Message.DMTBankMessage.NoBankFound;

            ObjModel.IsIMPS = Request.IsIMPS; int Result;
            try
            {
                Result = new DMTBankDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0)
                return Message.DMTBankMessage.UpdateFailure;

            return StatusType.Ok;
        }

        public string IMPSDeactiveByIFSC(string DefaultIFSC)
        {
            DMTBank ObjModel;
            try
            {
                ObjModel = new DMTBankDAL().SelectByIFSC(DefaultIFSC);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return Message.DMTBankMessage.NoBankFound;

            ObjModel.IsIMPS = false;
            int Result;
            try
            {
                Result = new DMTBankDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
                return Message.DMTBankMessage.UpdateFailure;

            return StatusType.Ok;
        }

        internal string Update(UpdateRequest updateRequest, out string failureMessage)
        {
            failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            DMTBank model = null;
            try
            {
                model = new DMTBankDAL().SelectByID(updateRequest.DMTBankID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (model == null)
                return Message.DMTBankMessage.NoBankFound;

            model.UpdatedDOC = DateTime.Now;
            model.DefaultIFSC = updateRequest.DefaultIFSC;

            failureMessage = Message.DMTBankMessage.UpdateFailureIFSC;
            int Result = -1;
            try
            {
                Result = new DMTBankDAL().Update(model);
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
            return Message.DMTBankMessage.UpdateSuccess;
        }

        internal string UpdateAll(List<UpdateRequest> updateRequest)
        {
            string failureMessage = string.Empty;
            foreach (UpdateRequest item in updateRequest)
            {
                failureMessage = Update(item, out failureMessage);
                if (!failureMessage.EqualsIgnoreCase(DMTBankMessage.UpdateSuccess)) return failureMessage;
            }
            failureMessage = StatusType.Ok;
            return failureMessage;
        }
    }
}