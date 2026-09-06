using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.LapuMaster;
using Project.Response.LapuMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class LapuMasterBal
    {
        public string Insert(InsertRequest mainRequest, int UserID, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            LapuMaster objModel = new LapuMasterDal().SelectByOperatorIDAndLapuNo(mainRequest.OperatorID, mainRequest.LapuNo);
            if (objModel != null)
                return Message.LapuMasterMessage.AlreadyExistsLapuParty;

            failureMessage = Message.LapuMasterMessage.InsertFail;
            objModel = new LapuMaster
            {
                LapuPartyMasterID = mainRequest.LapuPartyMasterID,
                LapuNo = mainRequest.LapuNo,
                ProivderLapuID = "0",
                OperatorID = mainRequest.OperatorID,
                ClosingBalance = 0,
            };

            int Result;

            try
            {
                Result = new LapuMasterDal().Insert(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            if (Result <= 0) return "";

            Task.Run(() =>
             {
                 new LapuMasterBal().UpdateLapuTransactionDetail(objModel, Result);
             }
            );

            failureMessage = StatusType.Ok;
            return Message.LapuMasterMessage.InsertSuccess;
        }

        private void UpdateLapuTransactionDetail(LapuMaster objModel, int LapuMasterID)
        {
            List<LapuTransactionDetail> List = new List<LapuTransactionDetail>();

            try
            {
                List = new LapuTransactionDetailDal().SelectByLapuNo(objModel.LapuNo);
                if (List == null)
                {
                    return;
                }

                foreach (LapuTransactionDetail item in List)
                {
                    item.LapuMasterID = LapuMasterID;
                    item.LapuPartyMasterID = objModel.LapuPartyMasterID;
                    new LapuTransactionDetailDal().Update(item);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new LapuMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel response = null;

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                response = new LapuMasterDal().SelectByMasterID(mainRequest.LapuMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = LapuMasterMessage.NotFound;
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Update(UpdateRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            LapuMaster ObjModel = null;
            ObjModel = new LapuMasterDal().SelectDuplicate(mainRequest.OperatorID, mainRequest.LapuNo, mainRequest.LapuMasterID);
            if (ObjModel != null)
                return Message.LapuMasterMessage.AlreadyExistsLapuParty;

            ObjModel = new LapuMasterDal().SelectByID(mainRequest.LapuMasterID);

            if (ObjModel == null)
                return Message.LapuMasterMessage.InvalidLapuParty;

            ObjModel.LapuNo = mainRequest.LapuNo;
            ObjModel.OperatorID = mainRequest.OperatorID;
            ObjModel.LapuPartyMasterID = mainRequest.LapuPartyMasterID;
            ObjModel.LapuNo = mainRequest.LapuNo;

            failureMessage = Message.LapuMasterMessage.UpdateFailure;
            int Result = -1;
            try
            {
                Result = new LapuMasterDal().Update(ObjModel);
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
            return Message.LapuMasterMessage.UpdateSuccess;
        }

        internal string Delete(DeleteRequest deleteRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            LapuMaster lapumodel = null;

            try
            {
                lapumodel = new LapuMasterDal().SelectByID(deleteRequest.LapuMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (lapumodel == null) return Message.LapuMasterMessage.NotFound;

            failureMessage = Message.LapuMasterMessage.DeleleFail;

            int Result = -1;
            try
            {
                Result = new LapuMasterDal().Delete(lapumodel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (Result <= 0) return response;

            response = Message.LapuMasterMessage.DeleleSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }
    }
}