using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.CallbackMaster;
using Project.Response.CallbackMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class CallbackMasterBal
    {
        internal string Insert(InsertRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            int UserID = keyDataModel.UserID;
            if (keyDataModel.UserID == Company.AdminUserID)
            {
                UserID = mainRequest.UserID;
            }

            CallbackMaster objModel = new CallbackMaster
            {
                UserID = UserID,
                CallbackURL = mainRequest.CallbackURL,
                ServiceID = mainRequest.ServiceID
            };

            ViewModel callbackMaster = null;
            try
            {
                callbackMaster = new CallbackMasterDal().SelectByUserIDAndServiceID(UserID, mainRequest.ServiceID);

                if (callbackMaster == null && mainRequest.CallbackMasterID > 0)
                {
                    callbackMaster = new CallbackMasterDal().SelectByID(mainRequest.CallbackMasterID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            int Result;

            if (callbackMaster == null)
            {
                response = Message.CallbackMasterMessage.InsertFail;
                try
                {
                    Result = new CallbackMasterDal().Insert(objModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return response; ;
                }
                if (Result > 0)
                    response = Message.CallbackMasterMessage.InsertSuccess;
                else
                    failureMessage = response;
            }
            else
            {
                response = Message.CallbackMasterMessage.UpdateFail;
                objModel.CallbackMasterID = callbackMaster.CallbackMasterID;
                objModel.CallbackURL = mainRequest.CallbackURL;

                try
                {
                    Result = new CallbackMasterDal().Update(objModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return response;
                }
                if (Result > 0)
                    response = Message.CallbackMasterMessage.UpdateSuccess;
                else
                    failureMessage = response;
            }
            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<ViewModel> Select(int userID, out string failureMessage)

        {
            failureMessage = "";
            List<ViewModel> list = null;

            try
            {
                list = new CallbackMasterDal().SelectByUserID(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (list == null)
            {
                failureMessage = StatusType.Ok;
                return list;
            }

            failureMessage = StatusType.Ok;
            return list;
        }

        internal string Delete(DeleteRequest mainRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;
            int Result;
            failureMessage = Message.CallbackMasterMessage.DeleleFail;
            try
            {
                Result = new CallbackMasterDal().Delete(mainRequest.CallbackMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response; ;
            }
            if (Result > 0)
                response = Message.CallbackMasterMessage.DeleleSuccess;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal ViewModel SelectByID(DeleteRequest mainRequest)
        {
            ViewModel modelObj = new ViewModel();

            try
            {
                modelObj = new CallbackMasterDal().SelectByID(mainRequest.CallbackMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return modelObj;
            }

            if (modelObj == null)
            {
                return modelObj;
            }
            return modelObj;
        }
    }
}