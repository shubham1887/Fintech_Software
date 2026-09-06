using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing.XMPP;
using Project.Requests.Transaction;
using Project.Response.ProviderResponse;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Processing
{
    internal class StatusCheckProcessing
    {
        public string StatusCheckHit(TransactionMaster objModel, bool needStatusInReturn, out string status)
        {
            status = StatusType.Failure;
            ProviderReturnResponse providerResponse = new ProviderReturnResponse();
            providerResponse.Amount = Convert.ToDecimal(objModel.Amount);
            providerResponse.ConsumerNumber = objModel.ConsumerNumber;
            providerResponse.Balance = "0";
            providerResponse.Response = "";
            providerResponse.ProviderID = objModel.ProviderID;
            providerResponse.RequestID = objModel.ProviderRequestID;
            providerResponse.TransactionID = "0";

            bool isDevelopment = new Util().isDevelopment();
            if (isDevelopment == true)
            {
                return "At development, status check not allowed";
            }
            if (objModel.ProviderID <= 0)
                return TransactionMessage.TransactionNotYetSend;
            //need to check sec. diff 60
            string faileureMessage = new TransactionBal().isValidForBulkStatusCheck(objModel);
            if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                return faileureMessage;

            if (!new ResponseProcess().checkProcess(objModel.Status))// not in process
                return TransactionMessage.NotInProcess;

            Response.Provider.ViewModel providerMaster = new ProviderMasterDAL().SelectByProviderID(objModel.ProviderID);
            if (providerMaster == null)
            {
                return ProviderMessage.InValidProvider;
            }

            ProviderDetails providerDetails = new ProviderDetailsBAL().SelectURL(objModel.ProviderID, objModel.ServiceID, UrlTypes.StatusCheck);
            if (providerDetails == null)
            {
                return ProviderMessage.StatusCheckUrlNotFound;
            }

            string url = providerDetails.ProviderURL.ToString().Trim();

            if (url.Length <= 0)
            {
                return ProviderMessage.StatusCheckUrlNotFound;
            }

            string dateStr = "";
            if (providerDetails.DateFormat == null || providerDetails.DateFormat.Length <= 1)
            {
                dateStr = "";
            }
            else
            {
                dateStr = objModel.DOC.ToString(providerDetails.DateFormat);
            }

            string urlWithParam = new ProviderProcessing().URLFormatting(methodType: providerDetails.MethodType, baseURL: url, OpCode: "", ConsumerNumber: objModel.ConsumerNumber,
                                                    Amount: objModel.Amount, ServiceID: 0, RequestID: objModel.ProviderRequestID, Type: "",
                                                    RefID: objModel.ProviderRefNo, DateStr: dateStr, LoginID: providerMaster.LoginID, Token: providerMaster.Token);

            if (urlWithParam.Length <= 0)
            {
                return ProviderMessage.StatusCheckUrlNotFound;
            }

            string result = "";
            if (needStatusInReturn == true)// if user press status check from pending page
            {
                result = statusCheckCore(objModel, urlWithParam, providerDetails, providerResponse, needStatusInReturn);
            }
            else
            {
                result = "Status check done";
                Task.Run(() => statusCheckCore(objModel, urlWithParam, providerDetails, providerResponse, needStatusInReturn));
            }
            status = providerResponse.Status;

            return result;
        }

        private string statusCheckCore(TransactionMaster objModel, string urlWithParam, ProviderDetails providerDetails, ProviderReturnResponse providerResponse, bool needStatusInReturn)
        {
            string headerJson = providerDetails.HeaderJson.ToString().Trim();
            string HttpResponse = "";
            try
            {
                if (objModel.ProviderID == Company.XMPPProviderID)
                {
                    HttpResponse = new XMPPChat().RechargeCall(urlWithParam);
                }
                else if (providerDetails.MethodType.EqualsIgnoreCase("GET"))
                {
                    HttpResponse = HttpProcessing.CallGetMethod(urlWithParam, 30);
                }
                else
                {
                    HttpResponse = HttpProcessing.CallPostMethod(urlWithParam, headerJson);
                }

                int logID = 0;

                string uurlParraaam = urlWithParam;

                if (!uurlParraaam.ToLower().Contains(objModel.ConsumerNumber.ToLower()))
                {
                    uurlParraaam = urlWithParam + ", for number:" + objModel.ConsumerNumber;
                }

                ProviderRequestResponseLog providerRequestResponseLog = new ProviderRequestResponseLog
                {
                    ProviderID = objModel.ProviderID,
                    UserID = objModel.UserID,
                    TransactionID = objModel.TransactionMasterID,
                    ServiceID = objModel.ServiceID,
                    Request = uurlParraaam,
                    Response = HttpResponse,
                    IPAddress = "Status check: " + objModel.ConsumerNumber,
                    Through = PlatformCode.Web
                };

                if (HttpResponse == null || HttpResponse.Trim().Length <= 0)
                {
                    providerResponse.Status = StatusType.Process;
                    return TransactionMessage.BlankResponseFromStatusCheckAPI;
                }

                Task.Run(() => logID = new ProviderRequestResponseLogDal().Insert(providerRequestResponseLog));

                string respType = Enum.GetName(typeof(ResponseType), ResponseType.StatusCheck);
                TransactionResponseProcessing PRP = new TransactionResponseProcessing();

                PRP.ExtractResponseFromText(HttpResponse, respType, objModel.ProviderID, objModel.ProviderRequestID, providerResponse);

                if (new ResponseProcess().CheckSuccess(providerResponse.Status))
                {
                    TransactionResponse response = new TransactionResponse();
                    new TransactionBal().UpdateRechargeStatus(providerResponse, objModel.TransactionMasterID, response);
                    return StatusType.Ok;
                }

                if (providerResponse.Status.EqualsIgnoreCase(StatusType.Resend))
                {
                    providerResponse.Status = StatusType.Process;
                }

                if (needStatusInReturn == false)// if user press status check from pending page
                {
                    TransactionResponse response = new TransactionResponse();
                    new TransactionBal().UpdateRechargeStatus(providerResponse, objModel.TransactionMasterID, response);
                    return StatusType.Ok;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string RechargeTransactionStatusCheck(TransctionStatusRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (UserID != Company.AdminUserID)
            {
                return TransactionMessage.InvalidUser;
            }
            try
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(mainRequest.TransactionMasterID);

                if (transactionMaster == null) return TransactionMessage.InvalidID;
                string status = "";

                if (transactionMaster.ProviderID <= 0)
                    return TransactionMessage.TransactionNotYetSend;

                StatusCheckHit(transactionMaster, needStatusInReturn: false, out status);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return "Status check completed";
        }

        public void StatusCheck()
        {
            try
            {
                bool isDevelopment = new Util().isDevelopment();
                if (isDevelopment == true)
                {
                    return;// "At development, status check not allowed";
                }

                string failureMessage = new Validation().ServerDownTime();

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return; // "At ServerDownTime, status check not allowed";
                }

                List<PendingResponseForStatusCheck> list = null;
                try
                {
                    list = new TransactionDAL().SelectPendingTransactionForStatusCheck();
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (list == null)
                    return;

                if (list.Count <= 0)
                    return;

                List<PendingResponseForStatusCheck> randomLast20 = list.OrderBy(x => Guid.NewGuid()).Take(50).ToList();

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(randomLast20.AsEnumerable(), parallelOptions, item =>
                    {
                        //foreach (PendingResponse item in list)
                        try
                        {
                            TransactionMaster transactionMaster = new TransactionDAL().SelectByID(item.TransactionMasterID);

                            if (transactionMaster == null)
                            {
                                return;
                            }

                            string faileureMessage = new TransactionBal().isValidForBulkStatusCheck(transactionMaster);

                            if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                                return;

                            TransctionStatusRequest mainRequest = new TransctionStatusRequest();
                            mainRequest.TransactionMasterID = transactionMaster.TransactionMasterID;
                            string result = RechargeTransactionStatusCheck(mainRequest, Company.AdminUserID);
                        }
                        catch (Exception ex)
                        {
                            new Logger().Write(ex);
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}