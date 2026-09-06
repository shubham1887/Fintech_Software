using Newtonsoft.Json;
using Project.BAL;
using Project.DAL;
using Project.Model;
using System;
using System.Threading.Tasks;


namespace Project.Processing.YesBank
{
    public class Hitachi_PayoutProcessing
    {
        private YesBankCore.Hitachi_PayoutYesBank mt = new YesBankCore.Hitachi_PayoutYesBank();
        public Hitachi_PayoutProcessing()
        {
            Project.Response.PayoutSetting.ViewModel payout = new PayoutSettingBal().SelectPayoutByName(Project.StaticModel.PayoutCode.YesBankPayout);
            mt.SetCredential(payout.MerchantID, payout.Slat, payout.Token);
        }
        public YesBankCore.Response.Payout.PayoutResponse MoneyTransfer(TransactionMaster transactionMaster, bool IsVerify)
        {
            string reqStr;
            string resStr;
            YesBankCore.Response.Payout.PayoutResponse resp = new YesBankCore.Response.Payout.PayoutResponse();
            if (IsVerify == false)
            {
                YesBankCore.Request.Payout.FundTransferRequest input = new YesBankCore.Request.Payout.FundTransferRequest();
                input.partnerReferenceNo = transactionMaster.ProviderRequestID;

                input.AccountNumber = transactionMaster.ConsumerNumber;
                input.AccountIFSC = transactionMaster.DMT_IFSC;
                input.Mode = transactionMaster.DMT_PaymentType;
                input.Amount = Convert.ToUInt32(transactionMaster.Amount).ToString();
                input.BeneficiaryName = transactionMaster.DMT_BeneficiaryName;

                resp = mt.MoneyTransfer(input);

                reqStr = JsonConvert.SerializeObject(input);
                resStr = JsonConvert.SerializeObject(resp);
                new Logger().Info("YesBank", "Payout", reqStr + "::" + resStr);

                Log(transactionMaster, reqStr, resStr);

                return resp;
            }
            YesBankCore.Request.Payout.VerificationRequest verification = new YesBankCore.Request.Payout.VerificationRequest();
            verification.RequestNumber = transactionMaster.ProviderRequestID;
            verification.AccountNumber = transactionMaster.ConsumerNumber;
            verification.AccountIFSC = transactionMaster.DMT_IFSC;
            verification.RemitterName = transactionMaster.DMT_SenderName;
            verification.RemitterMobile = transactionMaster.DMT_SenderNumber;
            verification.Remarks = "Verification";

            reqStr = JsonConvert.SerializeObject(verification);
            resStr = JsonConvert.SerializeObject(resp);
            new Logger().Info("YesBank", "Payout", reqStr + "::" + resStr);
            resp = mt.Verification(verification);
            Log(transactionMaster, reqStr, resStr);

            return resp;
        }

        public YesBankCore.Response.Payout.PayoutResponse StatusCheck(TransactionMaster transactionMaster)
        {
            YesBankCore.Request.Payout.PayoutStatusCheckRequest input = new YesBankCore.Request.Payout.PayoutStatusCheckRequest();
            input.PartnerReferenceNumber = transactionMaster.ProviderRequestID;
            input.RequestNumber = new Util().GetRandomNumberWithDatePrefix();
            YesBankCore.Response.Payout.PayoutResponse resp = mt.StatusCheck(input);

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
