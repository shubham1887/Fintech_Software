using MplanCore.Model;
using MplanCore.Processing;
using MplanCore.Request;
using MplanCore.Response;
using MplanCore.Response.ShighraPay;
using MplanCore.StaticModal;
using Newtonsoft.Json;

namespace MPlanCore.Processing
{
    public class ShighrapayAPIProcessing
    {
        public static OperatorResponse GetCircleCode(OperatorRequest input)
        {
            //if (string.IsNullOrEmpty(mobile))
            //{
            //    return "4";
            //}

            string requestJSON = JsonConvert.SerializeObject(input);
            HttpProcessing httpResponse = new HttpProcessing();

            NetworkResponse netwotk = httpResponse.POST(ShighraPayCredentials.OperatorSearchURL, requestJSON);
            OperatorResponse circleResponse = new OperatorResponse();

            try
            {
                circleResponse = JsonConvert.DeserializeObject<OperatorResponse>(netwotk.Response);
            }
            catch
            {
                return circleResponse;
            }

            if (circleResponse == null)
            {
                return circleResponse;
            }

            return circleResponse;
        }

        public static Records SelectPlan(RechargeRequest input)
        {
            Records record = new Records
            {
                Message = Message.PlanNotAvailable
            };

            string OperatorSearchrequestJSON = JsonConvert.SerializeObject(input);
            HttpProcessing httpResponse = new HttpProcessing();
            // circle hit
            OperatorResponse circleResponse = new OperatorResponse();
            NetworkResponse netwotk1 = httpResponse.POST(ShighraPayCredentials.OperatorSearchURL, OperatorSearchrequestJSON);

            try
            {
                circleResponse = JsonConvert.DeserializeObject<OperatorResponse>(netwotk1.Response);
                input.circleid = circleResponse.circleid;
                input.opid = circleResponse.opid;
            }
            catch
            {
                return record;
            }

            try
            {
                string requestJSON = JsonConvert.SerializeObject(input);
                NetworkResponse netwotk = httpResponse.POST(ShighraPayCredentials.PlanURL, requestJSON);

                if (!new Validation().IsValidString(netwotk.Response))
                {
                    return record;
                }

                if (netwotk.Response.ToUpper().Contains(Message.PlanNotAvailable.ToUpper()))
                {
                    return record;
                }

                RechargeResponse Response = new RechargeResponse();

                Response = JsonConvert.DeserializeObject<RechargeResponse>(netwotk.Response);
                record.Message = "Ok";
                int i = 0;

                if (Response.plans?.FULLTT != null)
                {
                    var FULLTT = new The2G[Response.plans.FULLTT.Count];

                    foreach (var item in Response.plans.FULLTT)
                    {
                        FULLTT[i] = new The2G();
                        FULLTT[i].Desc = item.desc;
                        FULLTT[i].LastUpdate = item.last_update;
                        FULLTT[i].Rs = item.rs.ToString();
                        FULLTT[i].Validity = item.validity;
                        record.FULLTT = FULLTT;
                        i++;
                    }
                }

                i = 0;
                if (Response.plans?.TOPUP != null)
                {
                    var TOPUP = new The2G[Response.plans.TOPUP.Count];

                    foreach (var item in Response.plans.TOPUP)
                    {
                        TOPUP[i] = new The2G();
                        TOPUP[i].Desc = item.desc;
                        TOPUP[i].LastUpdate = item.last_update;
                        TOPUP[i].Rs = item.rs.ToString();
                        TOPUP[i].Validity = item.validity;
                        record.TOPUP = TOPUP;
                        i++;
                    }
                }

                i = 0;
                if (Response.plans?._3G4G != null)
                {               
                    var _3G4G = new The2G[Response.plans._3G4G.Count];

                    foreach (var item in Response.plans._3G4G)
                    {
                        _3G4G[i] = new The2G();
                        _3G4G[i].Desc = item.desc;
                        _3G4G[i].LastUpdate = item.last_update;
                        _3G4G[i].Rs = item.rs.ToString();
                        _3G4G[i].Validity = item.validity;
                        record._3G4G = _3G4G;
                        i++;
                    }
                }

                i = 0;
                if (Response.plans?._2G != null)
                {

                    var _2G = new The2G[Response.plans._2G.Count];

                    foreach (var item in Response.plans._2G)
                    {
                        _2G[i] = new The2G();
                        _2G[i].Desc = item.desc;
                        _2G[i].LastUpdate = item.last_update;
                        _2G[i].Rs = item.rs.ToString();
                        _2G[i].Validity = item.validity;
                        record._2G = _2G;
                        i++;
                    }
                }
     
                i = 0;
                if (Response.plans?.Roaming != null)
                {
                    var Roaming = new The2G[Response.plans.Roaming.Count];

                    foreach (var item in Response.plans.Roaming)
                    {
                        Roaming[i] = new The2G();
                        Roaming[i].Desc = item.desc;
                        Roaming[i].LastUpdate = item.last_update;
                        Roaming[i].Rs = item.rs.ToString();
                        Roaming[i].Validity = item.validity;
                        record.Roaming = Roaming;
                        i++;
                    }
                }



                record.Message = Response.message;
            }
            catch (Exception)
            {
                return record;
            }

            return record;
        }

        public static DTHRecords SelectDTHPlan(DTHRequest input, string Operator)
        {
            DTHRecords record = new DTHRecords();
            Operator = GetDTHOperatorName(Operator);

            input.opid = Operator;
            input.circleid = "4"; // Maharashtra Circle ID ByDefault

            try
            {
                HttpProcessing httpResponse = new HttpProcessing();

                string requestJSON = JsonConvert.SerializeObject(input);
                NetworkResponse netwotk = httpResponse.POST(ShighraPayCredentials.PlanURL, requestJSON);

                if (!new Validation().IsValidString(netwotk.Response))
                {
                    return record;
                }

                if (netwotk.Response.ToUpper().Contains(Message.PlanNotAvailable.ToUpper()))
                {
                    return record;
                }

                DTHResponse Response = new DTHResponse();

                Response = JsonConvert.DeserializeObject<DTHResponse>(netwotk.Response);
                record.Message = "";

                int i = 0;
                if (Response.plans.Plan != null)
                {
                    var Plan = new Plan[Response.plans.Plan.Count];
                    foreach (var item in Response.plans.Plan)
                    {
                        Plan[i] = new Plan();
                        Plan[i].Rs = new PlanRs();
                        Plan[i].Desc = item.desc;
                        Plan[i].LastUpdate = item.last_update;
                        Plan[i].PlanName = item.plan_name;
                        Plan[i].Rs.The1Months = item.rs._1MONTHS?.ToString();
                        Plan[i].month1 = item.rs._1MONTHS?.ToString().Trim();
                        Plan[i].Rs.The3Months = item.rs._3MONTHS?.ToString();
                        //Plan[i].month3 = item.rs._3MONTHS?.ToString().Trim();
                        Plan[i].Rs.The6Months = item.rs._6MONTHS?.ToString();
                        //Plan[i].month6 = item.rs._6MONTHS?.ToString().Trim();
                        Plan[i].Rs.The1Year = item.rs._1YEAR?.ToString();
                        //Plan[i].Year1 = item.rs._1YEAR?.ToString().Trim();
                        record.Plan = Plan;
                        i++;
                    }
                }

                record.Message = Response.message;
            }
            catch (Exception)
            {
                return record;
            }

            return record;
        }

        private static string GetDTHOperatorName(string Operator)
        {
            string @operator = string.Empty;
            if (Operator.Trim().ToUpper().Contains("Airtel Digital Tv".Trim().ToUpper()))
                @operator = "17";
            else if (Operator.Trim().ToUpper().Contains("Dish Tv".Trim().ToUpper()))
                @operator = "12";
            else if (Operator.Trim().ToUpper().Contains("Sun Direct".Trim().ToUpper()))
                @operator = "14";
            else if (Operator.Trim().ToUpper().Contains("Tatasky".Trim().ToUpper()))
                @operator = "13";
            else if (Operator.Trim().ToUpper().Contains("Videocon D2h".Trim().ToUpper()))
                @operator = "15";

            return @operator;
        }
    }
}