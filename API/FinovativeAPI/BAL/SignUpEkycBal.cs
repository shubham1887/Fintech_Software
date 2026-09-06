using Newtonsoft.Json;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.eKYC;
using Project.Response.eKYC;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class SignUpEkycBal
    {
        internal SignUp_eKYCInitiateResponse SignUPeKYCInitiate(SignUP_eKYCInitiateRequest mainRequest, out string failureMessage, string IPAddress)
        {
            SignUp_eKYCInitiateResponse response = new SignUp_eKYCInitiateResponse();
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            Response.eKYCSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new eKYCSettingDal().SelectActiveeKYC();
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
            UserWalletMaster userWallet;

            string RequestID = new Util().GetRandomNumberWithDatePrefix();
            if (RequestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }
            RequestID = RequestID.Substring(0, 10);
            if (RequestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }
            if (viewModel.Name.EqualsIgnoreCase(eKycCode.Quick))
            {
                Response.Quickekyc.AadharGenerateOtpResponse apiResponse = new Processing.Quickekyc.eKycProcessing().Quick_AadharGenerateOtp(viewModel.Token, mainRequest.Aadhaar);

                if (apiResponse.Status.EqualsIgnoreCase(StatusType.Failure))
                {
                    failureMessage = apiResponse.Message;
                    return response;
                }

                string resStr = JsonConvert.SerializeObject(apiResponse);
                string reqStr = JsonConvert.SerializeObject(mainRequest);

                int ID = new eKYCMasterBal().Insert(0, 0, RequestID, apiResponse.ClientID, reqStr, resStr);
                response.eKYCMasterID = ID;
                response.Message = "OTP sent on your Aadhar linked mobile number.";
                failureMessage = StatusType.Ok;
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal Signup_eKYCValidateResponse SignUpeKYCValidate(Signup_eKYCValidateRequest mainRequest, out string failureMessage)
        {
            Signup_eKYCValidateResponse response = new Signup_eKYCValidateResponse();

            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;


            Response.eKYCSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new eKYCSettingDal().SelectActiveeKYC();
            }

            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return response;
            }
            if (viewModel == null)
            {
                failureMessage = eKYCMessage.NotActive;
                return response;
            }
            if (viewModel.IsActive == false)
            {
                failureMessage = eKYCMessage.NotActive;
                return response;
            }

            eKYCMaster objModel = new eKYCMasterDal().SelectByID(mainRequest.eKYCMasterID);

            if (objModel == null)
            {
                failureMessage = "eKYC ID not found";
                return response;
            }

            if (viewModel.Name.EqualsIgnoreCase(eKycCode.Quick))
            {
                Response.Quickekyc.AadharVerifyOtpResponse aadharResponse = new Processing.Quickekyc.eKycProcessing().Quick_AadharVerifyOtp(viewModel.Token, objModel.OTPGenerateHash, mainRequest.OTP);

                if (!aadharResponse.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    failureMessage = aadharResponse.Message;
                    return response;
                }
                if (aadharResponse.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    failureMessage = StatusType.Ok;
                    response.Message = aadharResponse.Message;
                    response.Address = aadharResponse.Address;
                    response.Pin = aadharResponse.Pin;
                    response.FullName = aadharResponse.FullName;
                    return response;
                }
            }

            failureMessage = StatusType.Ok;

            return response;
        }
    }
}
