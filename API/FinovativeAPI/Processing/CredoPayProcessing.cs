using Newtonsoft.Json;
using Project.BAL;
using Project.DAL;
using Project.Model;
using Project.StaticModel;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class CredoPayProcessing
    {
        private bool isValidTextForPasswordChanges(string response)
        {
            if (response.TrimIfNotNull().ToLower().Contains("Reset Password TID:".ToLower()))
            {
                return true;
            }

            if (response.TrimIfNotNull().ToLower().Contains("TID:".ToLower()))
            {
                return true;
            }

            return false;
        }

        public void updateCredoPayLoginIDAndPassword(string response)
        {
            //response : {"mobile":[reatilerMobileno],"message":"Reset Password TID: E00111111 Login ID: 2000011111111 password: aaaaaa"}
            if (response.TrimIfNotNull().Length <= 2)
            {
                return;
            }

            if (!isValidTextForPasswordChanges(response))
            {
                return;
            }

            Response.CredoPay.CredoPayLoginIDAndPassword originalResponse = JsonConvert.DeserializeObject<Response.CredoPay.CredoPayLoginIDAndPassword>(response);
            string mobile = originalResponse.Mobile;
            string message = originalResponse.Message;

            System.Text.RegularExpressions.Match match = Regex.Match(message, @"Login ID:(.+)password:(.+)", RegexOptions.Singleline);

            if (!match.Success)
            {
                return;
            }

            if (match.Groups.Count <= 1)
            {
                return;
            }

            string credoPayLoginID = match.Groups[1].Value.Trim();
            string credoPayPassword = match.Groups[2].Value.Trim();

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectUserByMobileNumberSynchronization(mobile);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }

            if (ObjModel == null)
                return;

            ObjModel.CredoPay_LoginID = credoPayLoginID;
            ObjModel.CredoPay_Password = credoPayPassword;
            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
        }
        public void InsertMerchantCPID(string response)
        {
            //response : {"mobile":"9872018818","email":"nitish.cwc@gmail.com","agg_id":"CPPF29010000395","merchant_cpId":"CPMS29010008551","cpId":"CPST29010008471","status":"activated","type":"terminal_status","user":"3000007629","password":"gZQfDb","topic":"terminal_webhooks"}
            if (response.TrimIfNotNull().Length <= 2)
            {
                return;
            }
            Response.CredoPay.TerminalStatusResponse originalResponse = JsonConvert.DeserializeObject<Response.CredoPay.TerminalStatusResponse>(response);
            if (originalResponse==null)
                return;
            if(originalResponse.mobile.Length<=9)
                return;
            if (originalResponse.cpId.Length <= 2)
                return;

            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectUserByMobileNumberSynchronization(originalResponse.mobile);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }

            if (ObjModel == null)
                return;

            try
            {
                 new ProviderUserMappingBal().Insert(ObjModel.UserID,Company.CredoPayProviderID,UserKey.MerchantCPID, originalResponse.cpId);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
        }

        public Project.Response.MicroATM.CredoPayStatusCheckResponse GetTransctionStatus(TransactionMaster transaction, string token)
        {
            string respStr = HttpProcessing.CredoPay_TransactionStatus(transaction.ProviderRequestID, token);

            string reqStr = transaction.ProviderRequestID;

            new Logger().Info("CredoPay", "GetTransctionStatus", reqStr + "::" + respStr);

            Log(transaction, reqStr, respStr);

            Response.MicroATM.CredoPayStatusCheckResponse originalResponse = JsonConvert.DeserializeObject<Response.MicroATM.CredoPayStatusCheckResponse>(respStr);
            return originalResponse;
        }
        public Project.Response.MicroATM.CredoPayStatusCheckResponse GetCredoPaySarataTransctionStatus(TransactionMaster transaction, string token)
        {
            string respStr = "";// HttpProcessing.CredoPaySarata_TransactionStatus(transaction.ProviderRequestID, token);

            string reqStr = transaction.ProviderRequestID;

            new Logger().Info("CredoPaySarata", "GetTransctionStatus", reqStr + "::" + respStr);

            Log(transaction, reqStr, respStr);

            Response.MicroATM.CredoPayStatusCheckResponse originalResponse = JsonConvert.DeserializeObject<Response.MicroATM.CredoPayStatusCheckResponse>(respStr);
            return originalResponse;
        }

        private void Log(TransactionMaster transactionMaster, string request, string response)
        {
            try
            {
                ProviderRequestResponseLog providerRequestResponseLog = new ProviderRequestResponseLog
                {
                    ProviderID = transactionMaster.ProviderID,
                    UserID = transactionMaster.UserID,
                    TransactionID = transactionMaster.TransactionMasterID,
                    Request = request,
                    Response = response,
                    IPAddress = transactionMaster.IPAddress,
                    Through = transactionMaster.Through,
                    ServiceID = transactionMaster.ServiceID,
                };

                Task.Run(() =>
                {
                    new ProviderRequestResponseLogDal().Insert(providerRequestResponseLog);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}