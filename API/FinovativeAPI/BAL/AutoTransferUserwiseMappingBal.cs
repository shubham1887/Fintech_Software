using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.AutoTransferUserwiseMapping;
using Project.Response.AutoTransferUserwiseMapping;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class AutoTransferUserwiseMappingBal
    {
        internal GenerateOTPResponse GenerateOTP(GenerateOTPRequest mainRequest, int UserID, out string failureMessage)
        {
            GenerateOTPResponse response = new GenerateOTPResponse();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);

            if (user == null)
                return response;

            failureMessage = AutoTransferMessages.InValidUser;
            if (!Util.isRoleAdminOrSDORDist(user.UserTypeID))
                return response;

            Insert(mainRequest, UserID, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.IsOTPSent = false;
                response.Message = failureMessage;
                return response;
            }

            new MessageSelection().SendAutoBalanceTransferOTP(user);
            response.IsOTPSent = true;
            response.Message = AutoTransferMessages.OTPSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }

        internal GenerateOTPResponse GenerateOTPForUpdate(GenerateOTPRequest mainRequest, int UserID, out string failureMessage)
        {
            GenerateOTPResponse response = new GenerateOTPResponse();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);

            if (user == null)
                return response;

            failureMessage = AutoTransferMessages.InValidUser;
            if (!Util.isRoleAdminOrSDORDist(user.UserTypeID))
                return response;

            new MessageSelection().SendAutoBalanceTransferOTP(user);
            response.IsOTPSent = true;
            response.Message = AutoTransferMessages.OTPSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }

        public List<ViewModel> Select(int ParentID, out string failureMessage)
        {
            failureMessage = AutoTransferMessages.NotFound;

            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new AutoTransferUserwiseMappingDal().Select(ParentID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return list;
            }

            if (list == null || list.Count <= 0) return list;

            failureMessage = StatusType.Ok;
            return list;
        }

        public string Insert(GenerateOTPRequest mainRequest, int UserID, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            AutoTransferUserwiseMapping objModel = new AutoTransferUserwiseMappingDal().SelectByUserID(mainRequest.UserID);

            if (objModel != null)
            {
                if (objModel.IsOTPVerified == true)
                    failureMessage = Message.AutoTransferMessages.AlreadyExist;

                if (objModel.IsOTPVerified == false)
                    failureMessage = StatusType.Ok;

                return Message.AutoTransferMessages.InsertSuccess;
            }
            failureMessage = Message.AutoTransferMessages.InsertFail;

            objModel = new AutoTransferUserwiseMapping
            {
                UserID = mainRequest.UserID,
                ParentID = UserID,
                DMTAmountIncreasedBy = 0,
                DMTLowBalanceReached = 100000,
                RechargeAmountIncreasedBy = 0,
                RechargeLowBalanceReached = 100000,
                MaxOutstanding = 0,
                IsOTPVerified = false,
                OTP = "0"
            };

            int Result;

            try
            {
                Result = new AutoTransferUserwiseMappingDal().Insert(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return Message.AutoTransferMessages.InsertSuccess;
        }

        internal GenerateOTPResponse VerifyOTP(VerifyOTPRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            GenerateOTPResponse response = new GenerateOTPResponse();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            UserMaster user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);

            if (user == null)
                return response;

            bool IsValidOTP = new OTPMasterBal().IsValid(user.UserID, mainRequest.OTP, Enum.GetName(typeof(OTPType), OTPType.AutoBalanceTransferOTP));

            response.IsOTPSent = IsValidOTP;

            failureMessage = Messages.InvalidOTP;
            if (!IsValidOTP)
                return response;

            if (IsValidOTP)
            {
                AutoTransferUserwiseMapping objModel = null;
                objModel = new AutoTransferUserwiseMappingDal().SelectByUserID(mainRequest.SelectedUserID);

                if (objModel == null) return response;

                objModel.OTP = mainRequest.OTP;
                objModel.IsOTPVerified = true;

                try
                {
                    new AutoTransferUserwiseMappingDal().Update(objModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            response.Message = AutoTransferMessages.OTPVerifySuccess;
            failureMessage = StatusType.Ok;

            return response;
        }

        public string Update(UpdateRequest mainRequest, int UserID, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);

            if (user == null)
                return failureMessage = Message.AutoTransferMessages.InValidUser;

            bool IsValidOTP = new OTPMasterBal().IsValid(user.UserID, mainRequest.OTP, Enum.GetName(typeof(OTPType), OTPType.AutoBalanceTransferOTP));

            failureMessage = Messages.InvalidOTP;
            if (!IsValidOTP)
                return failureMessage;

            failureMessage = Message.AutoTransferMessages.UpdateFail;

            AutoTransferUserwiseMapping ObjModel = new AutoTransferUserwiseMappingDal().SelectByID(mainRequest.AutoTransferUserwiseMappingID);

            if (ObjModel == null) return failureMessage;

            if (UserID != ObjModel.ParentID) return failureMessage;

            ObjModel.DMTAmountIncreasedBy = mainRequest.DMTAmountIncreasedBy;
            ObjModel.DMTLowBalanceReached = mainRequest.DMTLowBalanceReached;
            ObjModel.RechargeAmountIncreasedBy = mainRequest.RechargeAmountIncreasedBy;
            ObjModel.RechargeLowBalanceReached = mainRequest.RechargeLowBalanceReached;
            ObjModel.MaxOutstanding = mainRequest.MaxOutstanding;
            ObjModel.UpdatedDOC = DateTime.Now;

            int Result;

            try
            {
                Result = new AutoTransferUserwiseMappingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return Message.AutoTransferMessages.UpdateSuccess;
        }

        internal string Delete(DeleteRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return "";

            failureMessage = AutoTransferMessages.DeleteFail;
            AutoTransferUserwiseMapping ObjModel = new AutoTransferUserwiseMappingDal().SelectByID(mainRequest.AutoTransferUserwiseMappingID);
            if (ObjModel == null) return "";

            int Result = -1; ;
            try
            {
                Result = new AutoTransferUserwiseMappingDal().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return AutoTransferMessages.DeleteSuccess;
        }
    }
}