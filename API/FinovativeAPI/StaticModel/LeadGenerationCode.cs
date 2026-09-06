using System.Collections.Generic;
using System.Linq;

namespace Project.StaticModel
{
    public class LeadGenerationCode
    {
        public static readonly string Paysprint = "Paysprint";
        public static readonly string Onlyapi = "Onlyapi";

        public static readonly Dictionary<string, int> ProviderIds = new Dictionary<string, int>
        {
            { Paysprint, Company.PaysprintProviderID },
            { Onlyapi, Company.OnlyAPIProviderID },
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