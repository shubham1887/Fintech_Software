using System.Collections.Generic;
using System.Linq;

namespace Project.StaticModel
{
    public class DMTCode
    {
        //DMT Providers
        public static readonly string RoundPay = "RoundPay";

        public static readonly string CommonDMTAsPayout = "CommonDMTAsPayout";
        public static readonly string Novity = "Novity";
        public static readonly string Novity5L = "Novity5L";
        public static readonly string ClareInfoTech = "ClareInfoTech";

        public static readonly string PDMT = "PDMT";
        public static readonly string IDMT = "IDMT";
        public static readonly string MROA = "MROA";

        //   public static readonly string MasterMoneyDMT = "MasterMoneyDMT";
        public static readonly string ePocketDMT = "ePocketDMT";

        public static readonly string PaytmCCFDMT = "PaytmCCFDMT";
        public static readonly string OpenBankDMT = "OpenBankDMT";
        public static readonly string MinksDMT = "MinksDMT";
        public static readonly string ICashDMT = "ICashDMT";
        public static readonly string ICICI_Composite = "ICICI_Composite";
        public static readonly string Mahagram = "Mahagram";
        public static readonly string ICICI_CIB = "ICICI_CIB";

        //    public static readonly string Cyberplate = "Cyberplate";
        public static readonly string Eko = "Eko";

        public static readonly string Ambika = "Ambika";
        public static readonly string PayU = "PayU";
        public static readonly string Paysprint = "Paysprint";
        public static readonly string MyNetLinkDMT = "MyNetLinkDMT";
        public static readonly string IMoneyPay = "IMoneyPay";
        public static readonly string ZPlusDMT = "ZPlusDMT";
        public static readonly string PayOneMoneyDMT = "PayOneMoneyDMT";
        public static readonly string JaiJuiDMT = "JaiJuiDMT";
        public static readonly string CashFree = "CashFree";
        public static readonly string EzyPay = "EzyPay";
        public static readonly string EzyPayPaytm = "EzyPayPaytm";
        public static readonly string PrimePayDMT = "PrimePayDMT";
        public static readonly string ZPlusPayout = "ZPlusPayout";
        public static readonly string HyptoDMT = "HyptoDMT";
        public static readonly string Razorpay = "Razorpay";
        public static readonly string CanaraBank = "CanaraBank";
        public static readonly string RoyalPayDMT = "RoyalPayDMT";
        public static readonly string MasterMoneyPureDMT = "MasterMoneyPureDMT";
        public static readonly string InstantPayPayout = "InstantPayPayout";
        public static readonly string Paisarewards = "Paisarewards";
        public static readonly string DikshuPay = "DikshuPay";
        public static readonly string EaseBuzz = "EaseBuzz";
        public static readonly string Shantilex = "Shantilex";
        public static readonly string SafexPayDMT = "SafexPayDMT";
        public static readonly string FidyPay = "FidyPay";
        public static readonly string RMGPayDMT = "RMGPayDMT";
        public static readonly string RMGPay5LDMT = "RMGPay5LDMT";
        public static readonly string IServeUDMT = "IServeUDMT";
        public static readonly string IServeULiteDMT = "IServeULiteDMT";
        public static readonly string QuadrantIndiaDMT = "QuadrantIndiaDMT";
        public static readonly string PayInDMT = "PayInDMT";
        public static readonly string YesBankDMT = "YesBankDMT";
        public static readonly string MottoPeDMT = "MottoPeDMT";
        public static readonly string ThoshaniBankDMT = "ThoshaniBankDMT";
        public static readonly string VastWebDMT = "VastWebDMT";
        public static readonly string EkoPayoutDMT = "EkoPayoutDMT";
        public static readonly string BranchxDMT = "BranchxDMT";
        public static readonly string VSmartDMT = "VSmartDMT";
        public static readonly string RoundPayDMT = "RoundPayDMT";
        public static readonly string IMPSGuruDMT = "IMPSGuruDMT";
        public static readonly string ZwitchDMT = "ZwitchDMT";
        public static readonly string KinsenDMT = "KinsenDMT";
        public static readonly string PaypaisaDMT = "PaypaisaDMT";
        public static readonly string OnlyAPIDMT = "OnlyAPIDMT";
        public static readonly string OnePaisaDMT = "OnePaisaDMT";
        public static readonly string WaayuPayDMT = "WaayuPayDMT";
        public static readonly string JusPayDMT = "JusPayDMT";
        public static readonly string LevinDMT = "LevinDMT";
        public static readonly string GoindiapayDMT = "GoindiapayDMT";
        public static readonly string EZYPayV1DMT = "EZYPayV1DMT";

        public static readonly Dictionary<string, int> DMTProviderIds = new Dictionary<string, int>
        {
            { MROA, Company.MROAProviderID },
           // { IDMT, Company.InstantPayProviderID },
            { PaytmCCFDMT, Company.PaytmCCFProviderID },
            { ePocketDMT, Company.EpocketProviderID },
            { PayU, Company.PayUProviderID },
            { JaiJuiDMT, Company.JaiJuiProviderID },
            { IMoneyPay, Company.IMoneyPayProviderID },
            { OpenBankDMT, Company.OpenBankProviderID },
            { PrimePayDMT, Company.PrimePayProviderID },
            { ZPlusPayout, Company.ZplusPayoutProviderID },
            { CashFree, Company.CashFreeProviderID },
            { HyptoDMT, Company.HyptoProviderID },
            { Razorpay, Company.RazorPayProviderID },
            { CanaraBank, Company.CanaraBankProviderID },
            { InstantPayPayout, Company.InstantPayPayoutProviderID },
            { PDMT, Company.PaytmProviderID },
            { Mahagram, Company.MahagramProviderID },
            { Eko, Company.EkoProviderID },
            { MinksDMT, Company.MinksProviderID },
            { ICashDMT, Company.ICashProviderID },
            { Ambika, Company.AmbikaProviderID },
            { Paysprint, Company.PaysprintProviderID },
            { MyNetLinkDMT, Company.MyNetLinkProviderID },
            { ZPlusDMT, Company.ZplusProviderID },
            { PayOneMoneyDMT, Company.PayOneMoneyProviderID },
            { RoyalPayDMT, Company.RoyalPayProviderID },
            { ICICI_Composite, Company.ICICIProviderID },
            { EzyPay, Company.EzyPayProviderID },
            { EzyPayPaytm, Company.EzyPayPaytmProviderID },
            { MasterMoneyPureDMT, Company.MasterMoneyPureDMTProviderID },
            { Paisarewards, Company.PaisarewardsProviderID },
            { DikshuPay, Company.DikshupayProviderID },
            { EaseBuzz, Company.EaseBuzzProviderID },
            { Shantilex, Company.ShantilexProviderID },
            { SafexPayDMT, Company.SafexPayProviderID },
            { FidyPay, Company.FidyPayProviderID },
            { RMGPayDMT, Company.RMGPayProviderID },
            { RMGPay5LDMT, Company.RMGPay5LProviderID },
            { ClareInfoTech, Company.ClareInfoTechProviderID },
            { IServeUDMT, Company.IServeUProviderID },
            { IServeULiteDMT, Company.IServeULiteProviderID },
            { QuadrantIndiaDMT, Company.QuadrantIndiaProviderID },
            { PayInDMT, Company.PayInProviderID },
            { YesBankDMT, Company.YesBankProviderID },
            { MottoPeDMT, Company.MottoPeProviderID },
            { ThoshaniBankDMT, Company.ToshaniBankProviderID },
            { VastWebDMT, Company.VastWebProviderID },
            { EkoPayoutDMT, Company.EkoPayoutProviderID },
            { BranchxDMT, Company.BranchxProviderID },
            { VSmartDMT, Company.VSmartProviderID },
            { RoundPayDMT, Company.RoundPayProviderID },
            { IMPSGuruDMT, Company.IMPSGuruProviderID },
            { ZwitchDMT, Company.ZwitchProviderID },
            { KinsenDMT, Company.KinsenProviderID },
            { PaypaisaDMT, Company.PayPaisaProviderID },
            { OnlyAPIDMT, Company.OnlyAPIProviderID },
            { OnePaisaDMT, Company.OnePaisaProviderID },
            { WaayuPayDMT, Company.WaayuPayProviderID },
            { JusPayDMT, Company.JusPayProviderID },
            { LevinDMT, Company.LevinProviderID },
            { GoindiapayDMT, Company.GoindiapayProviderID },
            { ICICI_CIB, Company.ICICICIBProviderID },
            { EZYPayV1DMT, Company.EZYPayV1ProviderID },
        };

        public static string getNameByID(int ProviderID)
        {
            return DMTProviderIds.FirstOrDefault(pair => pair.Value == ProviderID).Key ?? "";
        }

        public static int getIDByName(string Name)
        {
            return DMTProviderIds.FirstOrDefault(pair => pair.Key.ToLower() == Name.ToLower()).Value;
        }

        public static readonly Dictionary<string, int> DMTAsPayoutProviderIds = new Dictionary<string, int>
        {
            { ICICI_Composite, Company.ICICIProviderID },
            { PayU, Company.PayUProviderID },
            { JaiJuiDMT, Company.JaiJuiProviderID },
            { IMoneyPay, Company.IMoneyPayProviderID },
            { OpenBankDMT, Company.OpenBankProviderID },
            { PrimePayDMT, Company.PrimePayProviderID },
            { ZPlusPayout, Company.ZplusPayoutProviderID },
            { CashFree, Company.CashFreeProviderID },
            { HyptoDMT, Company.HyptoProviderID },
            { Razorpay, Company.RazorPayProviderID },
            { CanaraBank, Company.CanaraBankProviderID },
            { InstantPayPayout, Company.InstantPayPayoutProviderID },
            { MyNetLinkDMT, Company.MyNetLinkProviderID },
            { Paisarewards, Company.PaisarewardsProviderID },
            { DikshuPay, Company.DikshupayProviderID },
            { EaseBuzz, Company.EaseBuzzProviderID },
            { SafexPayDMT, Company.SafexPayProviderID },
            { FidyPay, Company.FidyPayProviderID },
            { IServeUDMT, Company.IServeUProviderID },
            { IServeULiteDMT, Company.IServeULiteProviderID },
            { QuadrantIndiaDMT, Company.QuadrantIndiaProviderID },
            { MottoPeDMT, Company.MottoPeProviderID },
            { ThoshaniBankDMT, Company.ToshaniBankProviderID },
            { EkoPayoutDMT, Company.EkoPayoutProviderID },
            { BranchxDMT, Company.BranchxProviderID },
            { VSmartDMT, Company.VSmartProviderID },
            { RoundPayDMT, Company.RoundPayProviderID },
            { IMPSGuruDMT, Company.IMPSGuruProviderID },
            { ZwitchDMT, Company.ZwitchProviderID },
            { KinsenDMT, Company.KinsenProviderID },
            { PaypaisaDMT, Company.PayPaisaProviderID },
            { OnlyAPIDMT, Company.OnlyAPIProviderID },
            { OnePaisaDMT, Company.OnePaisaProviderID },
            { WaayuPayDMT, Company.WaayuPayProviderID },
            { JusPayDMT, Company.JusPayProviderID },
            { GoindiapayDMT, Company.GoindiapayProviderID },
            { ICICI_CIB, Company.ICICICIBProviderID },
        };

        public static int getIDByNameDMTAsPayoutProvider(string Name)
        {
            return DMTAsPayoutProviderIds.FirstOrDefault(pair => pair.Key.ToLower() == Name.ToLower()).Value;
        }

        public static int getDMTAsProviderID(int ProviderID)
        {
            return DMTAsPayoutProviderIds.FirstOrDefault(pair => pair.Value == ProviderID).Value;
        }


        // note : if provider returns status in web-hook, then only add below
        public static readonly Dictionary<string, int> WebhookProviderIds = new Dictionary<string, int>
        {
            { PayInDMT, Company.PaytmProviderID },
            { Razorpay, Company.RazorPayProviderID },
            { OnePaisaDMT, Company.OnePaisaProviderID },
            { RoyalPayDMT, Company.RoyalPayProviderID },
            { PayOneMoneyDMT, Company.PayOneMoneyProviderID },
            { RMGPayDMT, Company.RMGPayProviderID },
            { RMGPay5LDMT, Company.RMGPay5LProviderID },
            { InstantPayPayout, Company.InstantPayPayoutProviderID },
             {SafexPayDMT, Company.SafexPayProviderID },
            { PayU, Company.PayUProviderID },
             { WaayuPayDMT, Company.WaayuPayProviderID },
              { ThoshaniBankDMT, Company.ToshaniBankProviderID }
        };

        public static int getWebhookProviderID(int ProviderID)
        {
            return WebhookProviderIds.FirstOrDefault(pair => pair.Value == ProviderID).Value;
        }
    }
}