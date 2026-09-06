using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.StaticModel
{
    public class PayoutCode
    {
        public static readonly string Alle = "Alle";
        public static readonly string RazorPay = "RazorPay";
        public static readonly string Paytm = "Paytm";
        public static readonly string ICICI_Composite = "ICICI_Composite";
        public static readonly string InstantPay = "InstantPay";
        public static readonly string ApiClub = "ApiClub";
        public static readonly string PayU = "PayU";
        public static readonly string Paysprint = "Paysprint";
        public static readonly string IMoneyPay = "IMoneyPay";
        public static readonly string OpenBank = "OpenBank";
        public static readonly string JaiJui = "JaiJui";
        public static readonly string CashFree = "CashFree";
        public static readonly string PrimePay = "PrimePay";
        public static readonly string ZPlusPayout = "ZPlusPayout";
        public static readonly string HyptoPayout = "HyptoPayout";
        public static readonly string EaseBuzz = "EaseBuzz";
        public static readonly string Branchx = "Branchx";
        public static readonly string Paisarewards = "Paisarewards";
        public static readonly string Dikshupay = "Dikshupay";
        public static readonly string SafexPay = "SafexPay";
        public static readonly string Fidypay = "FidyPay";
        public static readonly string CanaraBank = "CanaraBank";
        public static readonly string EzyPay = "EzyPay";
        public static readonly string MyNetLink = "MyNetLink";
        public static readonly string IServeU = "IServeU";
        public static readonly string YesBankPayout = "YesBankPayout";
        public static readonly string ToshaniBankPayout = "ToshaniBankPayout";
        public static readonly string EkoPayout = "EkoPayout";
        public static readonly string VSmartPayout = "VSmartPayout";
        public static readonly string ZwitchPayout = "ZwitchPayout";
        public static readonly string OnePaisaPayout = "OnePaisaPayout";
        public static readonly string JusPayPayout = "JusPayPayout";
        public static readonly string IMPSGuruPayout = "IMPSGuruPayout";
        public static readonly string ICICI_CIB = "ICICI_CIB";

        public static readonly Dictionary<string, int> providerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
           {
               {  Alle, Company.AlleProviderID },
               {  RazorPay, Company.RazorPayProviderID },
               {  Paytm, Company.PaytmProviderID },
               {  ICICI_Composite, Company.ICICIProviderID },
               {  InstantPay, Company.InstantPayPayoutProviderID },
               {  ApiClub, Company.ApiClubProviderID },
               {  PayU, Company.PayUProviderID },
               {  Paysprint, Company.PaysprintProviderID },
               {  IMoneyPay, Company.IMoneyPayProviderID },
               {  JaiJui, Company.JaiJuiProviderID },
               {  OpenBank, Company.OpenBankProviderID },
               {  CashFree, Company.CashFreeProviderID },
               {  PrimePay, Company.PrimePayProviderID },
               {  ZPlusPayout, Company.ZplusPayoutProviderID },
               {  HyptoPayout, Company.HyptoProviderID },
               {  EaseBuzz, Company.EaseBuzzProviderID },
               {  Branchx, Company.BranchxProviderID },
               {  Paisarewards, Company.PaisarewardsProviderID },
               {  Dikshupay, Company.DikshupayProviderID },
               {  SafexPay, Company.SafexPayProviderID },
               {  Fidypay, Company.FidyPayProviderID },
               {  CanaraBank, Company.CanaraBankProviderID },
               {  EzyPay, Company.EzyPayProviderID },
               {  MyNetLink, Company.MyNetLinkProviderID },
               {  IServeU, Company.IServeUProviderID },
               {  YesBankPayout, Company.YesBankProviderID },
               {  ToshaniBankPayout, Company.ToshaniBankProviderID },
               {  EkoPayout, Company.EkoPayoutProviderID },
               {  VSmartPayout, Company.VSmartProviderID },
               {  ZwitchPayout, Company.ZwitchProviderID },
               {  OnePaisaPayout, Company.OnePaisaProviderID },
               {  JusPayPayout, Company.JusPayProviderID },
               {  IMPSGuruPayout, Company.IMPSGuruProviderID },
               {  ICICI_CIB, Company.ICICICIBProviderID },
               
           };

        public static int getIDByName(string Name)
        {
            return providerMap.FirstOrDefault(pair => pair.Key.ToLower() == Name.ToLower()).Value;
        }
    }
}