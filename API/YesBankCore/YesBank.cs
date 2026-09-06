using YESBankCore.Controller;
using YESBankCore.Model;
using YESBankCore.Request;
using YESBankCore.Response;

namespace YESBankCore
{
    public class YesBank
    {
        private ErrorProcessing log = new ErrorProcessing();

        public void SetCredential(string MerchantId, string MerchantKey, string IBMClientID, string XIBMClientSecret, string PrivateKeyPassword, string PrivateKeyFilePath)
        {
            Credential.MerchantId = MerchantId;
            Credential.MerchantKey = MerchantKey;

            Credential.XIBMClientID = IBMClientID;
            Credential.XIBMClientSecret = XIBMClientSecret;

            Credential.PrivateKeyPassword = PrivateKeyPassword;

            string[] arr = PrivateKeyFilePath.Split(',');
            Credential.PrivateKeyFilePath = arr[0].Trim();
            if (arr.Length >= 2)
            {
                Credential.Mode = arr[1].Trim();
            }
        }

        public QRInitResponse QRInit(QRPayRequest input)
        {
            QRInitResponse response = new QRInitResponse();
            response.Status = Message.FAILURE;
            response.Message = "";

            try
            {
                Credential cred = new Credential();
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

                HTTPProcessing http = new HTTPProcessing();
                string plainText = $"{Credential.MerchantId}|{input.OrderNo}||{input.Amount}||{Credential.PrivateKeyPassword}|10||||||||NA|NA";

                NetworkResponse network = http.Post(URL.getURL(URL.QRCollect), plainText);

                if (network.Status.EqualsIgnoreCase(Message.PROCESS))
                {
                    response.Status = Message.PROCESS;
                    response.Message = network.Response;
                    return response;
                }

                if (!network.Status.EqualsIgnoreCase(Message.OK))
                {
                    response.Status = Message.PROCESS;
                    response.Message = network.Response;
                    return response;
                }

                string fields = "RefNo|Order No|Amount|Date|Status|Status Desc|Txn ID|Payee VPA|Payee Name| Package List JSON |MCC|add1|add2|add3|add4|add5|add6|add7|add8|add9|add10 ";

                Dictionary<string, string> list = Util.getFields(fields, network.Response);
                response.TransactionID = Util.getValue(list, "RefNo");
                response.Message = Util.getValue(list, "Status Desc");
                response.AdminVPA = Util.getValue(list, "Payee VPA");
                response.MerchantName = Util.getValue(list, "Payee Name");
                response.StatusCode = Util.getValue(list, "Status");
                response.Message = Util.getValue(list, "Status Desc");
                response.MCCCode = Util.getValue(list, "MCC");
                response.Status = Message.PROCESS;
                if (response.StatusCode.EqualsIgnoreCase("S"))
                {
                    response.Status = Message.SUCCESS;
                }
                if (response.StatusCode.EqualsIgnoreCase("F"))
                {
                    response.Status = Message.FAILURE;
                }
                if (response.StatusCode.EqualsIgnoreCase("T"))
                {
                    response.Status = Message.FAILURE;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public StatusCheckResponse StatusCheck(StatusCheckRequest input)
        {
            StatusCheckResponse response = new StatusCheckResponse();
            response.Status = Message.PROCESS;
            response.Message = "";
            response.OrderNo = "";

            try
            {
                Credential cred = new Credential();
                string failureMessage = cred.IsValidModalState();

                if (!failureMessage.EqualsIgnoreCase(Message.OK))
                {
                    response.Status = Message.PROCESS;
                    response.Message = failureMessage;
                    return response;
                }

                failureMessage = input.IsValidModalState();

                if (!failureMessage.EqualsIgnoreCase(Message.OK))
                {
                    response.Status = Message.PROCESS;
                    response.Message = failureMessage;
                    return response;
                }

                response.OrderNo = input.OrderNo;

                HTTPProcessing http = new HTTPProcessing();
                string plainText = $"{Credential.MerchantId}|{input.OrderNo}||||||||||||NA|NA";

                NetworkResponse network = http.Post(URL.getURL(URL.StatusQuery), plainText);

                if (network.Status.EqualsIgnoreCase(Message.PROCESS))
                {
                    response.Status = Message.PROCESS;
                    response.Message = network.Response;
                    return response;
                }

                if (!network.Status.EqualsIgnoreCase(Message.OK))
                {
                    response.Status = Message.PROCESS;
                    response.Message = network.Response;
                    return response;
                }

                if (network.Response.ToLower().Contains("NA|FAILURE|NORECORD|NA"))
                {
                    response.Status = Message.PROCESS;
                    response.Message = "FAILURE|NORECORD";
                    return response;
                }

                string fields = "YBL TxnID|MerchantTxn ID|Amount|Txn Auth Date|status|statusdesc|respcode|approvalno|payerVA|NPCItxnid|referanceid|CustRefid| payer acc no | payer ifsc|Payer acc name| payee VA |payee ifsc| payee acc no | payee AADHAAR NO|payee name |add1|add2|add3|add4|add5|add6|add7|add8|add9|add10";

                Dictionary<string, string> list = Util.getFields(fields, network.Response);
                response.TransactionID = Util.getValue(list, "CustRefid");
                response.CustomerVPA = Util.getValue(list, "payerVA");
                response.StatusCode = Util.getValue(list, "status");
                response.Message = Util.getValue(list, "statusdesc");
                response.Amount = Util.getValue(list, "Amount");

                response.Status = Message.PROCESS;
                if (response.StatusCode.EqualsIgnoreCase("SUCCESS"))
                {
                    response.Status = Message.SUCCESS;
                }
                if (response.StatusCode.EqualsIgnoreCase("FAILED"))
                {
                    response.Status = Message.FAILURE;
                }
                //if (response.StatusCode.EqualsIgnoreCase("T"))
                //{
                //    response.Status = Message.FAILURE;
                //}

                if (response.Message.EqualsIgnoreCase("NORECORD"))
                {
                    response.Status = Message.PROCESS;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public QRPayResponse VPAValidate(VPAValidateRequest input)
        {
            QRPayResponse response = new QRPayResponse();
            response.Status = Message.FAILURE;
            response.Message = "";

            try
            {
                Credential cred = new Credential();
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

                HTTPProcessing http = new HTTPProcessing();
                string plainText = $"{Credential.MerchantId}|{input.OrderNo}|{input.VPA}|T|com.msg.app|0.0 ,0.0 |Mumbai|172.16.50.168|MOB|5200000200010004000639292929292|Android7.0|351898082074677|89914902900059967808|4e9389eadeea5b7c|02:00:00:00:00:00|02:00:00:00:00:00|||||||||NA|NA";
                //plainText = "YES0000000063923|YESL9098908werrtdF8C|9930738842@yesb|T|com.msg.app|0.0 ,0.0 |Mumbai|172.16.50.65|MOB|5200000200010004000639292929292|Android7.0|351898082074677|89914902900059967808|4e9389eadeea5b7c|02:00:00:00:00:00|02:00:00:00:00:00|||||||||NA|NA";
                NetworkResponse network = http.Post(URL.getURL(URL.CheckVirtualAddress), plainText);

                if (network.Status.EqualsIgnoreCase(Message.PROCESS))
                {
                    response.Status = Message.PROCESS;
                    response.Message = network.Response;
                    return response;
                }

                if (!network.Status.EqualsIgnoreCase(Message.OK))
                {
                    response.Status = Message.PROCESS;
                    response.Message = network.Response;
                    return response;
                }

                //"yblRefNo | virtualAddress | maskName | status | statusDesc | add1 | add2 | add3 | add4 | add5 | add6 | add7 | add8 | add9 | add10 |"
                //1234567 | rahul@yesb | RAHUL AGRAWAL | VE | SUCCESS | NA | NA | NA | NA | NA | NA | NA | NA | NA | NA
                string fields = "yblRefNo | virtualAddress | maskName | status | statusDesc | add1 | add2 | add3 | add4 | add5 | add6 | add7 | add8 | add9 | add10 |";

                Dictionary<string, string> list = Util.getFields(fields, network.Response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public StatusCheckResponse Callback(string inputText)
        {
            StatusCheckResponse response = new StatusCheckResponse();
            response.Status = Message.PROCESS;
            response.Message = "";

            try
            {
                if (inputText.Trim().StartsWith("meRes="))
                {
                    inputText = inputText.Replace("meRes=", "");
                }

                string descData = new EncryptDecrypt().Decrypt(Credential.MerchantKey, inputText);

                log.saveRequest("Callback data", descData);

                string fields = "YBL TxnID|MerchantTxn ID|Type|Amount|Txn Auth Date|status|statusdesc|respcode|ErrorCode|approvalNumber|payerVA|NPCItxnid|referanceid|CustRefid |payerMobile| payer Note|UniqueCustomerRefNo|payer acc no|payer ifsc|payerName | payee VA |payee ifsc| payee acc no | payee AADHAAR NO|payee name |add1|add2|add3|add4|add5|add6|add7|add8|add9|add10 ";

                Dictionary<string, string> list = Util.getFields(fields, descData);
                response.OrderNo = Util.getValue(list, "MerchantTxn ID");

                response.Message = Util.getValue(list, "statusdesc");
                response.Status = Message.PROCESS;
                response.Amount = Util.getValue(list, "Amount");

                response.TransactionID = Util.getValue(list, "CustRefid");

                if (response.TransactionID.TrimIfNotNull().Length <= 3)
                {
                    response.TransactionID = Util.getValue(list, "UniqueCustomerRefNo");
                }

                response.StatusCode = Util.getValue(list, "status");
                response.CustomerVPA = Util.getValue(list, "payerVA");

                if (response.StatusCode.EqualsIgnoreCase("S"))
                {
                    response.Status = Message.SUCCESS;
                }
                if (response.StatusCode.EqualsIgnoreCase("F"))
                {
                    response.Status = Message.FAILURE;
                }
                //if (response.StatusCode.EqualsIgnoreCase("T"))
                //{
                //    response.Status = Message.FAILURE;
                //}
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}