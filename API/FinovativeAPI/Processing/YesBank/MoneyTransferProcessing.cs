using Newtonsoft.Json;
using Project.BAL;
using Project.DAL;
using Project.Model;
using Project.Requests.Transaction;
using Project.StaticModel;
using System;
using System.Threading.Tasks;
using YesBankCore;
using YesBankCore.Request.MoneyTransfer;
using YesBankCore.Response.MoneyTransfer;

namespace Project.Processing.YesBank
{
    public class MoneyTransferProcessing
    {
        private YesBankDMTSOR mt = new YesBankDMTSOR();

        public MoneyTransferProcessing()
        {
            DMTSetting dmt = new DMTSettingsBal().SelectDMTByName(DMTCode.YesBankDMT);

            string AgentID = dmt.Token;
            string PartnerID = dmt.UserName;
            string PrivateKey = dmt.WalletID;
            string PublicKey = dmt.CallBackURL;
            string Mode = dmt.Mode;
            mt.SetCredential(PartnerID, AgentID, PrivateKey, PublicKey, Mode);
        }

        public string GetTxnID()
        {
            string reqid = Guid.NewGuid().ToString().Substring(0, 15);
            reqid = reqid.Replace("-", "");
            return reqid;
        }

        public SenderSelectResponse SelectSenderDetail(string mobile)
        {
            SenderSelectRequest input = new SenderSelectRequest();
            input.CustomerMob = mobile;
            input.TxnId = GetTxnID();

            SenderSelectResponse resp = mt.SelectSenderDetail(input);
            return resp;
        }

        public SenderAddResponse SenderRegistration(Requests.Sender.InsertRequest mainRequest, UserMaster user)
        {
            SenderAddResponse resp = new SenderAddResponse();

            string resultMessage = CheckAgeRange(mainRequest.FromDate);
            if (!resultMessage.EqualsIgnoreCase(StatusType.Ok))          
            {
                resp.Message = resultMessage;
                return resp;
            }

            SenderAddRequest input = new SenderAddRequest();
            input.CustomerMob = mainRequest.Mobile;
            input.Name = mainRequest.Name;
            input.Pincode = mainRequest.PinCode;
            input.City = mainRequest.CityName;
            input.State = mainRequest.State;
            input.District = mainRequest.District;
            input.Address = mainRequest.Address;
            input.DateOfBirth = mainRequest.FromDate.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            input.Gender = mainRequest.Gender;
            input.TxnId = GetTxnID();

            if (mainRequest.Gender == "M" || mainRequest.Gender == "O")
            {
                input.Title = "Mr";
            }
            else if (mainRequest.Gender == "F")
            {
                input.Title = "Mrs";
            }

             resp = mt.SenderRegistration(input);
            return resp;
        }

        public AddBeneOTPResponse AddBeneficiary(Requests.Beneficiary.InsertRequest mainReq)
        {
            BeneAddSendOTPRequest input = new BeneAddSendOTPRequest();
            input.AccountNo = mainReq.AccountNo;
            input.CustomerMob = mainReq.SenderMobile;
            input.Name = mainReq.Name;
            input.TxnId = GetTxnID();

            AddBeneOTPResponse resp = mt.AddBeneficiary(input);
            return resp;
        }

        public AddBeneResponse ValidateBeneficiaryOTP(Requests.Beneficiary.BeneficiaryVerifyOTPRequest req, UserMaster user)
        {
            BeneAddRequest input = new BeneAddRequest();
            input.CustomerMob = req.Mobile;
            input.Otp = req.OTP;
            input.BenefAccNo = req.AccountNo;
            input.IfscCode = req.IFSC;
            input.Name = req.BeneName;
            input.PinCode = user.Pincode;
            input.City = user.CityName;
            input.Address = user.Address;
            input.TxnId = GetTxnID();

            AddBeneResponse resp = mt.ValidateBeneficiaryOTP(input);
            return resp;
        }

        public BeneListResponse ViewBeneficiaryList(string mobile)
        {
            BeneListRequest input = new BeneListRequest();
            input.CustomerMob = mobile;
            input.TxnId = GetTxnID();

            BeneListResponse resp = mt.GetAllBeneficiary(input);
            return resp;
        }

        public DeActivateBeneResponse DeleteBeneficiary(Project.Requests.Beneficiary.DeleteRequest mainReq)
        {
            BeneDeactivationRequest input = new BeneDeactivationRequest();
            input.CustomerMob = mainReq.MobileNumber;
            input.AccountNo = mainReq.AccountNumber;
            input.IfscCode = mainReq.IFSC;
            input.TxnId = GetTxnID();

            DeActivateBeneResponse resp = mt.DeleteBeneficiary(input);
            return resp;
        }

        public TransferResponse VerifyBeneficiaryAccount(MoneyTransferRequest mainReq, TransactionMaster transactionMaster)
        {
            VerifyRequest input = new VerifyRequest();
            input.BeneName = transactionMaster.DMT_SenderName;
            input.CustomerMob = mainReq.Mobile;
            input.AccountNo = mainReq.AccountNo;
            input.Ifsc = mainReq.IFSC;
            input.TxnId = GetTxnID();

            TransferResponse verifyresp = mt.VerifyBeneficiaryAccount(input);

            string reqStr = JsonConvert.SerializeObject(input);
            string resStr = JsonConvert.SerializeObject(verifyresp);
            new Logger().Info("YesBankDMT", "VerifyBeneficiaryAccount", reqStr + "::" + resStr);
            Log(transactionMaster, reqStr, resStr);

            return verifyresp;
        }

        public TransferResponse MoneyTransfer(MoneyTransferRequest mainReq, int SplitAmount, TransactionMaster transactionMaster)
        {
            TransferResponse resp = null;
            UserMaster user = new UserDAL().SelectByID(transactionMaster.UserID);
            if (mainReq.IsVerify == false)
            {
                TransferRequest input = new TransferRequest();
                input.CustomerMob = mainReq.Mobile;
                input.MobileNo = mainReq.Mobile;
                input.AccountNo = mainReq.AccountNo;
                input.OrderId = transactionMaster.ProviderRequestID;
                input.Ifsc = mainReq.IFSC;
                input.Name = mainReq.Name;
                input.EmailId = user.EmailID;
                input.PaymentMode = mainReq.PaymentType;
                input.Amount = SplitAmount.ToString();
                input.TxnId = GetTxnID();

                resp = mt.Transfer(input);

                string reqStr = JsonConvert.SerializeObject(input);
                string resStr = JsonConvert.SerializeObject(resp);
                new Logger().Info("YesBankDMT ", "MoneyTransfer", reqStr + "::" + resStr);

                Log(transactionMaster, reqStr, resStr);
            }
            else
            {
                resp = VerifyBeneficiaryAccount(mainReq, transactionMaster);
            }

            return resp;
        }

        public TransferResponse GetTransctionStatus(TransactionMaster transactionMaster)
        {
            TransferResponse resp = null;
            YesBankCore.Request.MoneyTransfer.StatusCheckRequest input = new YesBankCore.Request.MoneyTransfer.StatusCheckRequest();
            input.OrderId = transactionMaster.ProviderRequestID;
            input.CustomerMob = transactionMaster.DMT_SenderNumber;
            input.FromDate = transactionMaster.DOC.ToString("dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            input.ToDate = transactionMaster.DOC.ToString("dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            input.TxnId = GetTxnID();
            input.OrgTxnId = transactionMaster.ProviderDOCNo;

            if (transactionMaster.Amount == 1)
            {
                resp = GetVerificationStatus(transactionMaster);
            }
            else
            {
                resp = mt.TransctionStatusCheck(input);
            }
         
            string reqStr = JsonConvert.SerializeObject(input);
            string resStr = JsonConvert.SerializeObject(resp);
            new Logger().Info("YesBankDMT ", "GetTransctionStatus", reqStr + "::" + resStr);

            Log(transactionMaster, reqStr, resStr);

            return resp;
        }

        public TransferResponse GetVerificationStatus(TransactionMaster transactionMaster)
        {
            YesBankCore.Request.MoneyTransfer.VerifyStatusCheckRequest input = new YesBankCore.Request.MoneyTransfer.VerifyStatusCheckRequest();
            input.OrgTxnId = transactionMaster.ProviderDOCNo;
            input.CustomerMob = transactionMaster.DMT_SenderNumber;
            input.TxnId = GetTxnID();

            TransferResponse resp = mt.VerifyTransctionStatusCheck(input);

            string reqStr = JsonConvert.SerializeObject(input);
            string resStr = JsonConvert.SerializeObject(resp);
            new Logger().Info("YesBankDMT ", "GetTransctionStatus", reqStr + "::" + resStr);

            Log(transactionMaster, reqStr, resStr);

            return resp;
        }

        public RefundTransferResponse TransferSendOTPRefund(TransactionMaster transactionMaster)
        {
            RefundOTPRequest input = new RefundOTPRequest();
            input.TxnId = GetTxnID();
            input.CustomerMob = transactionMaster.DMT_SenderNumber;
            input.OrderId = transactionMaster.ProviderRequestID;
            input.SortxnId = transactionMaster.Bill_Optional1;
            RefundTransferResponse resp = mt.SendOTPRefund(input);

            string reqStr = JsonConvert.SerializeObject(input);
            string resStr = JsonConvert.SerializeObject(resp);
            new Logger().Info("YesBankDMT ", "Refund With SendOTP", reqStr + "::" + resStr);
            return resp;
        }

        public RefundTransferResponse TransferVerifyOTPRefund(TransactionMaster transactionMaster, string OTP)
        {
            RefundTransactionRequest input = new RefundTransactionRequest();
            input.TxnId = GetTxnID();
            input.OTP = OTP;
            input.CustomerMob = transactionMaster.DMT_SenderNumber;
            input.OrderId = transactionMaster.ProviderRequestID;
            input.SortxnId = transactionMaster.Bill_Optional1;
            RefundTransferResponse resp = mt.VerifyOTPRefund(input);

            string reqStr = JsonConvert.SerializeObject(input);
            string resStr = JsonConvert.SerializeObject(resp);
            new Logger().Info("YesBankDMT ", "Refund With OTP", reqStr + "::" + resStr);
            return resp;
        }

        public RefundTransferResponse VerifyBeneSendOTPRefund(TransactionMaster transactionMaster)
        {
            VerifyBeneRefundOTPRequest input = new VerifyBeneRefundOTPRequest();
            input.TxnId = GetTxnID();
            input.CustomerMob = transactionMaster.DMT_SenderNumber;
            input.OrgTxnId = transactionMaster.ProviderDOCNo;
         
            RefundTransferResponse resp = mt.VerifyBeneSendOTPRefund(input);

            string reqStr = JsonConvert.SerializeObject(input);
            string resStr = JsonConvert.SerializeObject(resp);
            new Logger().Info("YesBankDMT ", "VerifyRefund With SendOTP", reqStr + "::" + resStr);
            return resp;
        }

        public RefundTransferResponse BeneVerifyOTPRefund(TransactionMaster transactionMaster, string OTP)
        {
            VerifyBeneRefundTransactionRequest input = new VerifyBeneRefundTransactionRequest();
            input.TxnId = GetTxnID();
            input.Otp = OTP;
            input.CustomerMob = transactionMaster.DMT_SenderNumber;
            input.OrgTxnId = transactionMaster.ProviderRequestID;

            RefundTransferResponse resp = mt.BeneVerifyOTPRefund(input);

            string reqStr = JsonConvert.SerializeObject(input);
            string resStr = JsonConvert.SerializeObject(resp);
            new Logger().Info("YesBankDMT ", "BeneVerifyRefund With OTP", reqStr + "::" + resStr);
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
                    ServiceID = transactionMaster.ServiceID
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

        public string CheckAgeRange(DateTime birthDate)
        {
            DateTime currentDate = DateTime.Now;
            int age = currentDate.Year - birthDate.Year;            
            string message = StatusType.Ok;

            if (age < 18)
            return message = $"Age: {age}. Age is less than 18, which is outside the allowed range.";

             if (age > 80)           
                return message = $"Age: {age}. Age is greater than 80, which is outside the allowed range.";
                               
            return message;
        }
    }
}