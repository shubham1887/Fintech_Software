using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.StaticModel
{
    public class VendorPayCode
    {
        public static readonly string Paisarewards = "Paisarewards";
        public static readonly string MyNetLink = "MyNetLink";
        public static readonly string CashFree = "CashFree";
        public static readonly string IServeU = "IServeU";
        public static readonly string YesBank = "YesBank";
        public static readonly string ToshaniBank = "ToshaniBank";
        public static readonly string Branchx = "Branchx";
        public static readonly string CanaraBank = "CanaraBank";
        public static readonly string EKO = "EKO";
        public static readonly string VSmart = "VSmart";
        public static readonly string JusPay = "JusPay";
        public static readonly string ICICI_CIB = "ICICI_CIB";

        public static readonly Dictionary<string, int> providerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
           {
               { Paisarewards, Company.PaisarewardsProviderID },
               { MyNetLink, Company.MyNetLinkProviderID },
               { CashFree, Company.CashFreeProviderID },
               { IServeU, Company.IServeUProviderID },
               { YesBank, Company.YesBankProviderID },
               { ToshaniBank, Company.ToshaniBankProviderID },
               { Branchx, Company.BranchxProviderID },
               { CanaraBank, Company.CanaraBankProviderID },
               { EKO, Company.EkoPayoutProviderID },
               { VSmart, Company.VSmartProviderID },
               { JusPay, Company.JusPayProviderID },
               { ICICI_CIB, Company.ICICICIBProviderID },
          };

        public static int getIDByName(string Name)
        {
            return providerMap.FirstOrDefault(pair => pair.Key.ToLower() == Name.ToLower()).Value;
        }

        public static string getNameByID(int ProviderID)
        {
            return providerMap.FirstOrDefault(pair => pair.Value == ProviderID).Key ?? "";
        }
    }
}