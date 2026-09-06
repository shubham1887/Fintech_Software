using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing.YesBank;
using Project.Requests;
using Project.Requests.Utility;
using Project.Response.Provider;
using Project.Response.ProviderResponse;
using Project.Response.Utility;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class UtilityProcessing
    {
        internal static FetchViewModel Fetch(BillFetchRequest mainRequest, KeyDataModel keyDataModel, string IPAddress, out string failureMessage)
        {
            FetchViewModel response = new FetchViewModel();

            failureMessage = mainRequest.IsValidModalState();
            response.Message = failureMessage;
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            UserMaster user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);


            if (Company.BillFetchValidation.EqualsIgnoreCase("Yes"))
            {
                UtilityProcessing utilityProcessing = new UtilityProcessing();
                string statusMessage = utilityProcessing.IsCheckValidUser(user.UserID);
                if (statusMessage != StatusType.Ok)
                {
                    response.Message = statusMessage;
                    return response;
                }
            }

            // Check here for BBPS Configuration
            bool IssBBPS = new OperatorMasterBAL().IsBBPSOperator(mainRequest.OperatorID);

            if (IssBBPS)
            {
                failureMessage = FunctionalValidationProcessing.IsValidRequestForBBPS(mainRequest.OperatorID, mainRequest.Param1, mainRequest.Param2, mainRequest.Param3, mainRequest.Param4, mainRequest.Param5);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    response.Message = failureMessage;
                    return response;
                }
            }

            int ProviderID = -1;
            try
            {
                ProviderID = new ViewProviderMappingDal().SelectProviderID(mainRequest.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = ProviderMessage.BillFetchAPINotSet;
            response.Message = failureMessage;
            if (ProviderID <= 0)
                return response;

            failureMessage = ProviderMessage.BillFetchUrlNotFound;
            response.Message = failureMessage;

            Response.Provider.ViewModel providerMaster = new ProviderMasterDAL().SelectByProviderID(ProviderID);
            if (providerMaster == null)
            {
                return response;
            }
            DetailsViewModel objModel = null;
            int ServiceID = new OperatorMasterBAL().SelectServiceID(mainRequest.OperatorID);
            try
            {
                objModel = new ProviderDetailsDAL().GetBillFetchURLByServiceID(ProviderID, ServiceID);
                if (objModel == null)
                    objModel = new ProviderDetailsDAL().GetBillFetchURL(ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            if (objModel == null) return response;

            string BillFetchUrl = objModel.ProviderURL.Trim();

            string headerJson = objModel.HeaderJson.Trim();

            ProviderConfiguration config = new ProviderConfigurationDAL().SelectByProviderIDAndOperatorID(ProviderID, mainRequest.OperatorID);

            if (config == null) return response;

            string type = config.Type;

            List<BBPSConfiguration> list = null;
            try
            {
                list = new BBPSConfigurationDal().SelectByOperatorID(mainRequest.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            ParamLabelViewModel paramLabelViewModel = new ParamLabelViewModel();
            if (list != null && list.Count > 0)
            {
                if (list.Count >= 1 && list[0].Label != null)
                    paramLabelViewModel.ParamLabel1 = list[0].Label;

                if (list.Count >= 2 && list[0].Label != null)
                    paramLabelViewModel.ParamLabel2 = list[1].Label;

                if (list.Count >= 3 && list[0].Label != null)
                    paramLabelViewModel.ParamLabel3 = list[2].Label;

                if (list.Count >= 4 && list[0].Label != null)
                    paramLabelViewModel.ParamLabel4 = list[3].Label;

                if (list.Count >= 5 && list[0].Label != null)
                    paramLabelViewModel.ParamLabel5 = list[4].Label;
            }

            //////////
            string HttpResponse = string.Empty;
            string urlWithParam = string.Empty;

            if (ProviderID == Company.YesBankProviderID)
            {
                string macAddress = StringExtension.GetMACAddress();
                BBPSProcessing bbpsProcessing = new BBPSProcessing();

                response = bbpsProcessing.BillFetch(mainRequest, user, IPAddress, macAddress, paramLabelViewModel, config.OpCode);
                HttpResponse = response.ToString();

                if (response == null)
                {
                    failureMessage = response.Message;
                    return response;
                }

            }
            else
            {

                urlWithParam = new ProviderProcessing().URLFormattingBBPS(paramLabelViewModel, methodType: objModel.MethodType, baseURL: BillFetchUrl, OpCode: config.OpCode,
   ConsumerNumber: mainRequest.ConsumerNumber, Amount: 0, ServiceID: 0,
   RequestID: new Util().GetRandomNumber(), Type: type, RefID: "", DateStr: "",
   Param1: mainRequest.Param1, Param2: mainRequest.Param2, Param3: mainRequest.Param3, Param4: mainRequest.Param4, Param5: mainRequest.Param5,
   IFSC: "", LoginID: providerMaster.LoginID, Token: providerMaster.Token, InstantPayOutletID: user.InstantPayOutletID, RetailerMobileNumber: user.MobileNumber);

                if (urlWithParam.Length <= 0)
                    return response;

                urlWithParam = urlWithParam.Replace(",\"\": \"\"", "");

                //string HttpResponse = string.Empty;
                try
                {
                    if (objModel.MethodType.EqualsIgnoreCase(MethodType.Get))
                        HttpResponse = HttpProcessing.CallGetMethod(urlWithParam);

                    if (objModel.MethodType.EqualsIgnoreCase(MethodType.Post))
                        HttpResponse = HttpProcessing.CallPostMethod(urlWithParam, objModel.HeaderJson);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            int logID = 0;

            try
            {
                ProviderRequestResponseLog providerRequestResponseLog = new ProviderRequestResponseLog
                {
                    ProviderID = ProviderID,
                    ServiceID = ServiceID,
                    UserID = keyDataModel.UserID,
                    TransactionID = 0,
                    Request = urlWithParam,
                    Response = HttpResponse,
                    IPAddress = IPAddress,
                    Through = keyDataModel.Platform
                };

                logID = new ProviderRequestResponseLogDal().Insert(providerRequestResponseLog);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            TransactionResponseProcessing PRP = new TransactionResponseProcessing();
            string view = Enum.GetName(typeof(ResponseType), ResponseType.View);
            ProviderReturnResponse extratedResponse = new ProviderReturnResponse();

            PRP.ExtractResponseFromText(HttpResponse.Trim(), view, ProviderID, "", extratedResponse);

            failureMessage = ProviderMessage.ResponseNotFound;

            //response.Message = "No Data found";
            response.Message = response.Message;

            if (!extratedResponse.Status.EqualsIgnoreCase(StatusType.Success))
            {
                response.Message = string.IsNullOrEmpty(extratedResponse.Message) ? response.Message : extratedResponse.Message;
                return response;
            }

            response.Status = true;
            response.CustomerName = (extratedResponse.CustomerName.EqualsIgnoreCase(Messages.MatcherParamNotFound)) ? Messages.DataNotFound : extratedResponse.CustomerName;
            response.DueDate = (extratedResponse.DueDate.EqualsIgnoreCase(Messages.MatcherParamNotFound)) ? Messages.DataNotFound : extratedResponse.DueDate;
            response.Message = extratedResponse.Message;
            response.Amount = ParseBalance(extratedResponse.Amount, IssBBPS);
            response.Message = (extratedResponse.Message.EqualsIgnoreCase(Messages.MatcherParamNotFound)) ? Messages.DataNotFound : extratedResponse.Message;
            response.BBPSViewID = (extratedResponse.BBPSViewID.EqualsIgnoreCase(Messages.MatcherParamNotFound)) ? Messages.DataNotFound : extratedResponse.BBPSViewID;
            response.BillDate = extratedResponse.BillDate;

            response.ResponseParam1 = (extratedResponse.ResponseParam1.EqualsIgnoreCase(Messages.MatcherParamNotFound)) ? Messages.DataNotFound : extratedResponse.ResponseParam1;
            response.ResponseParam2 = (extratedResponse.ResponseParam2.EqualsIgnoreCase(Messages.MatcherParamNotFound)) ? Messages.DataNotFound : extratedResponse.ResponseParam2;
            response.ResponseParam3 = (extratedResponse.ResponseParam3.EqualsIgnoreCase(Messages.MatcherParamNotFound)) ? Messages.DataNotFound : extratedResponse.ResponseParam3;
            response.ResponseParam4 = (extratedResponse.ResponseParam4.EqualsIgnoreCase(Messages.MatcherParamNotFound)) ? Messages.DataNotFound : extratedResponse.ResponseParam4;
            response.ResponseParam5 = (extratedResponse.ResponseParam5.EqualsIgnoreCase(Messages.MatcherParamNotFound)) ? Messages.DataNotFound : extratedResponse.ResponseParam5;


            Task.Run(() => new PlanSettingDAL().UpdateUserBillFetchUtilization(user.UserID));

            failureMessage = StatusType.Ok;
            return response;
        }


        public static string ParseBalance(decimal bal, bool IsBBPS)
        {
            try
            {
                if (bal.ToString().Trim().Length <= 0)
                {
                    return Messages.DataNotFound;
                }

                if (!IsBBPS)
                {
                    decimal dAmut = Math.Ceiling(bal);
                    return dAmut.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }

            return bal.ToString();
        }


        private string IsCheckValidUser(int UserID)
        {
            UserWisePlanLimit userWisePlanLimit = new UserWisePlanLimitDal().SelectByUserID(UserID);
            if (userWisePlanLimit == null)
                return TransactionMessage.BillViewStatus;

            if (userWisePlanLimit.BillFetchHitCount == 0)
                return TransactionMessage.BillViewStatus;

            UserPlanUtilization userPlanUtilizationDal = new UserPlanUtilizationDal().SelectByUserID(UserID);

            if (userPlanUtilizationDal == null)
                return StatusType.Ok;

            if (userPlanUtilizationDal.BillFetchCount >= userWisePlanLimit.BillFetchHitCount)
                return TransactionMessage.PlanViewLimitOver;

            return StatusType.Ok;
        }
    }
}