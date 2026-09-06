using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.BBPSConfiguration;
using Project.Response.BBPSConfiguration;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class BBPSConfigurationBal
    {
        internal string Insert(InsertRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            BBPSConfiguration ObjModel = new BBPSConfiguration
            {
                OperatorID = mainRequest.OperatorID,
                PlaceHolder = mainRequest.PlaceHolder,
                Label = mainRequest.Label,
                IsRequired = mainRequest.IsRequired,
                IsDisplay = mainRequest.IsDisplay,
                Type = mainRequest.Type,
                ErrorMeeage = mainRequest.ErrorMeeage,
                Sequence = mainRequest.Sequence,
                ConfigList = mainRequest.ConfigList
            };

            failureMessage = BBPSConfigurationMessages.InsertFail;
            int Result = -1; ;
            try
            {
                Result = new BBPSConfigurationDal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
                return failureMessage;

            failureMessage = StatusType.Ok;
            return BBPSConfigurationMessages.InsertSuccess;
        }

        internal List<ViewModel> Select(out string failureMessage)
        {
            failureMessage = BBPSConfigurationMessages.NotFound;
            List<ViewModel> response = new List<ViewModel>();
            try
            {
                response = new BBPSConfigurationDal().SelectAll();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response == null || response.Count <= 0) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<ViewModel> SelectByOperatorID(SelectByOperatorIDRequest mainRequest, out string failureMessage)
        {
            failureMessage = BBPSConfigurationMessages.NotFound;
            List<ViewModel> response = new List<ViewModel>();

            List<BBPSConfiguration> list = null;
            try
            {
                list = new BBPSConfigurationDal().SelectByOperatorID(mainRequest.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (list == null || list.Count <= 0)
            {
                return response;
            }

            foreach (BBPSConfiguration config in list)
            {
                response.Add(new ViewModel()
                {
                    BBPSConfigurationID = config.BBPSConfigurationID,
                    Label = config.Label,
                    PlaceHolder = config.PlaceHolder,
                    Type = config.Type,
                    IsRequired = config.IsRequired,
                    ErrorMeeage = config.ErrorMeeage,
                    Sequence = config.Sequence,
                    ConfigList = config.ConfigList,
                    OperatorID = config.OperatorID,
                    IsDisplay = config.IsDisplay,
                });
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel response = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            failureMessage = BBPSConfigurationMessages.NotFound;

            if (!IsAlreadyExists(mainRequest.ConfigurationID, out response))
                return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        private bool IsAlreadyExists(int BBPSConfigurationID, out ViewModel ObjModel)
        {
            ObjModel = new BBPSConfigurationDal().SelectByID(BBPSConfigurationID);

            return ObjModel != null;
        }

        internal string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return "";

            failureMessage = BBPSConfigurationMessages.NotFound;
            if (!IsAlreadyExists(mainRequest.ConfigurationID, out ViewModel ObjExistingModel))
                return "";

            //if (mainRequest.ConfigList != "")
            //{
            //    new BBPSConfigurationListBal().Insert(mainRequest.ConfigList);
            //}

            BBPSConfiguration ObjModel = new BBPSConfiguration
            {
                BBPSConfigurationID = mainRequest.ConfigurationID,
                OperatorID = mainRequest.OperatorID,
                PlaceHolder = mainRequest.PlaceHolder,
                Label = mainRequest.Label,
                IsRequired = mainRequest.IsRequired,
                IsDisplay = mainRequest.IsDisplay,
                Type = mainRequest.Type,
                ErrorMeeage = mainRequest.ErrorMeeage,
                Sequence = mainRequest.Sequence,
                ConfigList = mainRequest.ConfigList
            };

            failureMessage = BBPSConfigurationMessages.UpdateFail;
            int Result = -1; ;
            try
            {
                Result = new BBPSConfigurationDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return BBPSConfigurationMessages.UpdateSuccess;
        }

        internal string Delete(DeleteRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return "";

            failureMessage = BBPSConfigurationMessages.NotFound;
            if (!IsAlreadyExists(mainRequest.ConfigurationID, out ViewModel ObjExistingModel))
                return "";

            BBPSConfiguration ObjModel = new BBPSConfigurationDal().SelectByPrimaryID(mainRequest.ConfigurationID);

            failureMessage = BBPSConfigurationMessages.DeleteFail;
            int Result = -1; ;
            try
            {
                Result = new BBPSConfigurationDal().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return BBPSConfigurationMessages.DeleteSuccess;
        }

        internal BBPSWrapperResponse BBPSConfigurationByOperatorID(SelectByOperatorIDRequest mainRequest)
        {
            BBPSWrapperResponse wraperresponse = new BBPSWrapperResponse();
            wraperresponse.Status = StatusType.Failure;
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                wraperresponse.Message = failureMessage;
                return wraperresponse;
            }

            List<APIViewModel> response = new List<APIViewModel>();
            try
            {
                response = new BBPSConfigurationDal().BBPSConfigurationByOperatorID(mainRequest.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (response == null || response.Count <= 0)
            {
                wraperresponse.Message = BBPSConfigurationMessages.NotFound;
                return wraperresponse;
            }
            wraperresponse = new BBPSWrapperResponse()
            {
                Data = response,
                Message = BBPSConfigurationMessages.Success,
                Status = StatusType.Success
            };
            return wraperresponse;
        }
    }
}