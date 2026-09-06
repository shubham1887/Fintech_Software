using Newtonsoft.Json;
using Project.BAL;
using Project.Model;
using Project.Requests.Utility;
using Project.Response.ProviderResponse;
using Project.Response.Utility;
using YesBankCore;
using YesBankCore.Response.BBPSResponse;

namespace Project.Processing.YesBank
{
    public class BBPSProcessing
    {
        private YesBankBBPS mt = new YesBankBBPS();

        string Name = "YesBankBBPS";
        public BBPSProcessing()
        {
            BBPSSettings bbps = new BBPSSettingsBal().SelectBBPSByName(Name);

            string AgentID = bbps.AgentID;
            string PartnerID = bbps.PartnerID;
            string PrivateKey = bbps.Token;
            string PublicKey = bbps.MerchantID;

            mt.SetCredential(AgentID, PartnerID, PrivateKey, PublicKey);
        }

        public FetchViewModel BillFetch(BillFetchRequest request, UserMaster user, string IP, string MAC, Response.Utility.ParamLabelViewModel paramLabelViewModel,string BillerID)
        {
            FetchViewModel fetchViewModel = new FetchViewModel();

            YesBankCore.Request.BBPS.BillFetchRequest billFetchRequest = new YesBankCore.Request.BBPS.BillFetchRequest();
            YesBankCore.Response.BBPSResponse.ParamLabelViewModel paramLabel = new YesBankCore.Response.BBPSResponse.ParamLabelViewModel();

            billFetchRequest.ConsumerNumber = request.ConsumerNumber;
            billFetchRequest.param1 = request.Param1;
            billFetchRequest.param2 = request.Param2;
            billFetchRequest.param3 = request.Param3;
            billFetchRequest.param4 = request.Param4;
            billFetchRequest.param5 = request.Param5;
            billFetchRequest.Label = request.Label;
            billFetchRequest.MobileNo = request.MobileNo;

            paramLabel.Label1 = paramLabelViewModel.ParamLabel1;
            paramLabel.Label2 = paramLabelViewModel.ParamLabel2;
            paramLabel.Label3 = paramLabelViewModel.ParamLabel3;
            paramLabel.Label4 = paramLabelViewModel.ParamLabel4;
            paramLabel.Label5 = paramLabelViewModel.ParamLabel5;

            BillFetchResponse resp = mt.BillFetch(billFetchRequest, IP, MAC, user.EmailID, user.AadharNo, user.PANNo,user.MobileNumber,user.PersonName, paramLabel, BillerID);
            fetchViewModel.Message = resp.Message;
            fetchViewModel.CustomerName = resp.BillerName;
            fetchViewModel.BillDate = resp.BillDate;
            fetchViewModel.BillerID = resp.BillerID;
            fetchViewModel.DueDate = resp.BillDueDate;
            fetchViewModel.Amount = resp.Amount;
            fetchViewModel.BBPSViewID = resp.BillValidateId;
            return fetchViewModel;
        }

        public ProviderReturnResponse BillPay(TransactionMaster transactionMaster, string Label, string MAC, Response.Utility.ParamLabelViewModel paramLabelViewModel,string BillerID,UserMaster user)
        {
            ProviderReturnResponse payViewModel = new ProviderReturnResponse();

            YesBankCore.Request.BBPS.BillPayRequest billPayRequest = new YesBankCore.Request.BBPS.BillPayRequest();
            YesBankCore.Response.BBPSResponse.ParamLabelViewModel paramLabel = new YesBankCore.Response.BBPSResponse.ParamLabelViewModel();


            billPayRequest.ConsumerNumber = transactionMaster.ConsumerNumber;
            billPayRequest.param1 = transactionMaster.Bill_Optional1;
            billPayRequest.param2 = transactionMaster.Bill_Optional2;
            billPayRequest.param3 = transactionMaster.Bill_Optional3;
            billPayRequest.param4 = transactionMaster.Bill_Optional4;
            billPayRequest.param5 = transactionMaster.Bill_Optional5;
            billPayRequest.Label = Label;
            billPayRequest.MobileNo = transactionMaster.Bill_CustomerMessageMobileNo;

            paramLabel.Label1 = paramLabelViewModel.ParamLabel1;
            paramLabel.Label2 = paramLabelViewModel.ParamLabel2;
            paramLabel.Label3 = paramLabelViewModel.ParamLabel3;
            paramLabel.Label4 = paramLabelViewModel.ParamLabel4;
            paramLabel.Label5 = paramLabelViewModel.ParamLabel5;
            paramLabel.Label5 = paramLabelViewModel.ParamLabel5;

            BillPayResponse resp = mt.BillPay(billPayRequest, transactionMaster.IPAddress, transactionMaster.Amount.ToString(), transactionMaster.Bill_Optional6, MAC, paramLabel, BillerID, user.EmailID, user.AadharNo, user.PANNo, user.MobileNumber);
            payViewModel.Status = resp.Status;
            payViewModel.Message = resp.Message;
            payViewModel.TransactionID = resp.TransactionID;
            payViewModel.RequestID = resp.RequestID;
            payViewModel.ResponseParam2 = resp.TransactionDateTime;
            payViewModel.ResponseCode = resp.ResponseCode;

            Project.Processing.BBPSProcessings bbpsProcessing = new Project.Processing.BBPSProcessings();
            bbpsProcessing.UpdateYesBankBBPSStatus(resp, transactionMaster.TransactionMasterID, payViewModel);

            return payViewModel;
        }

        public BillPayResponse GetTransctionStatus(string ProviderRequestID, TransactionMaster transactionMaster, string BillerID)
        {
            YesBankCore.Request.BBPS.StatusCheckRequest input = new YesBankCore.Request.BBPS.StatusCheckRequest();
            input.TxnReferenceId = transactionMaster.ReferenceNo;
            input.BillerId = BillerID;
            BillPayResponse resp = mt.StatusCheck(input);

            string reqStr = JsonConvert.SerializeObject(input);
            string resStr = JsonConvert.SerializeObject(resp);
            new Logger().Info("MasterMoneyDMT ", "GetTransctionStatus", reqStr + "::" + resStr);
            return resp;
        }
    }
}
