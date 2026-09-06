using Newtonsoft.Json;
using Project.DAL;
using Project.Model;
using Project.Response.PaymentGateway;
using Project.StaticModel;
using System;
using YESBankCore.Request;
using YESBankCore.Response;

namespace Project.Processing.YesBank
{
    public class PaymentGatewayProcessing
    {
        private YESBankCore.YesBank mt = new YESBankCore.YesBank();

        public PaymentGatewayProcessing()
        {
            ViewModel viewModel = null;
            try
            {
                viewModel = new PaymentGatewaySettingDal().SelectPaymentGatewayByName(GatewayCode.YesBank);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            mt.SetCredential(viewModel.MerchantID, viewModel.Slat,
                viewModel.Token, viewModel.SuccessURL,
                viewModel.FailURL, viewModel.ActionURL);
        }

        internal YESBankCore.Response.QRInitResponse QRLogIntent(string OrderID, string Amount, int UserID)
        {
            YESBankCore.Request.QRPayRequest input = new YESBankCore.Request.QRPayRequest();
            input.OrderNo = OrderID;
            input.Amount = Convert.ToInt32(Amount) + ".00";

            QRInitResponse qrResponse = mt.QRInit(input);

            PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
            paymentGatewayLog.Amount = Convert.ToDecimal(Amount);
            paymentGatewayLog.UserID = UserID;
            paymentGatewayLog.Request = JsonConvert.SerializeObject(input);
            paymentGatewayLog.Response = JsonConvert.SerializeObject(qrResponse);
            paymentGatewayLog.Type = "init";
            paymentGatewayLog.OrderID = OrderID;
            paymentGatewayLog.GatewayName = GatewayCode.YesBank;
            new PaymentGatewayLogDAL().Insert(paymentGatewayLog);

            return qrResponse;
        }

        internal YESBankCore.Response.StatusCheckResponse TranactionStatus(string OrderID, decimal Amount, int UserID)
        {
            StatusCheckRequest input = new StatusCheckRequest();
            input.OrderNo = OrderID;

            YESBankCore.Response.StatusCheckResponse resp = mt.StatusCheck(input);

            PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
            paymentGatewayLog.Amount = Amount;
            paymentGatewayLog.UserID = UserID;
            paymentGatewayLog.Request = JsonConvert.SerializeObject(input);
            paymentGatewayLog.Response = JsonConvert.SerializeObject(resp);
            paymentGatewayLog.Type = "StatusCheck";
            paymentGatewayLog.OrderID = OrderID;
            paymentGatewayLog.GatewayName = GatewayCode.YesBank;
            new PaymentGatewayLogDAL().Insert(paymentGatewayLog);

            return resp;
        }

        internal YESBankCore.Response.StatusCheckResponse CallbackResponse(string data)
        {
            YESBankCore.Response.StatusCheckResponse resp = mt.Callback(data);

            PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
            paymentGatewayLog.Amount = 0;
            paymentGatewayLog.UserID = Company.AdminUserID;
            paymentGatewayLog.Request = data;
            paymentGatewayLog.Response = JsonConvert.SerializeObject(resp);
            paymentGatewayLog.Type = UserResponseType.Callback;
            paymentGatewayLog.OrderID = resp.OrderNo;
            paymentGatewayLog.GatewayName = GatewayCode.YesBank;
            new PaymentGatewayLogDAL().Insert(paymentGatewayLog);

            return resp;
        }
    }
}