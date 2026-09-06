using Newtonsoft.Json;
using Project.DAL;
using Project.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Processing.PaySwiffMicroATM
{
    public class PaySwiffMicroATMProcessing
    {     
        public Project.Response.MicroATM.PaySwiffStatusCheckResponse GetTransctionStatus(TransactionMaster transaction, string token, string MerchantAPIKey, string Mode)
        {          
            Project.Requests.MicroATM.PaySwiffStatusCheckRequest request = new Project.Requests.MicroATM.PaySwiffStatusCheckRequest();

            request.transcationType = "MICROATM";
            request.merchantRefNo = transaction.ProviderRequestID;
            request.merchantAPIKey = MerchantAPIKey;
            request.partnerAPIKey = token;

            string body = JsonConvert.SerializeObject(request);

            string respStr = HttpProcessing.Payswiff_TransactionStatus(body, Mode);        

            new Logger().Info("PaySwiff", "GetTransctionStatus", body + "::" + respStr);

            Log(transaction, body, respStr);
            var responseList = JsonConvert.DeserializeObject<List<Response.MicroATM.PaySwiffStatusCheckResponse>>(respStr);

            Response.MicroATM.PaySwiffStatusCheckResponse originalResponse = responseList.FirstOrDefault();

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