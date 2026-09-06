using MplanCore.Controller;
using MplanCore.Model;
using MplanCore.Request.MobiKwik;
using MplanCore.Response;
using MplanCore.Response.MobiKwik;
using MplanCore.StaticModal;
using Newtonsoft.Json;

namespace MplanCore.Processing
{
    public class MobiKwikProcessing
    {
        public static Records SelectPlan(PlanViewRequest request,string Operator)
        {
            Records record = new Records
            {
                Message = Message.PlanNotAvailable
            };
             Operator = GetMobileOperatorName(Operator);
            string URL = MobiKwikURL.PlanFetchingGET;
            URL = URL.Replace("[OperatorID]", (Operator));
            URL = URL.Replace("[CircleID]", Util.MaharashtraCircleID); 

            PlanViewAPIResponse Response = new PlanViewAPIResponse();
            NetworkResponse network = null;
            try
            {
                network = HttpProcessing.CallGetMethodForMobiKwik(URL, request.XMClient=Util.XMClient);

                if (!new Validation().IsValidString(network.Response))
                {
                    return record;
                }

                if (network.Response.ToUpper().Contains(Message.PlanNotAvailable.ToUpper()))
                {
                    return record;
                }

                Response = JsonConvert.DeserializeObject<PlanViewAPIResponse>(network.Response);
                record.Message = "Ok";
                int i = 0;

                if (Response.data?.plans!= null)
                {
                    var COMBO = new The2G[Response.data.plans.Count];

                    foreach (var item in Response.data.plans)
                    {
                        COMBO[i] = new The2G();
                        COMBO[i].Desc = item.planDescription;
                        //COMBO[i].LastUpdate = item.last_update;
                        COMBO[i].Rs = item.amount;
                        COMBO[i].Validity = item.validity;
                        record.COMBO = COMBO;
                        i++;
                    }
                }

                record.Message = Response.message?.text;
            }
            catch (Exception)
            {
                return record;
            }

            return record;
        }

        private static string GetMobileOperatorName(string Operator)
        {
            string @operator = string.Empty;
            if (Operator.Trim().ToUpper().Contains("Airtel".Trim().ToUpper()))
                @operator = "1";
            else if (Operator.Trim().ToUpper().Contains("BSNL".Trim().ToUpper()))
                @operator = "3";
            else if (Operator.Trim().ToUpper().Contains("MTNL".Trim().ToUpper()))
                @operator = "7";
            else if (Operator.Trim().ToUpper().Contains("Jio".Trim().ToUpper()))
                @operator = "140";
            else if (Operator.Trim().ToUpper().Contains("Vi".Trim().ToUpper()))
                @operator = "338";

            return @operator;
        }

    }
}
