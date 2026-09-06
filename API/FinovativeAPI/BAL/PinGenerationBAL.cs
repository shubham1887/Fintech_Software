using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.PinGeneration;
using Project.Response.PinGeneration;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class PinGenerationBAL
    {
        internal async Task<string> GeneratePinOTP(Requests.PinGeneration.PinGenerationRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            LoginResponse ReturnObj = null;

            updateRequest.Password = new Hashing().Hash(updateRequest.Password);
            try
            {
                ReturnObj = new UserDAL().SelectForMpinLogin(updateRequest.UserName, updateRequest.Password);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }

            if (ReturnObj == null)
            {
                failureMessage = UserMessage.InvalidUserNameOrPassword;
                return failureMessage;
            }

            if (updateRequest.Pin != updateRequest.ConfirmPin)
                return UserMessage.InvalidConfirmPin;

            if (IsInvalidPin(updateRequest.Pin))
                return UserMessage.PinValidation;

            List<ViewModel> response = null;
            try
            {
                response = new PinGenerationDal().SelectPin(ReturnObj.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = ValidatePinChange(response, updateRequest.Pin, updateRequest.DeviceID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            

            MPinHistory Obj = new MPinHistory
            {
                UserID = ReturnObj.UserID,
                Mpin = updateRequest.Pin,
                DeviceID = updateRequest.DeviceID,
                Device = updateRequest.Device
            };

            failureMessage = BBPSConfigurationMessages.InsertFail;
            int Result = -1;
            try
            {
                Result = new PinGenerationDal().Insert(Obj);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
                return failureMessage;

            failureMessage = Message.UserMessage.UserNotFound;
            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByID(ReturnObj.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return failureMessage;

            if(ObjModel.IsActive == false)
                return failureMessage;
            
            new MessageSelection().SendPinOTP(ObjModel);

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal async Task<string> ResendOTP(Requests.PinGeneration.PinGenerationRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            LoginResponse ReturnObj = null;

            updateRequest.Password = new Hashing().Hash(updateRequest.Password);
            try
            {
                ReturnObj = new UserDAL().SelectForMpinLogin(updateRequest.UserName, updateRequest.Password);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }

            if (ReturnObj == null)
            {
                failureMessage = UserMessage.InvalidUserNameOrPassword;
                return failureMessage;
            }

            if (updateRequest.Pin != updateRequest.ConfirmPin)
                return UserMessage.InvalidConfirmPin;

            if (IsInvalidPin(updateRequest.Pin))
                return UserMessage.PinValidation;
            
            failureMessage = Message.UserMessage.UserNotFound;
            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByID(ReturnObj.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return failureMessage;

            if(ObjModel.IsActive == false)
                return failureMessage;
            
            new MessageSelection().SendPinOTP(ObjModel);

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal VerifyOTPResponse VerifyPinOTP(PinVerifyRequest mainRequest, out string failureMessage)
        {
            VerifyOTPResponse verifyOTPResponse = new VerifyOTPResponse();
            failureMessage = Message.UserMessage.UserNotFound;

            UserMaster userMaster = null;
            try
            {
                userMaster = new UserDAL().SelectUserByMobileNumberSynchronization(mainRequest.MobileNumber);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return verifyOTPResponse;
            }

            if (userMaster == null)
            {
                verifyOTPResponse.Message = failureMessage;
                return verifyOTPResponse;
            }

            bool IsValidOTP = new OTPMasterBal().IsValid(userMaster.UserID, mainRequest.OTP, Enum.GetName(typeof(OTPType), OTPType.PinOTP));
            if (!IsValidOTP)
            {
                failureMessage = Message.UserMessage.InvalidOTP;
                verifyOTPResponse.Message = failureMessage;
                return verifyOTPResponse;
            }

            userMaster.Mpin = mainRequest.Pin;

            int Result;
            try
            {
                Result = new UserDAL().Update(userMaster);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return verifyOTPResponse;
            }
            if (Result <= 0)
            {
                verifyOTPResponse.Message = failureMessage;
                return verifyOTPResponse;
            }

            failureMessage = StatusType.Ok;

            verifyOTPResponse.Message = failureMessage;
            verifyOTPResponse.FirmName = userMaster.FirmName;
            return verifyOTPResponse;
        }

        internal async Task<string> ForgotMpinGenerateOTP(ForgotMpinRequest forgotMpinRequest)
        {
            string failureMessage = Message.UserMessage.UserNotFound;

            UserMaster userMaster = null;
            try
            {
                userMaster = await new UserDAL().SelectUserByMobileNumber(forgotMpinRequest.MobileNumber);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (userMaster == null) return failureMessage;

            if(userMaster.PANNo == forgotMpinRequest.PANNo)
                return failureMessage;

            failureMessage = StatusType.Ok;
            return failureMessage;
        }


        private string ValidatePinChange(List<ViewModel> response, int newPin, string deviceID)
        {
            int today = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));

            // Check pin changes today
            var pinChangesToday = response.Count(pin => pin.DOCInt == today && pin.DeviceID == deviceID);

            if (pinChangesToday >= 3)
            {
                return UserMessage.PinChangeLimitReached;
            }

            // Check if the pin is already used
            if (response.Any(pin => pin.Mpin == newPin && pin.DeviceID == deviceID))
            {
                return UserMessage.PinAlreadyUsed;
            }

            return StatusType.Ok;
        }


        private bool IsInvalidPin(int pin)
        {
            string pinString = pin.ToString();

            if (pinString.Length < 4)
                return false;

            bool isRepetitive = true;
            bool isSuccessive = true;

            char firstChar = pinString[0];

            for (int i = 0; i < pinString.Length; i++)
            {
                if (pinString[i] != firstChar)
                    isRepetitive = false;

                if (i > 0 && pinString[i] != pinString[i - 1] + 1)
                    isSuccessive = false;
            }

            return isRepetitive || isSuccessive;
        }

    }
}
