using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.LapuPartyMaster;
using Project.Response.LapuPartyMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class LapuPartyMasterBal
    {
        public string Insert(InsertRequest mainRequest, int UserID, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            LapuPartyMaster objModel = new LapuPartyMasterDal().SelectDuplicate(mainRequest);

            failureMessage = Message.LapuPartyMasterMessage.AlreadyExist;

            if (objModel != null) return "";

            failureMessage = Message.LapuPartyMasterMessage.InsertFail;
            StateMaster state = null;
            try
            {
                state = new StateDAL().SelectByID(mainRequest.StateID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = Message.LapuPartyMasterMessage.InvalidState;
            if (state == null) return "";

            objModel = new LapuPartyMaster
            {
                State = state.Name,
                PartyName = mainRequest.PartyName,
                Address = mainRequest.Address,
                EmailID = mainRequest.EmailID,
                MobileNumber = mainRequest.MobileNumber,
                City = mainRequest.City,
                IsActive = true
            };

            int Result;

            try
            {
                Result = new LapuPartyMasterDal().Insert(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return Message.LapuPartyMasterMessage.InsertSuccess;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new LapuPartyMasterDal().Select();
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
                response = new LapuPartyMasterDal().SelectByMasterID(mainRequest.LapuPartyMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = LapuPartyMasterMessage.NotFound;
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Update(UpdateRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            LapuPartyMaster ObjModel = null;

            ObjModel = new LapuPartyMasterDal().SelectByID(mainRequest.LapuPartyMasterID);

            if (ObjModel == null)
                return Message.LapuPartyMasterMessage.InvalidLapuParty;

            ObjModel.PartyName = mainRequest.PartyName;
            ObjModel.MobileNumber = mainRequest.MobileNumber;
            ObjModel.EmailID = mainRequest.EmailID;
            ObjModel.Address = mainRequest.Address;
            ObjModel.City = mainRequest.City;

            failureMessage = Message.LapuPartyMasterMessage.UpdateFailure;
            int Result = -1;
            try
            {
                Result = new LapuPartyMasterDal().Update(ObjModel);
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
            return Message.LapuPartyMasterMessage.UpdateSuccess;
        }

        internal string Delete(DeleteRequest deleteRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            LapuPartyMaster lapumodel = null;

            try
            {
                lapumodel = new LapuPartyMasterDal().SelectByID(deleteRequest.LapuPartyMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (lapumodel == null) return Message.LapuPartyMasterMessage.NotFound;

            failureMessage = Message.LapuPartyMasterMessage.DeleleFail;

            lapumodel.IsActive = false;

            int Result = -1;
            try
            {
                Result = new LapuPartyMasterDal().Update(lapumodel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (Result <= 0) return response;

            response = Message.LapuPartyMasterMessage.DeleleSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }
    }
}