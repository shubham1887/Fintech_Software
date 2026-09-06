using MplanCore.Response;
using MplanCore.StaticModal;
using Newtonsoft.Json;

namespace MplanCore.Processing
{
    public class EzytmProcessing
    {
        private static string GetCircleCode(string mobile)
        {
            if (string.IsNullOrEmpty(mobile))
            {
                return "90";// 90 is for Maharashtra
            }

            string httpResponse = "";
            string Url = EzytmCredentials.OpertorCheckURL;
            Url = Url.Replace("[Mobile]", mobile);
            Url = Url.Replace("[ApiUserID]", EzytmCredentials.ApiUserID);
            Url = Url.Replace("[ApiPassword]", EzytmCredentials.ApiPassword);
            try
            {
                httpResponse = HttpProcessing.CallGetMethod(Url);
            }
            catch (Exception)
            {
                return "90";// 90 is for Maharashtra
            }

            if (httpResponse == null || httpResponse.Length <= 5)
                return "90";// 90 is for Maharashtra

            EzytmCircleResponse circleResponse = new EzytmCircleResponse();

            try
            {
                circleResponse = JsonConvert.DeserializeObject<EzytmCircleResponse>(httpResponse);
            }
            catch
            {
                return "90";// 90 is for Maharashtra
            }

            if (circleResponse == null)
                return "90";// 90 is for Maharashtra

            if (circleResponse.CircleCode == null || circleResponse.CircleCode.Trim().Length <= 0)
                return "90";// 90 is for Maharashtra

            return circleResponse.CircleCode ?? "90";
        }

        public static List<ROfferResponse.Record> SelectROffer(string Operator, string ConsumerNumber)
        {
            ROfferResponse.Record report = new ROfferResponse.Record
            {
                desc = Message.PlanNotAvailable,
            };

            List<ROfferResponse.Record> details = new List<ROfferResponse.Record>
            {
                report
            };

            string opName = GetMobileOperatorName(Operator);

            bool b = IsBSNL(Operator);
            if (b)
            {
                opName = "5";
            }

            string URL = EzytmCredentials.ROfferURL;
            URL = URL.Replace("[ApiUserID]", EzytmCredentials.ApiUserID);
            URL = URL.Replace("[ApiPassword]", EzytmCredentials.ApiPassword);
            URL = URL.Replace("[Operator]", opName);
            URL = URL.Replace("[ConsumerNumber]", ConsumerNumber);

            string httpRseponse;
            try
            {
                httpRseponse = HttpProcessing.CallGetMethod(URL);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().Write(ex);
                return details;
            }

            if (!new Validation().IsValidString(httpRseponse))
            {
                return details;
            }

            if (httpRseponse.ToUpper().Contains(Message.PlanNotAvailable.ToUpper()))
            {
                return details;
            }

            if (httpRseponse.ToUpper().Contains(Message.PlanExpired.ToUpper()))
            {
                return details;
            }

            EzytmROfferResponse Response = null;
            try
            {
                Response = JsonConvert.DeserializeObject<EzytmROfferResponse>(httpRseponse);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().Write(ex);
                return details;
            }

            if (Response.STATUS == "1" && Response.RDATA.Count > 0)
            {
                try
                {
                    foreach (var item in Response.RDATA)
                    {
                        var records = new ROfferResponse.Record();
                        records.desc = item.logdesc;
                        records.status = item.ofrtext;
                        records.rs = item.price;
                        details.Add(records);
                    }
                    details.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    new ErrorProcessing().Write(ex);
                    report.desc = Message.PlanNotAvailable;
                    details.Add(report);
                    return details;
                }
            }

            return details;
        }

        private static bool IsBSNL(string Operator)
        {
            if (Operator.Trim().ToUpper().Contains("Bsnl".Trim().ToUpper()))
                return true;
            if (Operator.Trim().ToUpper().Contains("Bsnl Topup".Trim().ToUpper()))
                return true;
            if (Operator.Trim().ToUpper().Contains("Bsnl Special".Trim().ToUpper()))
                return true;

            return false;
        }

        public static Records SelectPlan(string Operator, string MobileNumber)
        {
            Records record = new Records
            {
                Message = Message.PlanNotAvailable
            };

            string URL = EzytmCredentials.PlanURL;
            string opName = GetMobileOperatorName(Operator);

            bool b = IsBSNL(Operator);
            if (b)
            {
                opName = "4";
            }

            URL = URL.Replace("[Circle]", GetCircleCode(MobileNumber));
            URL = URL.Replace("[ApiUserID]", EzytmCredentials.ApiUserID);
            URL = URL.Replace("[ApiPassword]", EzytmCredentials.ApiPassword);
            URL = URL.Replace("[Operator]", opName);

            EzytmPlanResponse Response = new EzytmPlanResponse();

            string httpResponse;
            try
            {
                httpResponse = HttpProcessing.CallGetMethod(URL);

                if (!new Validation().IsValidString(httpResponse))
                {
                    return record;
                }

                if (httpResponse.ToUpper().Contains(Message.PlanNotAvailable.ToUpper()))
                {
                    return record;
                }

                Response = JsonConvert.DeserializeObject<EzytmPlanResponse>(httpResponse);
                record.Message = "Ok";
                int i = 0;
                if (Response.RDATA?.FULLTT != null)
                {
                    var FULLTT = new The2G[Response.RDATA.FULLTT.Count];

                    foreach (var item in Response.RDATA.FULLTT)
                    {
                        FULLTT[i] = new The2G();
                        FULLTT[i].Desc = item.desc;
                        FULLTT[i].LastUpdate = item.Type;
                        FULLTT[i].Rs = item.rs.ToString();
                        FULLTT[i].Validity = item.validity;
                        record.FULLTT = FULLTT;
                        i++;
                    }
                }

                i = 0;
                if (Response.RDATA?.FRC != null)
                {
                    var FRC = new The2G[Response.RDATA.FRC.Count];

                    foreach (var item in Response.RDATA.FRC)
                    {
                        FRC[i] = new The2G();
                        FRC[i].Desc = item.desc;
                        FRC[i].LastUpdate = item.Type;
                        FRC[i].Rs = item.rs.ToString();
                        FRC[i].Validity = item.validity;
                        record.FRC = FRC;
                        i++;
                    }
                }

                i = 0;
                if (Response.RDATA?.SMS != null)
                {
                    var SMS = new The2G[Response.RDATA.SMS.Count];

                    foreach (var item in Response.RDATA.SMS)
                    {
                        SMS[i] = new The2G();
                        SMS[i].Desc = item.desc;
                        SMS[i].LastUpdate = item.Type;
                        SMS[i].Rs = item.rs.ToString();
                        SMS[i].Validity = item.validity;
                        record.SMS = SMS;
                        i++;
                    }
                }

                i = 0;
                if (Response.RDATA?.STV != null)
                {
                    var STV = new The2G[Response.RDATA.STV.Count];

                    foreach (var item in Response.RDATA.STV)
                    {
                        STV[i] = new The2G();
                        STV[i].Desc = item.desc;
                        STV[i].LastUpdate = item.Type;
                        STV[i].Rs = item.rs.ToString();
                        STV[i].Validity = item.validity;
                        record.COMBO = STV;
                        i++;
                    }
                }

                i = 0;
                if (Response.RDATA?.TOPUP != null)
                {
                    var TOPUP = new The2G[Response.RDATA.TOPUP.Count];

                    foreach (var item in Response.RDATA.TOPUP)
                    {
                        TOPUP[i] = new The2G();
                        TOPUP[i].Desc = item.desc;
                        TOPUP[i].LastUpdate = item.Type;
                        TOPUP[i].Rs = item.rs.ToString();
                        TOPUP[i].Validity = item.validity;
                        record.TOPUP = TOPUP;
                        i++;
                    }
                }

                i = 0;
                if (Response.RDATA?.DATA != null)
                {
                    var The3G4G = new The2G[Response.RDATA.DATA.Count];

                    foreach (var item in Response.RDATA.DATA)
                    {
                        The3G4G[i] = new The2G();
                        The3G4G[i].Desc = item.desc;
                        The3G4G[i].LastUpdate = item.Type;
                        The3G4G[i].Rs = item.rs.ToString();
                        The3G4G[i].Validity = item.validity;
                        record._3G4G = The3G4G;
                        i++;
                    }
                }

                i = 0;

                if (Response.RDATA?.Romaing != null)
                {
                    var Romaing = new The2G[Response.RDATA.Romaing.Count];
                    foreach (var item in Response.RDATA.Romaing)
                    {
                        Romaing[i] = new The2G();
                        Romaing[i].Desc = item.desc;
                        Romaing[i].LastUpdate = item.Type;
                        Romaing[i].Rs = item.rs.ToString();
                        Romaing[i].Validity = item.validity;
                        record.Roaming = Romaing;
                        i++;
                    }
                }

                i = 0;
                if (Response.RDATA?.RATE_CUTTER != null)
                {
                    var RATE_CUTTER = new The2G[Response.RDATA.RATE_CUTTER.Count];

                    foreach (var item in Response.RDATA.RATE_CUTTER)
                    {
                        RATE_CUTTER[i] = new The2G();
                        RATE_CUTTER[i].Desc = item.desc;
                        RATE_CUTTER[i].LastUpdate = item.Type;
                        RATE_CUTTER[i].Rs = item.rs.ToString();
                        RATE_CUTTER[i].Validity = item.validity;
                        record.RateCutter = RATE_CUTTER;
                        i++;
                    }
                }

                i = 0;
                if (Response.RDATA?.TwoG != null)
                {
                    var TwoG = new The2G[Response.RDATA.TwoG.Count];

                    foreach (var item in Response.RDATA.TwoG)
                    {
                        TwoG[i] = new The2G();
                        TwoG[i].Desc = item.desc;
                        TwoG[i].LastUpdate = item.Type;
                        TwoG[i].Rs = item.rs.ToString();
                        TwoG[i].Validity = item.validity;
                        record._2G = TwoG;
                        i++;
                    }
                }

                i = 0;
                if (Response.RDATA?.JioPhone != null)
                {
                    var JioPhone = new The2G[Response.RDATA.JioPhone.Count];

                    foreach (var item in Response.RDATA.JioPhone)
                    {
                        JioPhone[i] = new The2G();
                        JioPhone[i].Desc = item.desc;
                        JioPhone[i].LastUpdate = item.Type;
                        JioPhone[i].Rs = item.rs.ToString();
                        JioPhone[i].Validity = item.validity;
                        record.JioPhone = JioPhone;
                        i++;
                    }
                }

                record.Message = Response.MESSAGE;
            }
            catch (Exception)
            {
                return record;
            }

            return record;
        }

        public static DTHRecords SelectDTHPlan(string Operator)
        {
            DTHRecords record = new DTHRecords();
            Operator = GetDTHOperatorName(Operator);

            string URL = EzytmCredentials.DTHPlanURL;
            URL = URL.Replace("[ApiUserID]", EzytmCredentials.ApiUserID);
            URL = URL.Replace("[ApiPassword]", EzytmCredentials.ApiPassword);
            URL = URL.Replace("[Operator]", (Operator));

            string httpResponse = string.Empty;
            try
            {
                httpResponse = HttpProcessing.CallGetMethod(URL);
            }
            catch (Exception)
            {
                return record;
            }

            if (!new Validation().IsValidString(httpResponse))
            {
                return record;
            }

            if (httpResponse.ToUpper().Contains(Message.PlanNotAvailable.ToUpper()))
            {
                return record;
            }

            EzytmDTHPlanResponse Response = JsonConvert.DeserializeObject<EzytmDTHPlanResponse>(httpResponse);
            try
            {
                record.Message = "";

                int i = 0;
                if (Response.RDATA.AddOnPack != null)
                {
                    var AddOnPack = new AddOnPack[Response.RDATA.AddOnPack.Count];

                    foreach (var item in Response.RDATA.AddOnPack)
                    {
                        AddOnPack[i] = new AddOnPack();
                        AddOnPack[i].Desc = item.desc;
                        AddOnPack[i].LastUpdate = item.last_update;
                        AddOnPack[i].PlanName = item.plan_name;

                        var addOnPackRs = new AddOnPackRs();
                        addOnPackRs.The1Months = item.rs._1MONTHS.ToString();
                        AddOnPack[i].Rs.The1Months = addOnPackRs.The1Months;
                        AddOnPack[i].Rs.The1Months = item.rs._1MONTHS.ToString();
                        record.AddOnPack = AddOnPack;
                        i++;
                    }
                }

                i = 0;

                if (Response.RDATA.Plan != null)
                {
                    var Plan = new Plan[Response.RDATA.Plan.Count];
                    foreach (var item in Response.RDATA.Plan)
                    {
                        Plan[i] = new Plan();
                        Plan[i].Rs = new PlanRs();
                        Plan[i].Desc = item.desc;
                        Plan[i].LastUpdate = item.last_update;
                        Plan[i].PlanName = item.plan_name;
                        Plan[i].Rs.The1Months = item.rs._1MONTHS.ToString();
                        Plan[i].month1 = item.rs._1MONTHS.ToString().Trim();
                        record.Plan = Plan;
                        i++;
                    }
                }
                record.Message = Response.MESSAGE;
            }
            catch (Exception ex)
            {
            }
            return record;
        }

        public static SelectCustomerInfoResponse SelectCustomerInfo(string Operator, string ConsumerNumber)
        {
            SelectCustomerInfoResponse report = new SelectCustomerInfoResponse();

            Operator = GetDTHOperatorName(Operator);
            string URL = EzytmCredentials.CustomerInfoURL;
            URL = URL.Replace("[ApiUserID]", EzytmCredentials.ApiUserID);
            URL = URL.Replace("[ApiPassword]", EzytmCredentials.ApiPassword);
            URL = URL.Replace("[Operator]", Operator);
            URL = URL.Replace("[ConsumerNumber]", ConsumerNumber);
            report.tel = ConsumerNumber;
            report.Operator = Operator;
            report.status = 1;
            report.desc = "SUCCESS";

            EzytmCustomerInfoResponse response = null;
            string httpResponse;
            try
            {
                httpResponse = HttpProcessing.CallGetMethod(URL);

                if (!new Validation().IsValidString(httpResponse))
                {
                    return report;
                }

                if (httpResponse.ToUpper().Contains(Message.PlanNotAvailable.ToUpper()))
                {
                    return report;
                }

                if (httpResponse.ToUpper().Contains(Message.PlanExpired.ToUpper()))
                {
                    return report;
                }

                if (httpResponse.ToUpper().Contains(Message.UnkonwnError.ToUpper()))
                {
                    return report;
                }

                response = JsonConvert.DeserializeObject<EzytmCustomerInfoResponse>(httpResponse);

                if (response == null)
                {
                    return report;
                }
            }
            catch (Exception)
            {
                return report;
            }

            List<Record> records1 = new List<Record>();

            if (response.DATA != null)
            {
                var records = new Record();
                records.customerName = response.DATA.Name;
                records.Balance = response.DATA.Balance;
                records.MonthlyRecharge = response.DATA.Monthly;
                records.NextRechargeDate = response.DATA.NextRechargeDate;
                records.planname = response.DATA.Plan;
                records.status = response.Message;
                records1.Add(records);
            }
            report.records = records1;

            return report;
        }

        private static string GetMobileOperatorName(string Operator)
        {
            string @operator = string.Empty;
            if (Operator.Trim().ToUpper().Contains("VODAFONE".ToUpper()))
                @operator = "23";
            else if (Operator.Trim().ToUpper().Contains("AIRTEL".Trim().ToUpper()))
                @operator = "2";
            else if (Operator.Trim().ToUpper().Contains("Bsnl Topup".Trim().ToUpper()))
                @operator = "4";
            else if (Operator.Trim().ToUpper().Contains("Bsnl Special".Trim().ToUpper()))
                @operator = "5";
            else if (Operator.Trim().ToUpper().Contains("Bsnl".Trim().ToUpper()))
                @operator = "4";
            else if (Operator.Trim().ToUpper().Contains("IDEA".Trim().ToUpper()))
                @operator = "6";
            else if (Operator.Trim().ToUpper().Contains("JIO".Trim().ToUpper()))
                @operator = "11";

            return @operator;
        }

        private static string GetDTHOperatorName(string Operator)
        {
            string @operator = string.Empty;
            if (Operator.Trim().ToUpper().Contains("Airtel Digital Tv".Trim().ToUpper()))
                @operator = "24";
            else if (Operator.Trim().ToUpper().Contains("Dish Tv".Trim().ToUpper()))
                @operator = "25";
            else if (Operator.Trim().ToUpper().Contains("RELIANCE BIGTV".Trim().ToUpper()))
                @operator = "26";
            else if (Operator.Trim().ToUpper().Contains("Sun Direct".Trim().ToUpper()))
                @operator = "27";
            else if (Operator.Trim().ToUpper().Contains("TATASKY".Trim().ToUpper()))
                @operator = "28";
            else if (Operator.Trim().ToUpper().Contains("Videocon D2h".Trim().ToUpper()))
                @operator = "29";

            return @operator;
        }

        public static ElectricityViewResponse ViewElectricityUserInfo(string Operator, string ConsumerNumber)
        {
            ElectricityViewResponse RResponse = new ElectricityViewResponse();

            string URL = EzytmCredentials.ElectricityURL;
            URL = URL.Replace("[ApiUserID]", EzytmCredentials.ApiUserID);
            URL = URL.Replace("[ApiPassword]", EzytmCredentials.ApiPassword);
            URL = URL.Replace("[Operator]", Operator);
            URL = URL.Replace("[ConsumerNumber]", ConsumerNumber);
            URL = URL.Replace("[MobileNumber]", EzytmCredentials.ApiPassword);

            string httpResponse;
            try
            {
                httpResponse = HttpProcessing.CallGetMethod(URL);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().Write(ex);
                return RResponse;
            }

            if (!new Validation().IsValidString(httpResponse))
            {
                RResponse.Message = Message.CustomerNotFound;
                return RResponse;
            }

            if (httpResponse.ToUpper().Contains(Message.CustomerNotFound.ToUpper()))
            {
                RResponse.Message = Message.CustomerNotFound;
                return RResponse;
            }

            ElectricityResponse Response = JsonConvert.DeserializeObject<ElectricityResponse>(httpResponse);
            if (Response.status == 1)
            {
                RResponse.Status = Message.SUCCESS;
                RResponse.Message = "";
                //RResponse.CustomerName = Response.records[0].CustomerName;
                // //RResponse.Duedate = Convert.ToString(Response.records[0].Duedate);
                // RResponse.Billdate = Convert.ToString(Response.records[0].Billdate);
                // RResponse.Billamount = Convert.ToString(Response.records[0].Billamount);
            }

            return RResponse;
        }
    }
}