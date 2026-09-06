using System.Collections.Generic;
using System.Linq;

namespace Project.StaticModel
{
    public class MicroATMCode
    {
        public static readonly string PaySwiff = "PaySwiff";

        public static readonly Dictionary<string, int> ProviderIds = new Dictionary<string, int>
        {
            { PaySwiff, Company.PaySwiffProviderID },
        };

        public static string getNameByProviderID(int providerId)
        {
            return ProviderIds.FirstOrDefault(pair => pair.Value == providerId).Key ?? "";
        }
    }
}