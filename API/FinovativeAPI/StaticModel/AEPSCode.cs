using System.Collections.Generic;
using System.Linq;

namespace Project.StaticModel
{
    public class AEPSCode
    {
        public static readonly string Mahagram = "Mahagram";
        public static readonly string PaySprint = "PaySprint";
        public static readonly string PaysprintPaytm = "PaysprintPaytm";
        public static readonly string InstantPayWebSDK = "InstantPayWebSDK";
        public static readonly string MosambeeMF919 = "MosambeeMF919";
        public static readonly string Paytm = "Paytm";
        public static readonly string Alle = "Alle";
        public static readonly string Eko = "Eko";
        public static readonly string Fingpay = "Fingpay";
        public static readonly string ApiClub = "ApiClub";
        public static readonly string EzyPay = "EzyPay";
        public static readonly string CredoPay = "CredoPay";
        public static string RapiPay = "RapiPay";
        public static string InstantPay = "InstantPay";
        public static string YesBank = "YesBank";

        public static readonly Dictionary<string, int> ProviderIds = new Dictionary<string, int>
        {
            { Mahagram, Company.MahagramProviderID },
            { PaySprint, Company.PaysprintProviderID },
            { PaysprintPaytm, Company.PaysprintPaytmAEPSProviderID },
            { InstantPayWebSDK, Company.InstantPayPayoutProviderID },
            { MosambeeMF919, Company.MosambeeProviderID },
            { Paytm, Company.PaytmProviderID },
            { Alle, Company.AlleProviderID },
            { Eko, Company.EkoProviderID },
            { Fingpay, Company.FingpayProviderID },
            { ApiClub, Company.ApiClubProviderID },
            { EzyPay, Company.EzyPayProviderID },
            { CredoPay, Company.CredoPayProviderID },
            { RapiPay, Company.RapiPayProviderID },
            { InstantPay, Company.InstantPayPayoutProviderID },
            { YesBank, Company.YesBankProviderID },
        };

        public static string getNameByID(int ProviderID)
        {
            return ProviderIds.FirstOrDefault(pair => pair.Value == ProviderID).Key ?? "";
        }

    }
}