using Newtonsoft.Json;
using Project.BAL;
using Project.Model;
using Project.Requests.AEPS.YesBankAEPS;
using Project.Response.AEPSSetting;
using System;
using YesBankCore;
using YesBankCore.Request.AEPS;
using YesBankCore.Response.AEPS;

namespace Project.Processing.YesBank
{
    public class Hitachi_AEPSProcessing
    {
        private YesBankAEPSSOR aeps = new YesBankAEPSSOR();

        public Hitachi_AEPSProcessing()
        {
            ViewModel view = new AEPSSettingBal().SelectYesBank();

            if (view == null)
            {
                return;
            }

            string AGENTID = view.MerchantID;
            string PARTNERID = view.Slat;
            string PRIVATEKEY = view.Token;
            string PUBLICKEY = view.PublicKey;
            string MODE = view.Mode;

            aeps.SetCredential(AGENTID, PARTNERID, PRIVATEKEY, PUBLICKEY, MODE);
        }

        internal AuthenticationResponse Authentication(YesBank_AuthenticationRequest mainRequest, string requestID, string IPAddress, UserMaster user)
        {
            AuthenticationRequest request = new AuthenticationRequest();
            request.AadharNumber = mainRequest.AadhaarNumber;
            request.Latitude = mainRequest.Latitude;
            request.Longitude = mainRequest.Longitude;
            request.AgentName = user.PersonName;
            request.AgentAddress = user.Address;
            request.AgentPincode = user.Pincode;
            request.AgentState = "Maharashtra";
            //request.AgentState = user.State.Name;
            request.BiometricData = mainRequest.FingerPrint;
            request.DeviceIp = IPAddress;
            request.TxnId = requestID;
            request.AgentId = user.AliasName;
            if (mainRequest.AadhaarNumber.Length > 12)
            {
                request.IsVid = "true";
            }
            else
            {
                request.IsVid = "false";
            }

            AuthenticationResponse resp = aeps.Authentication(request);

            string reqStr = JsonConvert.SerializeObject(request);
            string resStr = JsonConvert.SerializeObject(resp);

            //new Logger().Info("YesBankAEPS", "Authentication", reqStr + "::" + resStr);

            return resp;
        }

        internal CashWithdrawalResponse Withdraw(YesBank_CashWithdrawalRequest mainRequest, string IPAddress, string requestID, UserMaster user)
        {
            CashWithdrawalResponse resp = new CashWithdrawalResponse();
            try
            {
                CashWithdrawalRequest request = new CashWithdrawalRequest();
                request.AadharNumber = mainRequest.AadhaarNumber;
                request.BankIIN = mainRequest.IIN;
                request.Latitude = mainRequest.Latitude;
                request.Longitude = mainRequest.Longitude;
                request.CustomerMob = mainRequest.ConsumerNumber;
                request.BiometricData = mainRequest.FingerPrint;
                request.Amount = mainRequest.Amount.ToString();
                request.AgentName = user.PersonName;
                request.AgentAddress = user.Address;
                request.AgentPincode = user.Pincode;
                request.AgentState = "Maharashtra";
                request.TxnId = requestID;
                request.DeviceIp = IPAddress;
                request.OrderId = requestID;
                request.SorTxnId = requestID;
                request.CustomerName = user.PersonName;
                request.AgentId = user.AliasName;
                if (mainRequest.AadhaarNumber.Length > 12)
                {
                    request.IsVid = "true";
                }
                else
                {
                    request.IsVid = "false";
                }

                resp = aeps.Withdraw(request);

                string reqStr = JsonConvert.SerializeObject(request);
                string resStr = JsonConvert.SerializeObject(resp);

                new Logger().Info("YesBankAEPS", "CashWithdraw", reqStr + "::" + resStr);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return resp;
        }

        internal CashWithdrawalResponse BalanceEnquiry(YesBank_BalanceEnquiryRequest mainRequest, string requestID,string IP, UserMaster user)
        {
            BalanceEnquiryRequest request = new BalanceEnquiryRequest();
            
            request.AadharNumber = mainRequest.AadhaarNumber;
            request.BankIIN = mainRequest.IIN;
            request.Latitude = mainRequest.Latitude;
            request.Longitude = mainRequest.Longitude;
            request.CustomerMob = mainRequest.ConsumerNumber;
            request.TxnId = requestID;
            request.OrderId = requestID;
            request.BiometricData = mainRequest.FingerPrint;
            request.AgentName = user.PersonName;
            request.AgentAddress = user.Address;
            request.AgentPincode = user.Pincode;
            request.AgentState = "Maharashtra";
            request.DeviceIp = IP;
            request.CustomerName = user.PersonName;
            request.AgentId = user.AliasName;
            if (mainRequest.AadhaarNumber.Length > 12)
            {
                request.IsVid = "true";
            }
            else
            {
                request.IsVid = "false";
            }

            CashWithdrawalResponse resp = aeps.BalanceEnquiry(request);

            string reqStr = JsonConvert.SerializeObject(request);
            string resStr = JsonConvert.SerializeObject(resp);

            new Logger().Info("YesBankAEPS", "BalanceEnquiry", reqStr + "::" + resStr);

            return resp;
        }

        internal MiniStatementResponse MiniStatement(YesBank_MinistatementRequest mainRequest, string requestID, string IP, UserMaster user)
        {
            MiniStatementRequest request = new MiniStatementRequest();          

            request.AadharNumber = mainRequest.AadhaarNumber;
            request.BankIIN = mainRequest.IIN;
            request.Latitude = mainRequest.Latitude;
            request.Longitude = mainRequest.Longitude;
            request.BiometricData = mainRequest.FingerPrint;
            request.CustomerMob = mainRequest.ConsumerNumber;
            request.AgentName = user.PersonName;
            request.AgentAddress = user.Address;
            request.AgentPincode = user.Pincode;
            request.AgentState = "Maharashtra";
            request.TxnId = requestID;
            request.OrderID = requestID;
            request.DeviceIp = IP;
            request.CustomerName = user.PersonName;
            request.AgentId = user.AliasName;
            if (mainRequest.AadhaarNumber.Length > 12)
            {
                request.IsVid = "true";
            }
            else
            {
                request.IsVid = "false";
            }

            MiniStatementResponse resp = aeps.MiniStatement(request);

            string reqStr = JsonConvert.SerializeObject(request);
            string resStr = JsonConvert.SerializeObject(resp);

            //new Logger().Info("YesBankAEPS", "MiniStatement", reqStr + "::" + resStr);

            return resp;
        }

        internal string isValidCapturedFingerPrint(string FingerPrint)
        {
            return aeps.isValidCapturedFingerPrint(FingerPrint);
        }

    }
}