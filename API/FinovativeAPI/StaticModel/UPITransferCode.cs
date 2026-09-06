using System.Collections.Generic;
using System.Linq;

namespace Project.StaticModel
{
    public class UPITransferCode
    {
        public static readonly string CashFree = "CashFree";
        public static readonly string MyNetLinkUPI = "NetLinkUPI";
        public static readonly string ClickPayUPI = "ClickPayUPI";
        public static readonly string RazorPayUPI = "RazorPay";
        public static readonly string InstantPay = "InstantPay";
        public static readonly string ICICI_CompisiteUPI = "ICICI_Compisite";
        public static readonly string APIClubUPI = "APIClubUPI";
        public static readonly string JaiJuiUPI = "JaiJuiUPI";
        public static readonly string EaseBuzz = "EaseBuzz";
        public static readonly string DikshuPay = "DikshuPay";
        public static readonly string QuadrantIndia = "QuadrantIndia";
        public static readonly string ToshaniBank = "ToshaniBank";
        public static readonly string RoundPayUPI = "RoundPayUPI";
        public static readonly string OnePaisaUPI = "OnePaisaUPI";
        public static readonly string ZwitchUPI = "ZwitchUPI";
        public static readonly string IMPSGuruUPI = "IMPSGuruUPI";

        public static readonly Dictionary<string, int> ProviderIds = new Dictionary<string, int>
        {
            { CashFree, Company.CashFreeProviderID },
            { APIClubUPI, Company.ApiClubProviderID },
            { MyNetLinkUPI, Company.MyNetLinkProviderID },
            { ClickPayUPI, Company.ClickPayProviderID },
            { RazorPayUPI, Company.RazorPayProviderID },
            { ICICI_CompisiteUPI, Company.ICICIProviderID },
            { InstantPay, Company.InstantPayPayoutProviderID },
            { JaiJuiUPI, Company.JaiJuiProviderID },
            { EaseBuzz, Company.EaseBuzzProviderID },
            { DikshuPay, Company.DikshupayProviderID },
            { QuadrantIndia, Company.QuadrantIndiaProviderID },
            { ToshaniBank, Company.ToshaniBankProviderID },
            { RoundPayUPI, Company.RoundPayProviderID },
            { OnePaisaUPI, Company.OnePaisaProviderID },
            { ZwitchUPI, Company.ZwitchProviderID },
            { IMPSGuruUPI, Company.IMPSGuruProviderID },
        };

        public static string getNameByID(int ProviderID)
        {
            return ProviderIds.FirstOrDefault(pair => pair.Value == ProviderID).Key ?? "";
        }

        public static int getIDByName(string Name)
        {
            return ProviderIds.FirstOrDefault(pair => pair.Key.ToLower() == Name.ToLower()).Value;
        }
    }
}