using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.UserwiseServiceChargeMaster;
using Project.Response.UserwiseServiceChargeMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UserwiseServiceChargeMasterBal
    {
        internal List<ViewModel> Select()
        {
            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new UserwiseServiceChargeMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return list;
            }

            return list;
        }

        internal string Insert(InsertRequest insertRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserwiseServiceChargeMaster ObjModel = new UserwiseServiceChargeMaster
            {
                UserID = insertRequest.UserID,
                ServiceID = insertRequest.ServiceID,
                OneTime = insertRequest.OneTime,
                IsDelete = false
            };

            UserwiseServiceChargeMaster model = null;
            try
            {
                model = new UserwiseServiceChargeMasterDal().SelectByID(insertRequest.UserwiseServiceChargeMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            int rowsAffected;
            if (model == null)
            {
                try
                {
                    response = ServiceMessage.DuplicateChargeFailure;
                    model = new UserwiseServiceChargeMasterDal().SelectByUserID(insertRequest.UserID, insertRequest.ServiceID);
                    if (model != null)
                    {
                        failureMessage = response;
                        return response;
                    }
                    response = ServiceMessage.InsertChargeFailure;
                    rowsAffected = new UserwiseServiceChargeMasterDal().Insert(ObjModel);
                    if (rowsAffected > 0)
                    {
                        response = ServiceMessage.InsertChargeSuccess;
                    }
                    else
                    {
                        failureMessage = response;
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
                failureMessage = StatusType.Ok;
                return response;
            }

            response = ServiceMessage.UpdateFail;
            model.OneTime = insertRequest.OneTime;

            try
            {
                rowsAffected = new UserwiseServiceChargeMasterDal().Update(model);

                if (rowsAffected > 0)
                {
                    response = ServiceMessage.UpdateSuccess;
                }
                else
                {
                    failureMessage = response;
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Delete(SelectRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserwiseServiceChargeMaster ObjModel = null;
            try
            {
                ObjModel = new UserwiseServiceChargeMasterDal().SelectByID(deleteRequest.UserwiseServiceChargeMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ObjModel == null)
                return ServiceMessage.NotFound;

            ObjModel.IsDelete = true;
            ObjModel.UpdatedDOC = DateTime.Now;

            try
            {
                int rowsAffected = new UserwiseServiceChargeMasterDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return ServiceMessage.DeleteChargeFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }
    }
}