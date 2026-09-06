using System;
using System.Collections.Generic;

namespace Project.StaticModel
{
    public class GatewayCode
    {
        public static readonly string Razorpay = "Razorpay";
        public static readonly string Paytm = "Paytm";
        public static readonly string InstantPay = "InstantPay";
        public static readonly string UPIGateway = "UPIGateway";
        public static readonly string IPG = "IPG";
        public static readonly string ICICIQR = "ICICI_QR";
        public static readonly string FreeUPI = "FreeUPI";
        public static readonly string OpenUPI = "OpenUPI";
        public static readonly string OpenBank = "OpenBank";
        public static readonly string IMoneyPay = "IMoneyPay";
        public static readonly string Easebuzz = "Easebuzz";
        public static readonly string CashFree = "CashFree";
        public static readonly string UPIALL = "UPIALL";
        public static readonly string Paysprint = "Paysprint";
        public static readonly string PayGoal = "PayGoal";
        public static readonly string RazorpayQR = "RazorpayQR";
        public static readonly string YesBank = "YesBank";
        public static readonly string PhonePay = "PhonePay";
        public static readonly string WorldLine = "WorldLine";
        public static readonly string EasebuzzStaticQR = "EasebuzzStaticQR";
        public static readonly string PayU = "PayU";
        


        public static readonly string PaytmInitURL = "https://securegw.paytm.in/theia/api/v1/initiateTransaction?mid=[mid]&orderId=[orderid]";
        //public static string PaytmInitURL = "https://securegw-stage.paytm.in/theia/api/v1/initiateTransaction?mid=[mid]&orderId=[orderid]";

        public static readonly HashSet<string> validGatewayNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
         {
             GatewayCode.Razorpay,
             GatewayCode.Paytm,
             GatewayCode.UPIGateway,
             GatewayCode.UPIALL,
             GatewayCode.ICICIQR,
             GatewayCode.OpenUPI,
             GatewayCode.OpenBank,
             GatewayCode.IMoneyPay,
             GatewayCode.PayGoal,
             GatewayCode.FreeUPI,
             GatewayCode.CashFree,
             GatewayCode.Easebuzz,
             GatewayCode.Paysprint,
             GatewayCode.RazorpayQR,
             GatewayCode.YesBank,
             GatewayCode.PhonePay,
             GatewayCode.WorldLine,
             GatewayCode.EasebuzzStaticQR,
             GatewayCode.PayU,
         };
    }

    public enum PaysprintUPICode
    {
        DynamicQR = 1,
        StaticQR = 2,
    }
}