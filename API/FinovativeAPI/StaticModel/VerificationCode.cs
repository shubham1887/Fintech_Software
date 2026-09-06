using System.Collections.Generic;
using System.Linq;

namespace Project.StaticModel
{
    public class VerificationCode
    {
        public static readonly string CashFree = "CashFree";

        public static readonly Dictionary<string, int> VerifyProviderIds = new Dictionary<string, int>
        {
            { CashFree, Company.CashFreeProviderID }
       
        };

        public static string getNameByID(int ProviderID)
        {
            return VerifyProviderIds.FirstOrDefault(pair => pair.Value == ProviderID).Key ?? "";
        }

        public static int getIDByName(string Name)
        {
            return VerifyProviderIds.FirstOrDefault(pair => pair.Key.ToLower() == Name.ToLower()).Value;
        }

    }
}