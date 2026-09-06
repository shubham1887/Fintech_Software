using Newtonsoft.Json;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.eKYC;
using Project.Response.eKYC;
using Project.StaticModel;
using Projet.Response.eKYC;
using System;
using System.Xml;
using YesBankCore.Response.KYC;

namespace Project.BAL
{
    public class eKYCBal
    {
        private readonly string urlWithParam = "https://oneproject7.consisty.com/ZoopeKYC/GetPANDetails?AgencyID=[ZoopeKYCAgenecyID]&APIKey=[ZoopeKYCAPIKey]&PanNumber=[PanNumber]";

        internal VerifyPanResponse VerifyPan(VerifyPanRequest mainRequest, out string failureMessage)
        {
            VerifyPanResponse response = new VerifyPanResponse();

            string ZoopeKYCAgenecyID = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ZoopeKYCAgenecyID), "");
            string ZoopeKYCAPIKey = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ZoopeKYCAPIKey), "");

            if (ZoopeKYCAgenecyID.Equals("") && ZoopeKYCAPIKey.Equals(""))
            {
                failureMessage = "Invalid crednitals";
                return response;
            }

            string url = urlWithParam;
            url = url.Replace("[ZoopeKYCAgenecyID]", ZoopeKYCAgenecyID);
            url = url.Replace("[ZoopeKYCAPIKey]", ZoopeKYCAPIKey);
            url = url.Replace("[PanNumber]", mainRequest.PanNumber);

            string HttpResponse = string.Empty;
            try
            {
                HttpResponse = HttpProcessing.CallGetMethod(url);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            response = JsonConvert.DeserializeObject<VerifyPanResponse>(HttpResponse);

            if (response.Message.Contains("Request is from invalid IP"))
            {
                failureMessage = response.Message;
                return response;
            }

            if (response.Status.EqualsIgnoreCase(StatusType.Failure))
            {
                failureMessage = response.Message;
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }




        internal GSTViewResponse GSTView(GSTViewRequest mainRequest, int UserID, out string failureMessage)
        {
            GSTViewResponse response = new GSTViewResponse();

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


            failureMessage = eKYCMessage.NotActive;
            return response;


        }



        


        internal Yes_eKYCInitiateResponse YesBankEkycInitiate(int UserID)
        {
            Yes_eKYCInitiateResponse response = new Yes_eKYCInitiateResponse();
            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);
            if (user == null)
            {
                response.Message = UserMessage.InvalidUser;
                return response;
            }
            if (user.IseKYC || user.IsKYCApproved)
            {
                response.Message = UserMessage.InvalideKYC;
                return response;
            }
            Response.eKYCSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new eKYCSettingDal().SelectActiveeKYCByName(eKycCode.YesBank);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            response.Message = eKYCMessage.NotActive;
            if (viewModel == null)
                return response;

            if (viewModel.IsActive == false)
            {
                return response;
            }
            UserWalletMaster userWallet;
            userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, (int)WalletCode.Recharge);

            string reqStr = "";
            (CreateAgentResponse apiResponse, reqStr) = new Processing.YesBank.KYCProcessing().KYCInitiate(user);

            if (apiResponse.Status.EqualsIgnoreCase(StatusType.Failure))
            {
                response.Message = apiResponse.Message;
                return response;
            }

            string resStr = JsonConvert.SerializeObject(apiResponse);

            eKYCMaster eKYCMasterdetails = new eKYCMasterDal().SelectByUserID(UserID);
            if (eKYCMasterdetails != null)
            {
                eKYCMaster objModel = new eKYCMasterDal().SelectByID(eKYCMasterdetails.eKYCMasterID);
                objModel.OTPToken = apiResponse.OTPToken;
                int Result = new eKYCMasterDal().Update(objModel);
                if (Result <= 0)
                {
                    response.Message = Message.eKYCMessage.EkycFailed;
                    return response;
                }
                response.Message = StatusType.Ok;
                return response;

            }
            int ID = new eKYCMasterBal().Insert(Company.InstantPayPayoutProviderID, UserID, apiResponse.OTPToken, apiResponse.RequestID, reqStr, resStr);
            if (ID <= 0)
            {
                response.Message = Message.eKYCMessage.EkycFailed;
                return response;
            }
            response.Message = StatusType.Ok;
            return response;
        }

        internal Yes_eKYCValidateResponse YesBankEkycValidate(Requests.eKYC.eKYCValidateRequest mainRequest, int UserID)
        {
            Yes_eKYCValidateResponse response = new Yes_eKYCValidateResponse();
            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);
            if (user == null)
            {
                response.Message = UserMessage.InvalidUser;
                return response;
            }
            if (user.IseKYC || user.IsKYCApproved)
            {
                response.Message = UserMessage.InvalideKYC;
                return response;
            }

            eKYCMaster eKYCMasterdetails = new eKYCMasterDal().SelectByUserID(UserID);

            if (eKYCMasterdetails == null)
            {
                response.Message = Message.eKYCMessage.OtpTokenNotFound;
                return response;
            }
            mainRequest.otpToken = eKYCMasterdetails.OTPToken;
            mainRequest.RequestID = eKYCMasterdetails.OrderID;
            OTPValidateResponse apiResponse = new Processing.YesBank.KYCProcessing().OTPValidate(mainRequest, user);

            if (apiResponse.Status.EqualsIgnoreCase(StatusType.Failure))
            {
                response.Message = apiResponse.Message;
                return response;
            }

            eKYCMaster objModel = new eKYCMasterDal().SelectByID(eKYCMasterdetails.eKYCMasterID);
            objModel.KYCToken = apiResponse.KYCToken;
            int Result = new eKYCMasterDal().Update(objModel);
            if (Result <= 0)
            {
                response.Message = Message.eKYCMessage.EkycFailed;
                return response;
            }
            GetAadharWadhResponse aadharWadhResponse = new Processing.YesBank.KYCProcessing().AadharWadh(eKYCMasterdetails.OrderID, apiResponse.KYCToken, user);

            if (aadharWadhResponse.Status.EqualsIgnoreCase(StatusType.Failure))
            {
                response.Message = apiResponse.Message;
                return response;
            }
            eKYCMaster updatewadhobjModel = new eKYCMasterDal().SelectByID(eKYCMasterdetails.eKYCMasterID);
            updatewadhobjModel.wadh = aadharWadhResponse.Wadh;
            int updateWadhResult = new eKYCMasterDal().Update(updatewadhobjModel);
            if (updateWadhResult <= 0)
            {
                response.Message = Message.eKYCMessage.EkycFailed;
                return response;
            }
            response.Message = StatusType.Ok;
            return response;
        }
        internal Yes_eKYCValidateResponse YesBankBioMatricVerification(Project.Requests.eKYC.YesBank_AadharBiometricRequest mainRequest, int UserID)
        {
            Yes_eKYCValidateResponse response = new Yes_eKYCValidateResponse();
            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);
            if (user == null)
            {
                response.Message = UserMessage.InvalidUser;
                return response;
            }
            if (user.IseKYC || user.IsKYCApproved)
            {
                response.Message = UserMessage.InvalideKYC;
                return response;
            }
            eKYCMaster eKYCMasterdetails = new eKYCMasterDal().SelectByUserID(UserID);
            if (eKYCMasterdetails.KYCToken == null)
            {
                response.Message = Message.eKYCMessage.KYCTokenNotFound;
                return response;
            }
            mainRequest.RequestID = eKYCMasterdetails.OrderID;
            mainRequest.KYCToken = eKYCMasterdetails.KYCToken;
            mainRequest.WadhValue = eKYCMasterdetails.wadh;
            mainRequest.AadharNo = mainRequest.AadharNo;
            string xmlData = mainRequest.FingerPrint;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            XmlNode paramNode = xmlDoc.SelectSingleNode("//Param[@name='sysid']");
            if (paramNode != null && paramNode.Attributes["value"] != null)
            {
                mainRequest.SerialNumber = paramNode.Attributes["value"].Value;
            }
            AadharBioMetricKYCResponse apiResponse = new Processing.YesBank.KYCProcessing().AadharBiometricKYC(mainRequest, user);

            if (apiResponse.Status.EqualsIgnoreCase(StatusType.Failure))
            {
                response.Message = apiResponse.Message;
                return response;
            }
            eKYCMaster objModel = new eKYCMasterDal().SelectByID(eKYCMasterdetails.eKYCMasterID);

            int Result = new eKYCMasterDal().Update(objModel);
            if (Result <= 0)
            {
                response.Message = Message.eKYCMessage.EkycFailed;
                return response;
            }
            user.IseKYC = true;
            user.IsKYCApproved = true;
            new eKYCLogBal().Insert(UserID, mainRequest.AadharNo, eKYCDocType.Aadhar, JsonConvert.SerializeObject(apiResponse));

            new UserDAL().Update(user);
            string failureMessage = new ProviderUserMappingBal().Insert(UserID, Company.YesBankProviderID, UserKey.UidToken, apiResponse.Token);
            if (!failureMessage.EqualsIgnoreCase(StaticModel.StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }

            response.Message = StatusType.Ok;
            return response;

        }

        internal DateResponse GetEkycDate(int UserID)
        {
            DateResponse response = new DateResponse();
            eKYCMaster eKYCMasterdetails = new eKYCMasterDal().SelectByUserID(UserID);
            if (eKYCMasterdetails == null)
            {
                return response;
            }
            response.DateIntstring = eKYCMasterdetails.UpdatedDOC.ToString(Messages.DateShortIntegerFormat);
            response.Date = eKYCMasterdetails.UpdatedDOC.ToString(Messages.OnlyDateFormat);
            return response;
        }

    }
}
