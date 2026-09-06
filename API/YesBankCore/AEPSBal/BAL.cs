using Newtonsoft.Json;
using YesBankCore.Controller;
using YesBankCore.Model;
using YesBankCore.Request.AEPS;
using YesBankCore.Response.AEPS;
using YESBankCore.Controller;
using YESBankCore.Response;

namespace YesBankCore.AEPSBal
{
    public class BAL
    {
        public AuthenticationResponse Authentication(AuthenticationRequest input)
        {
            AuthenticationResponse response = new AuthenticationResponse();
            AEPSCredentials cred = new AEPSCredentials();
            try
            {
                new FingerPrintProcessing().extractAuthAttribute(input.BiometricData, input);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().saveRequest("Authentication FingerPrint Exception  : ", ex.Message);
            }

            string failureMessage = cred.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }

            failureMessage = input.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }

            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId;
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            AuthenticationAPIRequest request = new AuthenticationAPIRequest();
            request.txnId = input.TxnId;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.aadharNumber = input.AadharNumber;
            request.isVid = input.IsVid;
            request.txnType = input.TxnType;
            request.customerConsent = input.CustomerConsent;
            request.latitude = input.Latitude;
            request.longitude = input.Longitude;
            request.appVersion = input.AppVersion;
            request.appId = input.AppId;
            request.agentName = input.AgentName;
            request.agentAddress = input.AgentAddress;
            request.agentPincode = input.AgentPincode;
            request.agentDistrict = input.AgentDistrict;
            request.agentState = input.AgentState;
            request.agentCountry = input.AgentCountry;
            request.deviceType = input.DeviceType;
            request.deviceMake = input.DeviceMake;
            request.deviceModel = input.DeviceModel;
            request.deviceIp = input.DeviceIp;
            request.hashKey = HashKey;
            request.authType = input.authType;
            request.biometricData = input.BiometricData;

            string requestJSON = JsonConvert.SerializeObject(request);
            AEPSHTTPProcessingSOR http = new AEPSHTTPProcessingSOR();
       
            NetworkResponse netwotk = http.PostForAEPS(AEPSURL.getURL(AEPSURL.Authentication), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            string repose = netwotk.Response;
            response.Message = repose;
            response.Status = Message.PROCESS;

            AuthenticationAPIResponse res = JsonConvert.DeserializeObject<AuthenticationAPIResponse>(repose);
            if (res.data == null)
            {
                response.Status = Message.FAILURE;
                response.Message = res.errorResponse[0].errorMsg;
                //response.Message = res.responseMsg;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = res.responseMsg;       
                response.ReferenceNo = res.data.sorTxnId;
                response.DeviceType = request.deviceType;
                response.DeviceModel = request.deviceModel;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = res.errorResponse[0].errorMsg;
                response.DeviceType = request.deviceType;
                response.DeviceModel = request.deviceModel;
                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = res.responseMsg;
            response.DeviceType = request.deviceType;
            response.DeviceModel = request.deviceModel;
            return response;
        }

        public CashWithdrawalResponse Withdraw(CashWithdrawalRequest input)
        {
            CashWithdrawalResponse response = new CashWithdrawalResponse();
            AEPSCredentials cred = new AEPSCredentials();

            try
            {
                new FingerPrintProcessing().extractCashWithdrawAttribute(input.BiometricData, input);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().saveRequest("CashWithdraw FingerPrint Exception  : ", ex.Message);
            }

            string failureMessage = cred.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }

            failureMessage = input.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }
       
            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.Channel + input.CustomerMob + input.Amount;
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            CashWithdrawAPIRequest request = new CashWithdrawAPIRequest();
            request.txnId = input.TxnId;
            request.orderId = input.OrderId;
            request.sorTxnId = input.SorTxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.channel = input.Channel;
            request.aadharNumber = input.AadharNumber;
            request.amount = input.Amount;
            request.isVid = input.IsVid;
            request.customerName = input.CustomerName;
            request.bankIIN = input.BankIIN;
            request.biometricData = input.BiometricData;
            request.customerConsent = input.CustomerConsent;
            request.latitude = input.Latitude;
            request.longitude = input.Longitude;
            request.appVersion = input.AppVersion;
            request.appId = input.AppId;
            request.agentName = input.AgentName;
            request.agentAddress = input.AgentAddress;
            request.agentDistrict = input.AgentDistrict;
            request.agentPincode = input.AgentPincode;
            request.agentState = input.AgentState;
            request.agentCountry = input.AgentCountry;
            request.deviceType = input.DeviceType;
            request.deviceMake = input.DeviceMake;
            request.deviceModel = input.DeviceModel;
            request.deviceIp = input.DeviceIp;
            request.hashKey = HashKey;
            request.authType = input.authType;

            string requestJSON = JsonConvert.SerializeObject(request);
            AEPSHTTPProcessingSOR http = new AEPSHTTPProcessingSOR();

            NetworkResponse netwotk = http.PostForAEPS(AEPSURL.getURL(AEPSURL.CashWithdraw), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            string repose = netwotk.Response;
            response.Message = repose;
            response.Status = Message.PROCESS;

            CashWithdrawalAPIResponse res = JsonConvert.DeserializeObject<CashWithdrawalAPIResponse>(repose);        
            if (res.data == null)
            {
                response.Status = Message.FAILURE;
                //response.Message = res.responseMsg;
                response.Message = res.errorResponse[0].errorMsg;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = res.responseMsg;
                response.Balance = res.data.balanceAmount;            
                response.BankRRN = res.data.referenceNo;
                response.TransactionID = res.txnId;
                response.DateTime = res.data.transactionDate;
                response.DeviceType = request.deviceType;
                response.DeviceModel = request.deviceModel;
                response.SysTraceNo = res.data.sysTraceNo;
                response.AgentID = AEPSCredentials.AGENTID;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = res.errorResponse[0].errorMsg;
                //response.Message = res.responseMsg;
                response.Balance = res.data.balanceAmount;
                response.DeviceType = request.deviceType;
                response.DeviceModel = request.deviceModel;
                response.SysTraceNo = res.data.sysTraceNo;
                response.AgentID = AEPSCredentials.AGENTID;

                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = res.responseMsg;
            response.DeviceType = request.deviceType;
            response.DeviceModel = request.deviceModel;
            response.SysTraceNo = res.data.sysTraceNo;
            response.AgentID = AEPSCredentials.AGENTID;

            return response;
        }

        public CashWithdrawalResponse BalanceEnquiry(BalanceEnquiryRequest input)
        {
            CashWithdrawalResponse response = new CashWithdrawalResponse();
            AEPSCredentials cred = new AEPSCredentials();
            try
            {
                new FingerPrintProcessing().extractBalanceEnquiryAttribute(input.BiometricData, input);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().saveRequest("BalanceEnquiry FingerPrint Exception  : ", ex.Message);
            }

            string failureMessage = cred.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }          

            failureMessage = input.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }
         
            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.Channel + input.CustomerMob;
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            CashWithdrawAPIRequest request = new CashWithdrawAPIRequest();
            request.txnId = input.TxnId;
            request.orderId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.channel = input.Channel;
            request.aadharNumber = input.AadharNumber;
            request.bankIIN = input.BankIIN;
            request.biometricData = input.BiometricData;
            request.latitude = input.Latitude;
            request.longitude = input.Longitude;
            request.appId = input.AppId;
            request.agentName = input.AgentName;
            request.agentAddress = input.AgentAddress;
            request.agentPincode = input.AgentPincode;
            request.agentDistrict = input.AgentDistrict;
            request.agentState = input.AgentState;
            request.deviceType = input.DeviceType;
            request.deviceMake = input.DeviceMake;
            request.deviceModel = input.DeviceModel;
            request.deviceIp = input.DeviceIp;
            request.customerName = input.CustomerName;
            request.customerConsent = input.CustomerConsent;
            request.appVersion = input.AppVersion;
            request.isVid = input.IsVid;
            request.authType = input.authType;
            request.agentCountry = input.AgentCountry;      
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);
            AEPSHTTPProcessingSOR http = new AEPSHTTPProcessingSOR();

            NetworkResponse netwotk = http.PostForAEPS(AEPSURL.getURL(AEPSURL.BalanceEnquiry), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            string repose = netwotk.Response;
            response.Message = repose;
            response.Status = Message.PROCESS;

            BalanceEnquiryAPIResponse res = JsonConvert.DeserializeObject<BalanceEnquiryAPIResponse>(repose);
         
            if (res.data == null)
            {
                response.Status = Message.FAILURE;
                response.Message = res.errorResponse[0].errorMsg;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = res.responseMsg;
                response.Balance = res.data.balance;           
                response.BankRRN = res.data.referenceNo;
                response.TransactionID = res.txnId;
                response.SysTraceNo = res.data.sysTraceNo;
                response.DateTime = res.data.transactionDate;
                response.DeviceType = request.deviceType;
                response.DeviceModel = request.deviceModel;
                response.AgentID = AEPSCredentials.AGENTID;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = res.errorResponse[0].errorMsg;
                response.Balance = res.data.balance;
                response.DateTime = res.data.transactionDate;
                response.DeviceType = request.deviceType;
                response.SysTraceNo = res.data.sysTraceNo;
                response.DeviceModel = request.deviceModel;
                response.AgentID = AEPSCredentials.AGENTID;

                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = res.responseMsg;
            response.DeviceType = request.deviceType;
            response.SysTraceNo = res.data.sysTraceNo;
            response.DeviceModel = request.deviceModel;
            response.AgentID = AEPSCredentials.AGENTID;
            return response;
        }

        public MiniStatementResponse MiniStatement(MiniStatementRequest input)
        {
            MiniStatementResponse response = new MiniStatementResponse();
            AEPSCredentials cred = new AEPSCredentials();

            try
            {
                new FingerPrintProcessing().extractMiniStatementAttribute(input.BiometricData, input);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().saveRequest("MiniStatement FingerPrint Exception  : ", ex.Message);
            }

            string failureMessage = cred.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }

            failureMessage = input.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }
           
            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.Channel + input.CustomerMob;
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            MiniStatementAPIRequest request = new MiniStatementAPIRequest();
            request.txnId = input.TxnId;
            request.orderId = input.OrderID;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.channel = input.Channel;
            request.aadharNumber = input.AadharNumber;
            request.isVid = input.IsVid;
            request.bankIIN = input.BankIIN;
            request.biometricData = input.BiometricData;
            request.customerConsent = input.CustomerConsent;
            request.latitude = input.Latitude;
            request.longitude = input.Longitude;
            request.appVersion = input.AppVersion;
            request.appId = input.AppId;
            request.agentName = input.AgentName;
            request.agentAddress = input.AgentAddress;
            request.agentPincode = input.AgentPincode;
            request.agentDistrict = input.AgentDistrict;
            request.agentState = input.AgentState;
            request.agentCountry = input.AgentCountry;
            request.deviceType = input.DeviceType;
            request.deviceMake = input.DeviceMake;
            request.deviceModel = input.DeviceModel;
            request.deviceIp = input.DeviceIp;
            request.customerName = input.CustomerName;
            request.authType = input.authType;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);
            AEPSHTTPProcessingSOR http = new AEPSHTTPProcessingSOR();

            NetworkResponse netwotk = http.PostForAEPS(AEPSURL.getURL(AEPSURL.Ministatement), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            string repose = netwotk.Response;
            response.Message = repose;
            response.Status = Message.PROCESS;

            MiniStatementAPIResponse res = JsonConvert.DeserializeObject<MiniStatementAPIResponse>(repose);

            if (res.data == null)
            {
                response.Status = Message.FAILURE;
                response.Message = res.data.txnErrorMsg;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                string miniStatement = res.data.miniStatement;
                string balanceLine = miniStatement.Split('|').LastOrDefault(line => line.Contains("Balance:"));
                string balance = balanceLine?.Substring(balanceLine.IndexOf("Balance:") + "Balance:".Length).Trim();
                if (balance != null)
                {
                    balance = balance.Replace("+", " ");
                }

                response.Status = Message.SUCCESS;
                response.Message = res.responseMsg;
                response.MiniStatement = res.data.miniStatement;
                response.BankRRN = res.data.referenceNo;
                response.DateTime = res.data.transactionDate;
                response.Balance = balance;
                response.DeviceType = request.deviceType;
                response.DeviceModel = request.deviceModel;
                response.SysTraceNo = res.data.sysTraceNo;
                response.TransactionID = res.txnId;
                response.AgentID = AEPSCredentials.AGENTID;

                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                //response.Message = res.responseMsg;
                response.Message = res.data.txnErrorMsg;
                response.DeviceType = request.deviceType;
                response.DeviceModel = request.deviceModel;
                response.SysTraceNo = res.data.sysTraceNo;
                response.AgentID = AEPSCredentials.AGENTID;

                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = res.responseMsg;
            response.SysTraceNo = res.data.sysTraceNo;
            response.DeviceType = request.deviceType;
            response.DeviceModel = request.deviceModel;
            response.AgentID = AEPSCredentials.AGENTID;
            return response;
        }
    }
}
