using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.SignUp;
using Project.Response.FailedSignUpData;
using Project.Response.SignUp;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class SignUpBal
    {
        internal GenerateOTPResponse GenerateSignUpOTP(SignUpRequest request, string IPAddress, out string failureMessage)
        {
            GenerateOTPResponse response = new GenerateOTPResponse();
            failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            string OnbordingMessage = new SignUpDal().CheckOnbordingPending(request.MobileNumber, request.PANNo);
            if (!OnbordingMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.RedirectUrl = OnbordingMessage;
                failureMessage = StatusType.Ok;
                return response;
            }
            bool IsDuplicate = new SignUpDal().CheckDuplicate(request.MobileNumber,request.PANNo);
            if (IsDuplicate)
            {
                failureMessage = Message.SignUpMessage.DuplicateNumber;
                return response;
            }

            
            failureMessage = SignUpMessage.InvalidOTP;
            response.OTPMasterID = new MessageSelection().SendSignUPPasswordOTP(request.MobileNumber);
            if (response.OTPMasterID <= 0)
                return response;

            failureMessage = StatusType.Ok;
            response.Message = Message.UserMessage.SignUpOTP;
            response.Status = StatusType.Ok;

            return response;
        }

        internal VerifyOTPResponse VerifySignUpOTP(SignUpVerifyRequest request, string IPAddress, out string failureMessage)
        {
            VerifyOTPResponse response = new VerifyOTPResponse();
            failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;
            if (!IsValidOTP(request.OTPMasterID, request.OTP, request.MobileNumber))
            {
                failureMessage = Message.UserMessage.InvalidOTP;
                return response;
            }

            bool Pincode = new Location_PinCodeMasterDal().CheckPinCode(request.PinCode);
            if (Pincode == true)
            {
                FailedSignUpDataResponse res = new FailedSignUpDataBal().Insert(request, out failureMessage);
                return response;
            }

            response.Status = StatusType.Success;
            failureMessage = StatusType.Ok;
            return response;
        }


        internal VerifyOTPResponse SignUp(SignUpVerifyRequest request, string IPAddress, out string failureMessage)
        {
            VerifyOTPResponse response = new VerifyOTPResponse();
            failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            Response.eKYCSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new eKYCSettingDal().SelectActiveeKYCByName(eKycCode.Idfy);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            failureMessage = eKYCMessage.NotActive;
            if (viewModel == null)
                return response;

            if (viewModel.IsActive == false)
            {
                return response;
            }
            List<SignUp> list = new SignUpDal().SelectByOnBoarding(request.MobileNumber, request.PinCode);
            var Count = list.Count() + 1;


            string OrderID = $"{request.MobileNumber}_{request.PinCode}_{Count}";
            if (OrderID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }
            CreateProfileAPIRequest createProfileAPIRequest = new CreateProfileAPIRequest();

            createProfileAPIRequest.reference_id = OrderID;
            createProfileAPIRequest.config.id = viewModel.MerchantID;

            createProfileAPIRequest.data.ind_pan_details.id_number = request.PANNo;
            if (request.EmployeeID != string.Empty)
            {
                createProfileAPIRequest.data.additional_details.Add(new AdditionalDetails
                {
                    category = "employee_code",
                    value = request.EmployeeID,
                    page = "1",
                    type = "text"
                });
            }
            if (request.ReferralCode != string.Empty)
            {
                createProfileAPIRequest.data.additional_details.Add(new AdditionalDetails
                {
                    category = "referral_code",
                    value = request.ReferralCode,
                    page = "1",
                    type = "text"
                });
            }

            createProfileAPIRequest.data.additional_details.Add(new AdditionalDetails
            {
                category = "mobile_number",
                value = request.MobileNumber,
                page = "1",
                type = "text"
            });


            CreateProfileResponse apiResponse = new Processing.Idfy.Processing().CreateProfile(createProfileAPIRequest, viewModel.AuthKey, viewModel.Token, viewModel.MerchantID);

            if (apiResponse == null)
            {
                failureMessage = "Profile not created";
                return response;
            }
            if (apiResponse.Status != StatusType.Success)
            {
                failureMessage = apiResponse.Message;
                return response;
            }

            SignUp model = new SignUp
            {
                ReferralCode = request?.ReferralCode ?? "",
                EmployeeID = request?.EmployeeID ?? "",
                OrderID = OrderID,
                RedirectURL = apiResponse?.RedirectUrl ?? "",
                Status = StatusType.Process,
                ProfileID = apiResponse?.ProfileID,
                MobileNumber = request.MobileNumber,
                PANNo = request.PANNo,
                PinCode = request.PinCode,
            };

            int rowsAfftected = new SignUpDal().Insert(model);
            if (rowsAfftected <= 0)
            {
                failureMessage = SignUpMessage.InsertFailure;
                return response;
            }
            response.RedirectUrl = apiResponse.RedirectUrl;
            response.Status = StatusType.Success;
            response.Message = SignUpMessage.InsertSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }
        private bool IsValidOTP(int OTPMasterID, string OTP, string Mobile)
        {
            OTPMaster otpMaster = new OTPMaster();
            try
            {
                otpMaster = new OTPMasterDal().SelectOtp(OTPMasterID, OTP, Mobile);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }

            if (otpMaster == null)
                return false;

            if (!otpMaster.OTP.EqualsIgnoreCase(OTP))
                return false;

            int Result;

            otpMaster.IsVerified = true;

            try
            {
                Result = new OTPMasterDal().Update(otpMaster);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return true;

        }

        internal string UpdateStatus(string Mobile, string OrderID, string ProfileID,string Status)
        {
            string failureMessage = string.Empty;
            SignUp signUp = new SignUpDal().SelectByDetailsWithProfileID(Mobile, OrderID, ProfileID);
            if (signUp == null)
            {
                failureMessage = Message.SignUpMessage.InsertFailure;
                return failureMessage;
            }
            signUp.Status = Status;
            signUp.UpdatedDOC = DateTime.Now;
            int Result = new SignUpDal().Update(signUp);
            if (Result < 0)
            {
                failureMessage = Message.SignUpMessage.InsertFailure;
                return failureMessage;
            }
            failureMessage = StaticModel.StatusType.Ok;
            return failureMessage;
        }

        internal Response.SignUp.UserWrapper SelectReport(SelectReportRequest mainRequest, out string failureMessage)
        {
            Response.SignUp.UserWrapper wrapper = new Response.SignUp.UserWrapper();
            failureMessage = "";
            List<SelectReportResponse> list = new List<SelectReportResponse>();

            try
            {
                list = new SignUpDal().SelectReport(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }


            failureMessage = UserMessage.DataNotFound;
            if (list.Count <= 0) return wrapper;

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            wrapper.Count = list.Count();
            if (!mainRequest.IsExcelDownload)
            {
                list = list.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;
            failureMessage = StatusType.Ok;
            return wrapper;
        }
    }
}
