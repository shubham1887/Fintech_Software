using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MplanCore.StaticModal
{
    public class CircleApiCredential
    {
        public static string ApiKey = "";
        public static string Password = "";

        internal const string CircleApiURL = "http://planapi.in/api/Mobile/OperatorFetch?apimember_id=[apimember_id]&api_password=[api_password]&Mobileno=[Number]";

        public static void SetCredentials(string key, string password)
        {
            ApiKey = key;
            Password = password;
        }
    }
}
