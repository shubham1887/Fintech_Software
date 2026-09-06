using Newtonsoft.Json;
using Project.BAL;
using Project.DAL;
using Project.Model;
using Project.Response.PayoutSetting;
using Project.StaticModel;
using System;
using System.Threading.Tasks;
using YesBankCore;
using YesBankCore.Request;
using YesBankCore.Response;

namespace Project.Processing.YesBank
{
    public class PayoutProcessing
    {
        private YesBankPayout mt = new YesBankPayout();
        public PayoutProcessing()
        {
            ViewModel payout = new PayoutSettingBal().SelectPayoutByName(PayoutCode.YesBankPayout);

            string MerchantID = payout.MerchantID;
            string MerchantKey = payout.Slat;
            string[] Salt = payout.ActionURL.Split(",");
            string XIBMClientSecret = Salt[0];
            string XIBMClientID = "";
            if (Salt.Length >= 2)
            {
                XIBMClientID = Salt[1];
            }
            string PrivateKeyFilePath = "";
            if (Salt.Length >= 3)
            {
                PrivateKeyFilePath = Salt[2];
            }

            string AuthUserName = "";
            if (Salt.Length >= 4)
            {
                AuthUserName = Salt[3];
            }

            string AuthPassword = "";
            if (Salt.Length >= 5)
            {
                AuthPassword = Salt[4];
            }

            string PrivateKeyPassword = payout.Token;
            string Mode = payout.Mode;

            mt.SetCredential(MerchantID, MerchantKey, XIBMClientID, XIBMClientSecret,
               PrivateKeyPassword, PrivateKeyFilePath, AuthUserName, AuthPassword, Mode);
        }

        public PayoutResponse DomesticPayment(TransactionMaster transactionMaster, UserMaster user)
        {
            PayoutRequest input = new PayoutRequest();
            input.beneName = transactionMaster.DMT_BeneficiaryName;
            input.accountNumber = transactionMaster.ConsumerNumber;
            input.beneIfscCode = transactionMaster.DMT_IFSC;
            input.amount = Convert.ToUInt32(transactionMaster.Amount).ToString();
            input.transactionMode = transactionMaster.DMT_PaymentType;
            input.beneBankName = transactionMaster.DMT_Bank;
            input.orderID = transactionMaster.ProviderRequestID;
            input.beneMobileNumber = transactionMaster.DMT_SenderNumber;
            input.customerName = transactionMaster.DMT_BeneficiaryName ?? user.PersonName;
            input.customerMobileNumber = user.MobileNumber;
            input.email = user.EmailID;
            input.pincode = user.Pincode ?? "400001";

            PayoutResponse resp = mt.DomesticPayment(input);

            string reqStr = JsonConvert.SerializeObject(input);
            string resStr = JsonConvert.SerializeObject(resp);
            new Logger().Info("YesBank", "Payout", reqStr + "::" + resStr);

            Log(transactionMaster, reqStr, resStr);

            return resp;
        }

        public PayoutResponse StatusCheck(TransactionMaster transactionMaster)
        {
            PayoutStatusCheckRequest input = new PayoutStatusCheckRequest();
            input.orderID = transactionMaster.ProviderRequestID;

            PayoutResponse resp = mt.StatusCheck(input);

            string reqStr = JsonConvert.SerializeObject(input);
            string resStr = JsonConvert.SerializeObject(resp);
            new Logger().Info("YesBAnk ", "Payout StatusCheck", reqStr + "::" + resStr);

            Log(transactionMaster, reqStr, resStr);

            return resp;
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