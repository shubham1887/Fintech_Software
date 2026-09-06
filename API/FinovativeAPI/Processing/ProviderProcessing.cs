using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing.XMPP;
using Project.Processing.YesBank;
using Project.Response.ProviderResponse;
using Project.Response.TransactionProviderHistory;
using Project.Response.Utility;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class ProviderProcessing
    {
        internal ProviderReturnResponse Hit(TransactionMaster transactionMaster, string Label, int ProviderId = 0)
        {
            ProviderReturnResponse providerResponse = new ProviderReturnResponse
            {
                Amount = transactionMaster.Amount,
                ConsumerNumber = transactionMaster.ConsumerNumber,
                Balance = "0",
                ProviderCommission = "0",
                Response = "",
                ProviderID = 0,
                RequestID = "0",
                TransactionID = "0",
                IsHitToNewProvider = false
            };

            bool IsResentRecharge = false;
            int SequenseCount = 0;
            string failureMessage = "";
            HashSet<string> providerIDWithHLR = new HashSet<string>();
            SequenceSelection sequenceSelection = new SequenceSelection();
            if (ProviderId == 0)
            {
                providerIDWithHLR = sequenceSelection.select(transactionMaster.OperatorID, transactionMaster.Amount,
                    transactionMaster.UserID, transactionMaster.ConsumerNumber, out failureMessage);

                IsResentRecharge = false;
            }
            else  // For Auto resend Recharge
            {
                IsResentRecharge = true;
                providerIDWithHLR.Add(ProviderId.ToString());
            }

            SequenseCount = providerIDWithHLR.Count;
            TransactionResponseProcessing responseProcessing = new TransactionResponseProcessing();
            if (SequenseCount <= 0)
            {
                providerResponse.Status = StatusType.Failure;
                providerResponse.Response = failureMessage;
                providerResponse.Description = providerResponse.Response;

                return providerResponse;
            }

            foreach (string providerID in providerIDWithHLR)
            {
                int ProviderID = Convert.ToInt32(providerID);

                providerResponse.Status = StatusType.Process;
                providerResponse.RequestID = transactionMaster.ProviderRequestID;
                providerResponse.ProviderID = ProviderID;
                ProviderConfiguration config = new ProviderConfigurationBAL().ProviderValidation(ProviderID, transactionMaster.OperatorID);

                if (config == null)
                {
                    providerResponse.Status = StatusType.Failure;
                    providerResponse.Response = Message.TransactionMessage.OperatorOff;
                    providerResponse.Description = providerResponse.Response;
                    continue;
                }

                if (IsBSNLRechargeSwitch(Convert.ToInt32(transactionMaster.Amount), transactionMaster.OperatorID))
                {
                    config = new ProviderConfigurationBAL().ProviderValidation(ProviderID, Company.BsnlSpecialOperatorID);
                }
                string Opcode = config.OpCode;
                string type = config.Type;
                string Lapu = GetRandamLapu(config.Lapu);
                if (Opcode.Length <= 0)
                {
                    providerResponse.Status = StatusType.Failure;
                    providerResponse.Response = TransactionMessage.Opcodenotfound;
                    providerResponse.Description = providerResponse.Response;
                    continue;
                }
                UserMaster user = new UserDAL().SelectByIDFromCache(transactionMaster.UserID);
                string StateCode = new ProviderStateConfigurationBal().SelectStateCodeByStateID(transactionMaster.StateID, ProviderID);
                if (transactionMaster.ServiceID == (int)ServiceCode.Mobile)
                {
                    string CircleOpcode = sequenceSelection.GetCircleWiseOpCode(transactionMaster.OperatorID, ProviderID, transactionMaster.StateID);
                    if (CircleOpcode != null && CircleOpcode.Length >= 1)
                        Opcode = CircleOpcode;
                }

                if (!IsResentRecharge)
                {
                    TransactionHistoryResponse history = new TransactionProviderHistoryBal().Select(transactionMaster.TransactionMasterID, ProviderID);

                    if (history != null)
                    {
                        providerResponse.Status = StatusType.Failure;
                        providerResponse.Response = TransactionMessage.RechargeIsProcessedAlready;
                        providerResponse.Description = providerResponse.Response;
                        continue;
                    }

                    Task.Run(() => new TransactionProviderHistoryBal().Insert(transactionMaster, ProviderID, transactionMaster.OperatorID));
                }

                Response.Provider.ViewModel providerMaster = new ProviderMasterDAL().SelectByProviderID(ProviderID);

                if (providerMaster == null)
                {
                    providerResponse.Status = StatusType.Failure;
                    providerResponse.Response = TransactionMessage.TransactionURLNotFound;
                    providerResponse.Description = providerResponse.Response;
                    continue;
                }

                ProviderDetails providerDetails = new ProviderDetailsBAL().SelectURL(ProviderID, transactionMaster.ServiceID, UrlTypes.Pay);

                if (providerDetails == null)
                {
                    providerResponse.Status = StatusType.Failure;
                    providerResponse.Response = TransactionMessage.TransactionURLNotFound;
                    providerResponse.Description = providerResponse.Response;
                    continue;
                }
                // Penidng API For no shift
                if (IsResentRecharge == true && providerDetails.TransactionIDPrefix != null &&
                    providerDetails.TransactionIDPrefix.Length > 1 && Company.IsPendingProvider.EqualsIgnoreCase("Yes"))
                {
                    providerResponse.Response = TransactionMessage.NotpermittedAPI;
                    providerResponse.Description = providerResponse.Response;
                    return providerResponse;
                }
                // Change PendingProviderStatus
                if (IsResentRecharge)
                {
                    if (transactionMaster.PendingProviderStatus == StatusType.Process)
                        new TransactionBal().UpdatePendingProviderStatus(StatusType.Shift, transactionMaster.TransactionMasterID);
                }

                if (providerDetails.TransactionIDPrefix != null && providerDetails.TransactionIDPrefix.Length > 1 &&
                    Company.IsPendingProvider.EqualsIgnoreCase("Yes"))
                {
                    providerResponse.Status = StatusType.Success;
                    providerResponse.TransactionID = string.Concat(providerDetails.TransactionIDPrefix, new Util().GetRandomNumberWithDatePrefix());
                    providerResponse.Description = TransactionMessage.PenidngProviderResponse;
                    new TransactionBal().UpdateProviderID(ProviderID, transactionMaster.TransactionMasterID);
                    new TransactionBal().UpdatePendingProviderStatus(StatusType.Process, transactionMaster.TransactionMasterID);
                    return providerResponse;
                }
                string URLWithParameter = string.Empty;
                string url = providerDetails.ProviderURL.ToString().Trim();
                string headerJson = providerDetails.HeaderJson.ToString().Trim();

                if (url.Length <= 0)
                {
                    providerResponse.Status = StatusType.Failure;
                    providerResponse.Response = TransactionMessage.TransactionURLNotFound;
                    providerResponse.Description = providerResponse.Response;
                    continue;
                }
                //////*****************
                string HttpResponse = string.Empty;
                string urlWithParam = string.Empty;

                if (ProviderID == Company.YesBankProviderID)
                {

                    List<BBPSConfiguration> list = null;
                    try
                    {
                        list = new BBPSConfigurationDal().SelectByOperatorID(transactionMaster.OperatorID);
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

                    string macAddress = StringExtension.GetMACAddress();

                    BBPSProcessing bbpsProcessing = new BBPSProcessing();


                    providerResponse = bbpsProcessing.BillPay(transactionMaster, Label, macAddress, paramLabelViewModel, config.OpCode, user);
                    providerResponse.ResponseParam5 = config.OpCode;

                    HttpResponse = providerResponse.ToString();

                    if (providerResponse == null)
                    {
                        failureMessage = providerResponse.Message;
                        return providerResponse;
                    }
                }
                else
                {
                    urlWithParam = URLFormatting(methodType: providerDetails.MethodType, baseURL: url, OpCode: Opcode, ConsumerNumber: transactionMaster.ConsumerNumber,
                                                        Amount: transactionMaster.Amount, ServiceID: transactionMaster.ServiceID,
                                                        RequestID: transactionMaster.ProviderRequestID, Type: type, RefID: "", DateStr: "",
                                                        Param1: transactionMaster.Bill_Optional1, Param2: transactionMaster.Bill_Optional2, Param3: transactionMaster.Bill_Optional3,
                                                        Param4: transactionMaster.Bill_Optional4, Param5: transactionMaster.Bill_Optional5, IFSC: transactionMaster.DMT_IFSC,
                                                        LoginID: providerMaster.LoginID, Token: providerMaster.Token, BBPSViewID: transactionMaster.Bill_Optional6, StateCode: StateCode, Lapu: Lapu, PinCode: user.Pincode, InstantPayOutletID: user.InstantPayOutletID, RetailerMobileNumber: user.MobileNumber);

                    if (urlWithParam.Length <= 0)
                    {
                        providerResponse.Status = StatusType.Failure;
                        providerResponse.Response = TransactionMessage.TransactionURLNotFound;
                        providerResponse.Description = providerResponse.Response;
                        continue;
                    }

                    // check current status, if not process return it
                    TransactionMaster objModelTran = new TransactionDAL().SelectByID(transactionMaster.TransactionMasterID);
                    if (!IsResentRecharge)
                    {
                        if (!new ResponseProcess().checkProcess(objModelTran.Status))// not in process
                        {
                            providerResponse.Status = StatusType.Process;
                            providerResponse.Response = Messages.NotInProcess;
                            providerResponse.Description = Messages.NotInProcess;
                            return providerResponse;
                        }
                    }

                    providerResponse.IsHitToNewProvider = true;
                    new TransactionBal().UpdateProviderID(ProviderID, transactionMaster.TransactionMasterID);

                    //string HttpResponse = string.Empty;
                    try
                    {
                        if (ProviderID == Company.XMPPProviderID)
                        {
                            HttpResponse = new XMPPChat().RechargeCall(urlWithParam);
                        }
                        else if (providerDetails.MethodType.EqualsIgnoreCase("GET"))
                        {
                            HttpResponse = HttpProcessing.CallGetMethod(urlWithParam);
                        }
                        else
                        {
                            HttpResponse = HttpProcessing.CallPostMethod(urlWithParam, headerJson);
                        }
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                        providerResponse.Status = StatusType.Process;
                        providerResponse.Response = Messages.ExceptionOccured;
                        providerResponse.Description = providerResponse.Response;
                        return providerResponse;
                    }
                }

                int logID = 0;
                try
                {
                    ProviderRequestResponseLog providerRequestResponseLog = new ProviderRequestResponseLog
                    {
                        ProviderID = ProviderID,
                        UserID = transactionMaster.UserID,
                        TransactionID = transactionMaster.TransactionMasterID,
                        Request = urlWithParam,
                        Response = HttpResponse,
                        IPAddress = transactionMaster.IPAddress,
                        Through = transactionMaster.Through,
                        ServiceID = transactionMaster.ServiceID
                    };

                    logID = new ProviderRequestResponseLogDal().Insert(providerRequestResponseLog);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (HttpResponse.Length <= 0)
                {
                    providerResponse.Status = StatusType.Process;
                    providerResponse.Response = TransactionMessage.BlankResponse;
                    providerResponse.Description = providerResponse.Response;
                    return providerResponse;
                }

                string respType = Enum.GetName(typeof(ResponseType), ResponseType.Initial);
                TransactionResponseProcessing PRP = new TransactionResponseProcessing();
                if (ProviderID != Company.YesBankProviderID)
                {
                    PRP.ExtractResponseFromText(HttpResponse, respType, ProviderID, transactionMaster.ProviderRequestID, providerResponse);
                }
                Task.Run(() => new ProviderMasterBAL().UpdateProviderBalance(ProviderID, providerResponse.Balance, providerResponse.DMTBalance));

                if (IsFail(providerResponse.Status))
                {
                    continue;
                }
                break;
            }

            return providerResponse;
        }

        private string GetRandamLapu(string LapuRange)
        {
            if (LapuRange == null)
                return "";
            if (LapuRange.Length <= 0)
                return "";

            string[] CommaSeperatedArr = LapuRange.Split(',');
            if (CommaSeperatedArr.Length <= 1)
                return LapuRange;

            Random random = new Random();
            int start2 = random.Next(0, CommaSeperatedArr.Length);
            return CommaSeperatedArr[start2];
        }

        private string getAmountAsIntergerIfInterger(decimal Amount)
        {
            if ((Amount % 1) == 0)
            {
                return Convert.ToInt32(Amount).ToString();
            }
            else
            {
                return Amount.ToString();
            }
        }

        public string URLFormatting(string methodType, string baseURL, string OpCode, string ConsumerNumber, decimal Amount, int ServiceID, string RequestID,
            string Type, string RefID, string DateStr, string Param1 = "", string Param2 = "", string Param3 = "", string Param4 = "", string Param5 = "", string IFSC = "",
            string LoginID = "", string Token = "", string BBPSViewID = "", string Remark = "", string StateCode = "", string Lapu = "", string PinCode = "", string InstantPayOutletID = "", string RetailerMobileNumber = "")
        {
            string str;
            try
            {
                str = baseURL;
                //str = str.Replace("[Amount]", Convert.ToInt32(Amount).ToString());
                str = str.Replace("[Amount]", getAmountAsIntergerIfInterger(Amount));

                str = getURLEncode(methodType, str, "[Number]", ConsumerNumber);
                str = getURLEncode(methodType, str, "[RequestID]", RequestID);
                str = getURLEncode(methodType, str, "[Service]", ServiceID.ToString());
                str = getURLEncode(methodType, str, "[Operator]", OpCode);
                str = getURLEncode(methodType, str, "[Type]", Type);

                str = getURLEncode(methodType, str, "[RefID]", RefID);
                str = getURLEncode(methodType, str, "[Date]", DateStr);

                str = getURLEncode(methodType, str, "[Param1]", Param1);
                str = getURLEncode(methodType, str, "[Param2]", Param2);
                str = getURLEncode(methodType, str, "[Param3]", Param3);
                str = getURLEncode(methodType, str, "[Param4]", Param4);
                str = getURLEncode(methodType, str, "[Param5]", Param5);
                str = getURLEncode(methodType, str, "[IFSC]", IFSC);

                str = getURLEncode(methodType, str, "[LoginID]", LoginID);
                str = getURLEncode(methodType, str, "[Token]", Token);
                str = getURLEncode(methodType, str, "[BBPSViewID]", BBPSViewID);
                str = getURLEncode(methodType, str, "[Remark]", Remark);
                str = getURLEncode(methodType, str, "[StateCode]", StateCode);
                str = getURLEncode(methodType, str, "[Lapu]", Lapu);
                str = getURLEncode(methodType, str, "[PinCode]", PinCode);
                str = getURLEncode(methodType, str, "[InstantPayOutletID]", InstantPayOutletID);
                str = getURLEncode(methodType, str, "[RetailerMobileNumber]", RetailerMobileNumber);
            }
            catch (Exception ex)
            {
                str = "";
                new Logger().Write(ex);
            }

            return str;
        }

        public string URLFormattingBBPS(Response.Utility.ParamLabelViewModel paramLabelViewModel, string methodType, string baseURL, string OpCode, string ConsumerNumber, decimal Amount, int ServiceID, string RequestID,
                string Type, string RefID, string DateStr, string Param1 = "", string Param2 = "", string Param3 = "", string Param4 = "", string Param5 = "", string IFSC = "",
                string LoginID = "", string Token = "", string BBPSViewID = "", string Remark = "", string StateCode = "", string Lapu = "", string InstantPayOutletID = "", string RetailerMobileNumber = "")
        {
            string str;
            try
            {
                str = baseURL;
                //str = str.Replace("[Amount]", Convert.ToInt32(Amount).ToString());
                str = str.Replace("[Amount]", getAmountAsIntergerIfInterger(Amount));

                str = getURLEncode(methodType, str, "[Number]", ConsumerNumber);
                str = getURLEncode(methodType, str, "[RequestID]", RequestID);
                str = getURLEncode(methodType, str, "[Service]", ServiceID.ToString());
                str = getURLEncode(methodType, str, "[Operator]", OpCode);
                str = getURLEncode(methodType, str, "[Type]", Type);

                str = getURLEncode(methodType, str, "[RefID]", RefID);
                str = getURLEncode(methodType, str, "[Date]", DateStr);

                str = getURLEncode(methodType, str, "[Param1]", Param1);
                str = getURLEncode(methodType, str, "[Param2]", Param2);
                str = getURLEncode(methodType, str, "[Param3]", Param3);
                str = getURLEncode(methodType, str, "[Param4]", Param4);
                str = getURLEncode(methodType, str, "[Param5]", Param5);
                str = getURLEncode(methodType, str, "[IFSC]", IFSC);

                str = getURLEncode(methodType, str, "[LoginID]", LoginID);
                str = getURLEncode(methodType, str, "[Token]", Token);
                str = getURLEncode(methodType, str, "[BBPSViewID]", BBPSViewID);
                str = getURLEncode(methodType, str, "[Remark]", Remark);
                str = getURLEncode(methodType, str, "[StateCode]", StateCode);
                str = getURLEncode(methodType, str, "[Lapu]", Lapu);
                str = getURLEncode(methodType, str, "[InstantPayOutletID]", InstantPayOutletID);
                str = getURLEncode(methodType, str, "[RetailerMobileNumber]", RetailerMobileNumber);

                str = getURLEncode(methodType, str, "[ParamLabel1]", paramLabelViewModel.ParamLabel1);
                str = getURLEncode(methodType, str, "[ParamLabel2]", paramLabelViewModel.ParamLabel2);
                str = getURLEncode(methodType, str, "[ParamLabel3]", paramLabelViewModel.ParamLabel3);
                str = getURLEncode(methodType, str, "[ParamLabel4]", paramLabelViewModel.ParamLabel4);
                str = getURLEncode(methodType, str, "[ParamLabel5]", paramLabelViewModel.ParamLabel5);
            }
            catch (Exception ex)
            {
                str = "";
                new Logger().Write(ex);
            }

            return str;
        }

        private string getURLEncode(string methodType, string mainStr, string replacewith, string replaceWord)
        {
            if (methodType.EqualsIgnoreCase(MethodType.Get))
            {
                mainStr = mainStr.Replace(replacewith, System.Web.HttpUtility.UrlEncode(replaceWord));
            }
            else
            {
                mainStr = mainStr.Replace(replacewith, replaceWord);
            }
            return mainStr;
        }

        private bool IsFail(string Status)
        {
            ResponseProcess resProcess = new ResponseProcess();

            return resProcess.CheckFailure(Status);
        }

        private bool isBSNLOperator(int OperatorID)
        {
            if (Company.BsnlOperatorID == OperatorID)
            {
                return true;
            }

            if (Company.BsnlSpecialOperatorID == OperatorID)
            {
                return true;
            }
            return false;
        }

        private bool IsBSNLRechargeSwitch(int Amount, int OperatorID)
        {
            if (Company.BsnlOperatorID <= 0)
            {
                return false;
            }

            if (Company.BsnlSpecialOperatorID <= 0)
            {
                return false;
            }

            string BSNLRechargeSwitch = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.BSNLRechargeSwitch), "No");

            if (!BSNLRechargeSwitch.EqualsIgnoreCase("Yes"))
            {
                return false;
            }

            if (!isBSNLOperator(OperatorID))
            {
                return false;
            }

            if (Amount % 10 != 0)
                return true;

            return false;
        }
    }
}