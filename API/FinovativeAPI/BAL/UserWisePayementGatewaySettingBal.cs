using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.PaymentGateway;
using Project.Response.PaymentGateway;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UserWisePayementGatewaySettingBal
    {
        internal string Insert(UserwisePaymentGatewayRequest insertRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            try
            {
                UserWisePayementGatewaySetting gateway = new UserWisePayementGatewaySettingDal().SelectForDuplicate(insertRequest.UserID);
                if (gateway != null)
                {
                    return failureMessage = PaymentGatewayMessage.Duplicate;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            UserWisePayementGatewaySetting ObjModel = new UserWisePayementGatewaySetting
            {
                UserID = insertRequest.UserID,
                PaymentGatewaySettingID = insertRequest.PaymentGatewaySettingID,
                MinimumAmount = insertRequest.MinimumAmount,
                MaximumAmount = insertRequest.MaximumAmount,
            };

            try
            {
                failureMessage = PaymentGatewayMessage.InsertFail;
                int rowsAffected = new UserWisePayementGatewaySettingDal().Insert(ObjModel);
                if (rowsAffected <= 0) return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            failureMessage = StatusType.Ok;
            return PaymentGatewayMessage.InsertSuccess;
        }

        internal List<UserwiseGatewayViewModel> Select()
        {
            List<UserwiseGatewayViewModel> ReturnList = null;
            try
            {
                ReturnList = new UserWisePayementGatewaySettingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }

        internal UserwiseGatewayViewModel SelectByID(SelectByUserwiseIDRequest selectRequest, out string failureMessage)
        {
            UserwiseGatewayViewModel Details = new UserwiseGatewayViewModel();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return Details;
            try
            {
                Details = new UserWisePayementGatewaySettingDal().SelectByUserWisePayementGatewaySettingID(selectRequest.UserWisePayementGatewaySettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return Details;
        }

        internal string Delete(SelectByUserwiseIDRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserWisePayementGatewaySetting ObjModel = null;
            try
            {
                ObjModel = new UserWisePayementGatewaySettingDal().SelectByID(deleteRequest.UserWisePayementGatewaySettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return PaymentGatewayMessage.DataNotFound;
            }

            ObjModel.UserWisePayementGatewaySettingID = deleteRequest.UserWisePayementGatewaySettingID;

            try
            {
                int rowsAffected = new UserWisePayementGatewaySettingDal().Delete(ObjModel);
                if (rowsAffected <= 0)
                {
                    return PaymentGatewayMessage.DeleteFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string Update(UserwisePaymentGatewayRequest insertRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserWisePayementGatewaySetting ObjModel = null;
            try
            {
                ObjModel = new UserWisePayementGatewaySettingDal().SelectByID(insertRequest.UserWisePayementGatewaySettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return PaymentGatewayMessage.DataNotFound;
            }

            ObjModel.UserID = insertRequest.UserID;
            ObjModel.PaymentGatewaySettingID = insertRequest.PaymentGatewaySettingID;
            ObjModel.MinimumAmount = insertRequest.MinimumAmount;
            ObjModel.MaximumAmount = insertRequest.MaximumAmount;
            ObjModel.UpdatedDOC = DateTime.Now;

            try
            {
                failureMessage = PaymentGatewayMessage.UpdateFail;
                int rowsAffected = new UserWisePayementGatewaySettingDal().Update(ObjModel);
                if (rowsAffected <= 0) return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            failureMessage = StatusType.Ok;
            return PaymentGatewayMessage.UpdateSuccess;
        }

        internal UserwiseGatewayViewModel SelectPGByUserID(int UserID)
        {
            int ParentID = 0, SuperParentID = 0;
            UserwiseGatewayViewModel ObjModel = null;
            try
            {
                ObjModel = new UserWisePayementGatewaySettingDal().SelectPGByUserID(UserID);
                if (ObjModel != null)
                    return ObjModel;

                ParentID = new UserDAL().SelectParentID(UserID);
                if (ParentID == Company.AdminUserID)
                    return ObjModel;

                ObjModel = new UserWisePayementGatewaySettingDal().SelectPGByUserID(ParentID);
                if (ObjModel != null)
                    return ObjModel;

                SuperParentID = new UserDAL().SelectParentID(ParentID);
                if (ParentID == Company.AdminUserID)
                    return ObjModel;

                ObjModel = new UserWisePayementGatewaySettingDal().SelectPGByUserID(SuperParentID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return ObjModel;
        }
    }
}