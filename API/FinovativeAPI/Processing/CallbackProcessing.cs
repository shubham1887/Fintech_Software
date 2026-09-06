using Project.BAL;
using Project.DAL;
using Project.Model;
using Project.Requests.Transaction;
using Project.Response.ProviderResponse;
using Project.StaticModel;
using System;
using System.Threading.Tasks;
using System.Web;

namespace Project.Processing
{
    public class CallbackProcessing
    {
        internal void PreExtractCallBack(int ProviderID, string response)
        {
            Task.Run(() => ExtractCallBack(ProviderID, response));
        }

        public void ExtractCallBack(int ProviderID, string response)
        {
            try
            {
                response = response.Replace("\n", " ");
                response = HttpUtility.UrlDecode(response);

                response = response.Replace("\n", " ");

                int logID = new ProviderCallBackLogBal().Insert(ProviderID, response);

                ProviderReturnResponse extratedResponse = new ProviderReturnResponse();
                TransactionResponseProcessing PRP = new TransactionResponseProcessing();
                string callback = Enum.GetName(typeof(ResponseType), ResponseType.Callback);

                PRP.ExtractResponseFromText(response.Trim(), callback, ProviderID, "", extratedResponse);

                if (extratedResponse == null)
                {
                    return;
                }
                if (extratedResponse.RequestID == null)
                {
                    return;
                }
                if (extratedResponse.Status == null)
                {
                    return;
                }

                extratedResponse.ProviderID = ProviderID;
                string status = extratedResponse.Status;
                string ProviderRequestID = extratedResponse.RequestID;

                if (ProviderRequestID == null)
                {
                    return;
                }

                if (ProviderRequestID.Length <= 0)
                {
                    return;
                }

                if (ProviderRequestID.Trim().EqualsIgnoreCase(Messages.MatcherParamNotFound))
                {
                    return;
                }

                string customerNumber = extratedResponse.ConsumerNumber;

                Task.Run(() => new ProviderMasterBAL().UpdateProviderBalance(ProviderID, extratedResponse.Balance, extratedResponse.DMTBalance));

                int DOCInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
                using (ConcurrencyControl.AcquireLock(DOCInt, ProviderRequestID)) // need here, because some api hit callback multiple times at same time
                {
                    ProcessForAllService(logID, response, extratedResponse);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        private void ProcessForAllService(int logID, string response, ProviderReturnResponse extratedResponse)
        {
            try
            {
                int ProviderID = extratedResponse.ProviderID;
                string status = extratedResponse.Status;
                string ProviderRequestID = extratedResponse.RequestID;
                Response.TransactionMaster.ViewModel Model = new TransactionBal().SelectByRequestID(ProviderRequestID);

                if (Model == null)
                    return;

                new ProviderCallBackLogBal().UpdateTansactionID(logID, Model.TransactionMasterID);

                if (!response.ToLower().Contains(Model.ConsumerNumber.ToLower()))
                {
                    // added due some api not providing customer number in response
                    string afterExtractionMessage = $"[Callback] {Environment.NewLine} Number :[Number]";
                    afterExtractionMessage = afterExtractionMessage.Replace("[Number]", Model.ConsumerNumber);
                    afterExtractionMessage = afterExtractionMessage.Replace("[Callback]", response);
                    new ProviderCallBackLogBal().Update(logID, afterExtractionMessage);
                }

                if (Model.ProviderID != ProviderID)
                    return;

                 

                if (!new ResponseProcess().checkProcess(Model.Status))
                {
                    bool IsValidUpdateRecharge = CheckValidationForUpdateRecharge(Model.TransactionMasterID, Model.Status, status);
                    if (!IsValidUpdateRecharge)
                        return;
                }

                

                if (Model.ServiceID == (int)ServiceCode.MoneyTransfer)
                {
                    DMTTransctionStatusRequest mainRequest = new DMTTransctionStatusRequest
                    {
                        ID = Model.TransactionMasterID.ToString()
                    };
                    new DMTProcessing().DMTTransactionStatusCheck(mainRequest, Model.UserID);
                    return;
                }
                if (Model.ServiceID == (int)ServiceCode.UPITransfer)
                {
                    UPITransctionStatusRequest mainRequest = new UPITransctionStatusRequest
                    {
                        ID = Model.TransactionMasterID.ToString()
                    };
                    new UPITransferProcessing().UPITransactionStatusCheck(mainRequest, Model.UserID);
                    return;
                }

                if (Model.ServiceID == (int)ServiceCode.VendorPay)
                {
                    SmartPayTransctionStatusRequest mainRequest = new SmartPayTransctionStatusRequest
                    {
                        ID = Model.TransactionMasterID.ToString()
                    };
                    new VendorPayProcessing.VendorPayProcessing().VendorPayTransactionStatusCheck(mainRequest, Model.UserID);
                    return;
                }


                if (Model.ServiceID == (int)ServiceCode.PaymentGateway && Model.ProviderID == Company.InstantPayUPIGatewayID)
                {
                    new PaymentGatewayProcessing().UpdateTransaction(extratedResponse.RequestID, extratedResponse.Status, extratedResponse.TransactionID,
                        extratedResponse.Amount.ToString(), "UPI", GatewayCode.InstantPay, response);
                    return;
                }
              
                if (Model.ServiceID == (int)ServiceCode.Payout)
                {
                    PayoutTransctionStatusRequest mainRequest = new PayoutTransctionStatusRequest
                    {
                        ID = Model.TransactionMasterID.ToString()
                    };
                    new PayoutProcessingV1.PayoutProcessingV1().PayOutTransactionStatusCheck(mainRequest, Model.UserID);
                    return;
                }

                if (new ResponseProcess().checkProcess(Model.Status))
                {
                    if (status.EqualsIgnoreCase(StatusType.Failure) &&
                        ValidServiceForShift(Model.ServiceID))
                    {
                        ShiftRecharge(Model.TransactionMasterID, extratedResponse);
                        return;
                    }
                }

                Response.TransactionMaster.TransactionResponse txnResponse = new Response.TransactionMaster.TransactionResponse();

                new TransactionBal().UpdateRechargeStatus(extratedResponse, Model.TransactionMasterID, txnResponse);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        private bool IsNovity(Response.TransactionMaster.ViewModel model)
        {
            if (model.ServiceID != (int)ServiceCode.MoneyTransfer) return false;

            if (model.ProviderID == Company.RoyalPayProviderID) return true;
            if (model.ProviderID == Company.PayOneMoneyProviderID) return true;
            if (model.ProviderID == Company.RMGPayProviderID) return true;

            return false;
        }

        private bool IsNovity5L(Response.TransactionMaster.ViewModel model)
        {
            if (model.ServiceID != (int)ServiceCode.MoneyTransfer) return false;

            if (model.ProviderID == Company.RMGPay5LProviderID) return true;

            return false;
        }

        private bool ValidServiceForShift(int ServiceID)
        {
            if (ServiceID == (int)ServiceCode.Mobile)
                return true;
            if (ServiceID == (int)ServiceCode.Dth)
                return true;
            if (ServiceID == (int)ServiceCode.Fasttag)
                return true;
            if (ServiceID == (int)ServiceCode.Game)
                return true;
            if (ServiceID == (int)ServiceCode.Electricity)
                return true;
            if (ServiceID == (int)ServiceCode.LifeInsurance)
                return true;
            if (ServiceID == (int)ServiceCode.LoanRePayment)
                return true;
            if (ServiceID == (int)ServiceCode.PipedGas)
                return true;

            return false;
        }

        private bool CheckValidationForUpdateRecharge(int TransactionMasterID, string dbStatus, string CallbackStatus)
        {
            bool IsValidComplaint = new TransactionBal().IsValidComplaintCallback(TransactionMasterID, dbStatus);
            if (IsValidComplaint)
                return true;
            if (Company.CallbackStatusChangeAllowed.EqualsIgnoreCase("Yes"))
                return true;
            if (IsCallbackStatusSuccesstoFailOnly(dbStatus, CallbackStatus))
                return true;

            return false;
        }
        private bool IsCallbackStatusSuccesstoFailOnly(string dbStatus, string CallbackStatus)
        {
            if (!new ResponseProcess().CheckSuccess(dbStatus))
                return false;
            if (!new ResponseProcess().CheckFailure(CallbackStatus))
                return false;
            if (!Company.CallbackStatusSuccessToFailOnly.EqualsIgnoreCase("Yes"))
                return false;
            return true;
        }

        private void ShiftRecharge(int TransactionMasterID, ProviderReturnResponse extratedResponse)
        {
            try
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(TransactionMasterID);
                if (transactionMaster == null)
                    return;

                if (!new ResponseProcess().checkProcess(transactionMaster.Status))
                {
                    return;
                }

                DateTime RchDate = transactionMaster.DOC;
                DateTime currDate = DateTime.Now;

                double diffInMin = (currDate - RchDate).TotalMinutes;

                Response.TransactionMaster.TransactionResponse transresponse = new Response.TransactionMaster.TransactionResponse();

                if (diffInMin >= 3) // if rch is 3 min old, then dont send to new api
                {
                    new TransactionBal().UpdateRechargeStatus(extratedResponse, transactionMaster.TransactionMasterID, transresponse);
                    return;
                }

                ProviderReturnResponse providerReturnResponse = new ProviderProcessing().Hit(transactionMaster,"");

                if (providerReturnResponse.IsHitToNewProvider == false)
                {
                    new TransactionBal().UpdateRechargeStatus(extratedResponse, transactionMaster.TransactionMasterID, transresponse);
                    return;
                }

                new TransactionBal().UpdateRechargeStatus(providerReturnResponse, transactionMaster.TransactionMasterID, transresponse);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}