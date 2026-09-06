using Newtonsoft.Json;
using System.Net.NetworkInformation;
using YesBankCore.Controller;
using YesBankCore.Model;
using YesBankCore.Request.BBPS;
using YesBankCore.Response.BBPSResponse;
using YESBankCore.Controller;
using YESBankCore.Response;

namespace YesBankCore
{
    public class YesBankBBPS
    {
        public void SetCredential(string agentId, string partnerId, string PrivateKey, string PublicKey)
        {
            BBPSCredential.AgentId = agentId;
            BBPSCredential.PartnerId = partnerId;
            BBPSCredential.PublicKey = PublicKey;
            BBPSCredential.PrivateKey = PrivateKey;
        }

        public BillFetchResponse BillFetch(BillFetchRequest request, string IP, string MAC, string Email, string Aadhar, string Pan, string Mobile, string Name, ParamLabelViewModel label, string BillerID)
        {
            BillFetchResponse billFetchResponse = new BillFetchResponse();
            BillFetchAPIRequest input = new BillFetchAPIRequest();

            billFetchResponse.Status = StatusType.Process;
            input.custDetails = new CustDetails();
            input.billDetails = new BillDetails();

            input.chId = 1;
            input.custDetails.mobileNo = request.MobileNo;

            input.custDetails.customerTags = new List<CustomerTag>();

            input.custDetails.customerTags.Add(new CustomerTag { name = "EMAIL", value = Email });

            input.agentDetails = new AgentDetails();

            input.agentDetails.agentId = BBPSCredential.AgentId;
            input.agentDetails.partnerId = BBPSCredential.PartnerId;

            input.agentDetails.deviceTags = new List<DeviceTag>();

            input.agentDetails.deviceTags.Add(new DeviceTag { name = "INITIATING_CHANNEL", value = "BSC" });
            input.agentDetails.deviceTags.Add(new DeviceTag { name = "GEOCODE", value = "12.9667, 77.5667" });
            input.agentDetails.deviceTags.Add(new DeviceTag { name = "MOBILE", value = Mobile });
            input.agentDetails.deviceTags.Add(new DeviceTag { name = "TERMINAL_ID", value = "12345698" });
            input.agentDetails.deviceTags.Add(new DeviceTag { name = "POSTAL_CODE", value = "400059" });


            input.billDetails.billerId = BillerID;

            var labelParamMapping = GetFetchLabelParamMapping(label, request);
            input.billDetails.customerParams = new List<CustomerParam>();

            input.billDetails.customerParams.Add(new CustomerParam { name = request.Label, value = request.ConsumerNumber });

            foreach (var entry in labelParamMapping)
            {

                input.billDetails.customerParams.Add(new CustomerParam { name = entry.Key, value = entry.Value });
            }

            string requestJSON = JsonConvert.SerializeObject(input);

            BBPSHttpProcessing httpProcessing = new BBPSHttpProcessing();


            NetworkResponse netwotk = httpProcessing.BBPSPOST(BBPSURL.BillFetchURL, requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(StatusType.Ok))
            {
                billFetchResponse.Status = StatusType.Process;
                billFetchResponse.Message = netwotk.Response;
                return billFetchResponse;
            }

            BillFetchAPIResponse apiResponse = null;

            apiResponse = JsonConvert.DeserializeObject<BillFetchAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                billFetchResponse.Status = Message.FAILURE;
                billFetchResponse.Message = apiResponse.response.responseReason;
                return billFetchResponse;
            }
            if (apiResponse.status.EqualsIgnoreCase(StatusType.Failure))
            {
                billFetchResponse.Status = StatusType.Failure;
                billFetchResponse.Message = apiResponse.response.complianceReason;
                return billFetchResponse;
            }

            if (apiResponse.status.EqualsIgnoreCase(StatusType.Success))
            {
                billFetchResponse.Status = StatusType.Success;
                billFetchResponse.Message = apiResponse.response.responseReason;
                billFetchResponse.BillerName = apiResponse.response.billerResponse.customerName;
                billFetchResponse.Amount = apiResponse.response.billerResponse.amount;
                
                billFetchResponse.BillDate = apiResponse.response.billerResponse.billDate;
                billFetchResponse.BillDueDate = apiResponse.response.billerResponse.dueDate;
                billFetchResponse.BillerID = BillerID;

                if (DateTime.TryParse(billFetchResponse.BillDueDate, out DateTime billDueDate))
                {
                    if (billDueDate < DateTime.Today)
                    {
                        var latePaymentAmountTag = apiResponse.response.billerResponse.billTags
                                                   .FirstOrDefault(tag => tag.name == "Late Payment Amount");

                        if (latePaymentAmountTag != null)
                        {
                            billFetchResponse.Amount = latePaymentAmountTag.value;
                        }
                    }
                }
                billFetchResponse.BillValidateId = apiResponse.response.refId;
                return billFetchResponse;
            }
            billFetchResponse.Status = StatusType.Process;
            billFetchResponse.BillerName = apiResponse.response.billerResponse.customerName;
            billFetchResponse.Amount = apiResponse.response.billerResponse.amount;
            billFetchResponse.BillDate = apiResponse.response.billerResponse.billDate;
            billFetchResponse.BillDueDate = apiResponse.response.billerResponse.dueDate;
            billFetchResponse.BillValidateId = apiResponse.response.refId;
            billFetchResponse.BillerID = BillerID;

            return billFetchResponse;

        }

        public BillPayResponse BillPay(BillPayRequest request, string IP, string Amount, string RefID, string MAC, ParamLabelViewModel label, string BillerID, string Email, string Aadhar, string Pan, string Mobile)
        {
            BillPayResponse billPayResponse = new BillPayResponse();
            BillPayAPIRequest input = new BillPayAPIRequest();

            billPayResponse.Status = StatusType.Process;

            input.chId = 1;
            input.refId = RefID;
            input.clientRequestId = RefID;

            input.agentDetails = new AgentDetailsPay();
            input.agentDetails.deviceTags = new List<DeviceTagPay>();



            input.agentDetails.agentId = BBPSCredential.AgentId;
            input.agentDetails.partnerId = BBPSCredential.PartnerId;

            input.agentDetails.deviceTags.Add(new DeviceTagPay { name = "INITIATING_CHANNEL", value = "BSC" });
            input.agentDetails.deviceTags.Add(new DeviceTagPay { name = "IP", value = IP });
            input.agentDetails.deviceTags.Add(new DeviceTagPay { name = "MAC", value = MAC });

            input.amountDetails = new AmountDetails();
            input.amountDetails.amount = Amount;
            input.amountDetails.currency = "356";
            input.amountDetails.custConvFee = "0";
            input.amountDetails.couCustConvFee = "0";

            input.billDetails = new BillDetailsPay();
            input.billDetails.billerId = BillerID;


            var labelParamMapping = GetPayLabelParamMapping(label, request);
            input.billDetails.customerParams = new List<CustomerParamPay>();
            input.custDetails = input.custDetails ?? new CustDetailsPay();
            input.custDetails.customerTags = input.custDetails.customerTags ?? new List<CustomerTagPay>();


            input.billDetails.customerParams.Add(new CustomerParamPay { name = request.Label, value = request.ConsumerNumber });

            foreach (var entry in labelParamMapping)
            {
                input.billDetails.customerParams.Add(new CustomerParamPay { name = entry.Key, value = entry.Value });
            }


            input.custDetails.customerTags.Add(new CustomerTagPay { name = "EMAIL", value = Email });
            input.custDetails.customerTags.Add(new CustomerTagPay { name = "AADHAAR", value = Aadhar });
            input.custDetails.customerTags.Add(new CustomerTagPay { name = "PAN", value = Pan });

            input.custDetails.mobileNo = request.MobileNo;

            input.paymentDetails = input.paymentDetails ?? new PaymentDetails();
            input.paymentDetails.paymentInfo = input.paymentDetails.paymentInfo ?? new List<PaymentInfo>();


            input.paymentDetails.paymentInfo.Add(new PaymentInfo { name = "AuthCode", value = "7894561258" });
            input.paymentDetails.paymentMode = "Prepaid Card";
            input.paymentDetails.quickPay = "Yes";
            input.paymentDetails.splitPay = "No";
            input.paymentDetails.offusPay = "Yes";

            input.bbpsPGName = "YesPay";
            input.bbpsPGResponse = input.bbpsPGResponse ?? new BbpsPGResponse();

            input.bbpsPGResponse.txRefNo = "AI909108602491";
            input.bbpsPGResponse.txnDateTime = "2019-04-17 09:40:56";

            string requestJSON = JsonConvert.SerializeObject(input);

            BBPSHttpProcessing httpProcessing = new BBPSHttpProcessing();


            NetworkResponse netwotk = httpProcessing.BBPSPOST(BBPSURL.BillPayURL, requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(StatusType.Ok))
            {
                billPayResponse.Status = StatusType.Process;
                billPayResponse.Message = netwotk.Response;
                return billPayResponse;
            }

            BillPayAPIResponse apiResponse = null;

            apiResponse = JsonConvert.DeserializeObject<BillPayAPIResponse>(netwotk.Response);
            if (apiResponse == null)
            {
                billPayResponse.Status = Message.FAILURE;
                billPayResponse.Message = apiResponse.response.responseReason;
                return billPayResponse;
            }
            if (apiResponse.status.EqualsIgnoreCase(StatusType.Failure))
            {
                billPayResponse.Status = StatusType.Failure;
                billPayResponse.Message = apiResponse.response.complianceReason;
                billPayResponse.ResponseCode = apiResponse.response.responseCode;
                return billPayResponse;
            }

            if (apiResponse.status.EqualsIgnoreCase(StatusType.Success))
            {
                billPayResponse.Status = StatusType.Success;
                billPayResponse.Message = apiResponse.response.responseReason;
                billPayResponse.TransactionDateTime = apiResponse.response.txnDateTime;
                billPayResponse.RequestID = apiResponse.response.refId;
                billPayResponse.TransactionID = apiResponse.response.txnReferenceId;

                return billPayResponse;
            }

            billPayResponse.Status = StatusType.Process;
            billPayResponse.Message = apiResponse.response.responseReason;
            billPayResponse.TransactionDateTime = apiResponse.response.txnDateTime;
            billPayResponse.RequestID = apiResponse.response.refId;
            billPayResponse.TransactionID = apiResponse.response.txnReferenceId;

            return billPayResponse;

        }

        public YesBankCore.Response.BBPSResponse.BillPayResponse StatusCheck(StatusCheckRequest request)
        {
            YesBankCore.Response.BBPSResponse.BillPayResponse statusCheckResponse = new YesBankCore.Response.BBPSResponse.BillPayResponse();
            YesBankCore.Request.BBPS.StatusCheckAPIRequest input = new StatusCheckAPIRequest();

            statusCheckResponse.Status = StatusType.Process;
            input.billerId = request.BillerId;
            input.txnReferenceId = request.TxnReferenceId;
            input.xchangeId = "402";


            string requestJSON = JsonConvert.SerializeObject(input);

            BBPSHttpProcessing httpProcessing = new BBPSHttpProcessing();

            //NetworkResponse netwotk = httpProcessing.BBPSPOST(BBPSURL.StatusCheckURL, encryptedRequestJSON, Auth, BBPSCredential.Key);
            NetworkResponse netwotk = httpProcessing.BBPSPOST(BBPSURL.StatusCheckURL, requestJSON);
            if (!netwotk.Status.EqualsIgnoreCase(StatusType.Ok))
            {
                statusCheckResponse.Status = StatusType.Process;
                statusCheckResponse.Message = netwotk.Response;
                return statusCheckResponse;
            }

            StatusCheckAPIResponse apiResponse = null;

            apiResponse = JsonConvert.DeserializeObject<StatusCheckAPIResponse>(netwotk.Response);
            if (apiResponse == null)
            {
                statusCheckResponse.Status = StatusType.Failure;
                statusCheckResponse.Message = apiResponse.response;
                return statusCheckResponse;
            }
            if (apiResponse.status.EqualsIgnoreCase(StatusType.Failure))
            {
                statusCheckResponse.Status = StatusType.Failure;
                return statusCheckResponse;
            }

            if (apiResponse.status.EqualsIgnoreCase(StatusType.Success))
            {
                statusCheckResponse.Status = StatusType.Success;
                statusCheckResponse.Message = apiResponse.response;
                /*statusCheckResponse.RequestID = apiResponse.response.refId;
                statusCheckResponse.TransactionID = apiResponse.response.txnReferenceId;*/
                return statusCheckResponse;
            }

            statusCheckResponse.Status = StatusType.Process;
            statusCheckResponse.Message = apiResponse.response;
            /*statusCheckResponse.RequestID = apiResponse.response.refId;
            statusCheckResponse.TransactionID = apiResponse.response.txnReferenceId;*/

            return statusCheckResponse;

        }

        public ComplaintResponse Complaint(ComplaintAPIRequest request)
        {
            YesBankCore.Response.BBPSResponse.ComplaintResponse complaintResponse = new YesBankCore.Response.BBPSResponse.ComplaintResponse();

            complaintResponse.Status = StatusType.Process;


            string requestJSON = JsonConvert.SerializeObject(request);

            string encrypt = new EncryptDecrypt().AesEncrypt(requestJSON, request.EncryptionKey);

            EncryptedRequest encryptedRequest = new EncryptedRequest { request = encrypt };
            string encryptedRequestJSON = JsonConvert.SerializeObject(encryptedRequest);

            string Auth = new EncryptDecrypt().AesEncrypt(requestJSON, request.EncryptionKey);

            BBPSHttpProcessing httpProcessing = new BBPSHttpProcessing();


            //NetworkResponse netwotk = httpProcessing.BBPSPOST(BBPSURL.ComplaintURL, encryptedRequestJSON, Auth, request.EncryptionKey);
            NetworkResponse netwotk = httpProcessing.BBPSPOST(BBPSURL.BillFetchURL, requestJSON);
            if (!netwotk.Status.EqualsIgnoreCase(StatusType.Ok))
            {
                complaintResponse.Status = StatusType.Process;
                complaintResponse.Message = netwotk.Response;
                return complaintResponse;
            }

            ComplaintAPIResponse apiResponse = null;

            apiResponse = JsonConvert.DeserializeObject<ComplaintAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                complaintResponse.Status = StatusType.Failure;
                return complaintResponse;
            }
            if (apiResponse.status.EqualsIgnoreCase(StatusType.Failure))
            {
                complaintResponse.Status = StatusType.Failure;
                return complaintResponse;
            }

            if (apiResponse.status.EqualsIgnoreCase(StatusType.Success))
            {
                complaintResponse.Status = StatusType.Success;
                complaintResponse.Message = apiResponse.response.txnStatusComplainResp.complaintStatus;
                complaintResponse.ComplaintId = apiResponse.response.txnStatusComplainResp.complaintId;
                return complaintResponse;
            }

            complaintResponse.Status = StatusType.Process;
            complaintResponse.Message = apiResponse.response.txnStatusComplainResp.complaintStatus;
            complaintResponse.ComplaintId = apiResponse.response.txnStatusComplainResp.complaintId;

            return complaintResponse;
        }

        public ComplaintStatusResponse ComplaintStatusCheck(ComplaintStatusAPIRequest request)
        {
            YesBankCore.Response.BBPSResponse.ComplaintStatusResponse complaintResponse = new YesBankCore.Response.BBPSResponse.ComplaintStatusResponse();

            complaintResponse.Status = StatusType.Process;


            string requestJSON = JsonConvert.SerializeObject(request);

            string encrypt = new EncryptDecrypt().AesEncrypt(requestJSON, request.EncryptionKey);

            EncryptedRequest encryptedRequest = new EncryptedRequest { request = encrypt };
            string encryptedRequestJSON = JsonConvert.SerializeObject(encryptedRequest);

            string Auth = new EncryptDecrypt().AesEncrypt(requestJSON, request.EncryptionKey);

            BBPSHttpProcessing httpProcessing = new BBPSHttpProcessing();


            //NetworkResponse netwotk = httpProcessing.BBPSPOST(BBPSURL.ComplaintURL, encryptedRequestJSON, Auth, request.EncryptionKey);
            NetworkResponse netwotk = httpProcessing.BBPSPOST(BBPSURL.BillFetchURL, requestJSON);
            if (!netwotk.Status.EqualsIgnoreCase(StatusType.Ok))
            {
                complaintResponse.Status = StatusType.Process;
                complaintResponse.Message = netwotk.Response;
                return complaintResponse;
            }

            ComplaintStatusAPIResponse apiResponse = null;

            apiResponse = JsonConvert.DeserializeObject<ComplaintStatusAPIResponse>(netwotk.Response);
            if (apiResponse == null)
            {
                complaintResponse.Status = StatusType.Failure;
                return complaintResponse;
            }
            if (apiResponse.status.EqualsIgnoreCase(StatusType.Failure))
            {
                complaintResponse.Status = StatusType.Failure;
                return complaintResponse;
            }

            if (apiResponse.status.EqualsIgnoreCase(StatusType.Success))
            {
                complaintResponse.Status = StatusType.Success;
                complaintResponse.Message = apiResponse.response.txnStatusComplainResp.complaintStatus;
                return complaintResponse;
            }

            complaintResponse.Status = StatusType.Process;
            complaintResponse.Message = apiResponse.response.txnStatusComplainResp.complaintStatus;

            return complaintResponse;
        }

        public string GetMacAddress()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    var macAddress = networkInterface.GetPhysicalAddress().ToString();
                    return macAddress;
                }
            }
            return string.Empty;
        }

        private Dictionary<string, string> GetFetchLabelParamMapping(ParamLabelViewModel label, BillFetchRequest request)
        {
            var labelParamMapping = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(label.Label1) && !string.IsNullOrEmpty(request.param1))
                labelParamMapping[label.Label1] = request.param1;
            if (!string.IsNullOrEmpty(label.Label2) && !string.IsNullOrEmpty(request.param2))
                labelParamMapping[label.Label2] = request.param2;
            if (!string.IsNullOrEmpty(label.Label3) && !string.IsNullOrEmpty(request.param3))
                labelParamMapping[label.Label3] = request.param3;
            if (!string.IsNullOrEmpty(label.Label4) && !string.IsNullOrEmpty(request.param4))
                labelParamMapping[label.Label4] = request.param4;
            if (!string.IsNullOrEmpty(label.Label5) && !string.IsNullOrEmpty(request.param5))
                labelParamMapping[label.Label5] = request.param5;

            return labelParamMapping;
        }

        private Dictionary<string, string> GetPayLabelParamMapping(ParamLabelViewModel label, BillPayRequest request)
        {
            var labelParamMapping = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(label.Label1) && !string.IsNullOrEmpty(request.param1))
                labelParamMapping[label.Label1] = request.param1;
            if (!string.IsNullOrEmpty(label.Label2) && !string.IsNullOrEmpty(request.param2))
                labelParamMapping[label.Label2] = request.param2;
            if (!string.IsNullOrEmpty(label.Label3) && !string.IsNullOrEmpty(request.param3))
                labelParamMapping[label.Label3] = request.param3;
            if (!string.IsNullOrEmpty(label.Label4) && !string.IsNullOrEmpty(request.param4))
                labelParamMapping[label.Label4] = request.param4;
            if (!string.IsNullOrEmpty(label.Label5) && !string.IsNullOrEmpty(request.param5))
                labelParamMapping[label.Label5] = request.param5;

            return labelParamMapping;
        }
    }
}
