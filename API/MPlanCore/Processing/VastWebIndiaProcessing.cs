using MplanCore.Response;
using MplanCore.StaticModal;
using Newtonsoft.Json;

namespace MplanCore.Processing
{
    public class VastWebIndiaProcessing
    {
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

            Operator = GetMobileOperatorName(Operator);
            string URL = VastWebIndiaCredentials.ROfferURL;
            URL = URL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
            URL = URL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
            URL = URL.Replace("[Operator]", Operator);
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

            VastWebIndiaROfferResponse Response = null;
            try
            {
                Response = JsonConvert.DeserializeObject<VastWebIndiaROfferResponse>(httpRseponse);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().Write(ex);
                return details;
            }

            if (Response == null)
            {
                return details;
            }

            if (Response.status == "SUCCESS")
            {
                try
                {
                    foreach (var item in Response.Response)
                    {
                        var records = new ROfferResponse.Record();
                        records.desc = item.offerDetails;
                        records.status = item.offer;
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

        public static Records SelectPlan(string Operator)
        {
            Records record = new Records
            {
                Message = Message.PlanNotAvailable
            };

            string URL = VastWebIndiaCredentials.MobilePlanNameURL;
            URL = URL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
            URL = URL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
            URL = URL.Replace("[Operator]", GetMobileOperatorName(Operator));

            VastWebIndiaPlanNameResponse Response = new VastWebIndiaPlanNameResponse();

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

                if (httpResponse.ToUpper().Contains(Message.PlanExpired.ToUpper()))
                {
                    return record;
                }

                Response = JsonConvert.DeserializeObject<VastWebIndiaPlanNameResponse>(httpResponse);
                record.Message = "Ok";
                foreach (var item in Response.Response)
                {
                    if (item.name == "FULLTT")
                    {
                        string PlanURL = VastWebIndiaCredentials.MobilePlanURL;
                        PlanURL = PlanURL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
                        PlanURL = PlanURL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
                        PlanURL = PlanURL.Replace("[Operator]", GetMobileOperatorName(Operator));
                        PlanURL = PlanURL.Replace("[Name]", item.name);

                        VastWebIndiaPlanResponse mobile = new VastWebIndiaPlanResponse();
                        httpResponse = HttpProcessing.CallGetMethod(PlanURL);

                        mobile = JsonConvert.DeserializeObject<VastWebIndiaPlanResponse>(httpResponse);

                        int i = 0;
                        var FULLTT = new The2G[mobile.Response.Count];
                        foreach (var plan in mobile.Response)
                        {
                            FULLTT[i] = new The2G();
                            FULLTT[i].Desc = plan.description;
                            FULLTT[i].LastUpdate = plan.Talktime;
                            FULLTT[i].Rs = plan.price.ToString();
                            FULLTT[i].Validity = plan.Validity;
                            record.FULLTT = FULLTT;
                            i++;
                        }
                    }

                    if (item.name == "FRC")
                    {
                        string PlanURL = VastWebIndiaCredentials.MobilePlanURL;
                        PlanURL = PlanURL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
                        PlanURL = PlanURL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
                        PlanURL = PlanURL.Replace("[Operator]", GetMobileOperatorName(Operator));
                        PlanURL = PlanURL.Replace("[Name]", item.name);

                        VastWebIndiaPlanResponse mobile = new VastWebIndiaPlanResponse();
                        httpResponse = HttpProcessing.CallGetMethod(PlanURL);

                        mobile = JsonConvert.DeserializeObject<VastWebIndiaPlanResponse>(httpResponse);

                        int i = 0;
                        var FRC = new The2G[mobile.Response.Count];
                        foreach (var plan in mobile.Response)
                        {
                            FRC[i] = new The2G();
                            FRC[i].Desc = plan.description;
                            FRC[i].LastUpdate = plan.Talktime;
                            FRC[i].Rs = plan.price.ToString();
                            FRC[i].Validity = plan.Validity;
                            record.FRC = FRC;
                            i++;
                        }
                    }

                    if (item.name == "SMS")
                    {
                        string PlanURL = VastWebIndiaCredentials.MobilePlanURL;
                        PlanURL = PlanURL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
                        PlanURL = PlanURL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
                        PlanURL = PlanURL.Replace("[Operator]", GetMobileOperatorName(Operator));
                        PlanURL = PlanURL.Replace("[Name]", item.name);

                        VastWebIndiaPlanResponse mobile = new VastWebIndiaPlanResponse();
                        httpResponse = HttpProcessing.CallGetMethod(PlanURL);

                        mobile = JsonConvert.DeserializeObject<VastWebIndiaPlanResponse>(httpResponse);

                        var SMS = new The2G[mobile.Response.Count];
                        int i = 0;
                        foreach (var plan in mobile.Response)
                        {
                            SMS[i] = new The2G();
                            SMS[i].Desc = plan.description;
                            SMS[i].LastUpdate = plan.Talktime;
                            SMS[i].Rs = plan.price.ToString();
                            SMS[i].Validity = plan.Validity;
                            record.SMS = SMS;
                            i++;
                        }
                    }

                    if (item.name == "STV")
                    {
                        string PlanURL = VastWebIndiaCredentials.MobilePlanURL;
                        PlanURL = PlanURL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
                        PlanURL = PlanURL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
                        PlanURL = PlanURL.Replace("[Operator]", GetMobileOperatorName(Operator));
                        PlanURL = PlanURL.Replace("[Name]", item.name);

                        VastWebIndiaPlanResponse mobile = new VastWebIndiaPlanResponse();
                        httpResponse = HttpProcessing.CallGetMethod(PlanURL);

                        mobile = JsonConvert.DeserializeObject<VastWebIndiaPlanResponse>(httpResponse);

                        int i = 0;
                        var STV = new The2G[mobile.Response.Count];
                        foreach (var plan in mobile.Response)
                        {
                            STV[i] = new The2G();
                            STV[i].Desc = plan.description;
                            STV[i].LastUpdate = plan.Talktime;
                            STV[i].Rs = plan.price.ToString();
                            STV[i].Validity = plan.Validity;
                            record.COMBO = STV;
                            i++;
                        }
                    }

                    if (item.name == "Romaing")
                    {
                        string PlanURL = VastWebIndiaCredentials.MobilePlanURL;
                        PlanURL = PlanURL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
                        PlanURL = PlanURL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
                        PlanURL = PlanURL.Replace("[Operator]", GetMobileOperatorName(Operator));
                        PlanURL = PlanURL.Replace("[Name]", item.name);

                        VastWebIndiaPlanResponse mobile = new VastWebIndiaPlanResponse();
                        httpResponse = HttpProcessing.CallGetMethod(PlanURL);

                        mobile = JsonConvert.DeserializeObject<VastWebIndiaPlanResponse>(httpResponse);

                        int i = 0;
                        var Romaing = new The2G[mobile.Response.Count];
                        foreach (var plan in mobile.Response)
                        {
                            Romaing[i] = new The2G();
                            Romaing[i].Desc = plan.description;
                            Romaing[i].LastUpdate = plan.Talktime;
                            Romaing[i].Rs = plan.price.ToString();
                            Romaing[i].Validity = plan.Validity;
                            record.Roaming = Romaing;
                            i++;
                        }
                    }

                    if (item.name == "TOPUP")
                    {
                        string PlanURL = VastWebIndiaCredentials.MobilePlanURL;
                        PlanURL = PlanURL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
                        PlanURL = PlanURL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
                        PlanURL = PlanURL.Replace("[Operator]", GetMobileOperatorName(Operator));
                        PlanURL = PlanURL.Replace("[Name]", item.name);

                        VastWebIndiaPlanResponse mobile = new VastWebIndiaPlanResponse();
                        httpResponse = HttpProcessing.CallGetMethod(PlanURL);

                        mobile = JsonConvert.DeserializeObject<VastWebIndiaPlanResponse>(httpResponse);

                        int i = 0;
                        var TOPUP = new The2G[mobile.Response.Count];
                        foreach (var plan in mobile.Response)
                        {
                            TOPUP[i] = new The2G();
                            TOPUP[i].Desc = plan.description;
                            TOPUP[i].LastUpdate = plan.Talktime;
                            TOPUP[i].Rs = plan.price.ToString();
                            TOPUP[i].Validity = plan.Validity;
                            record.TOPUP = TOPUP;
                            i++;
                        }
                    }

                    if (item.name == "DATA")
                    {
                        string PlanURL = VastWebIndiaCredentials.MobilePlanURL;
                        PlanURL = PlanURL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
                        PlanURL = PlanURL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
                        PlanURL = PlanURL.Replace("[Operator]", GetMobileOperatorName(Operator));
                        PlanURL = PlanURL.Replace("[Name]", item.name);

                        VastWebIndiaPlanResponse mobile = new VastWebIndiaPlanResponse();
                        httpResponse = HttpProcessing.CallGetMethod(PlanURL);

                        mobile = JsonConvert.DeserializeObject<VastWebIndiaPlanResponse>(httpResponse);

                        int i = 0;
                        var The3G4G = new The2G[mobile.Response.Count];
                        foreach (var plan in mobile.Response)
                        {
                            The3G4G[i] = new The2G();

                            The3G4G[i].Desc = plan.description;
                            The3G4G[i].LastUpdate = plan.Talktime;
                            The3G4G[i].Rs = plan.price.ToString();
                            The3G4G[i].Validity = plan.Validity;

                            record._3G4G = The3G4G;
                            i++;
                        }
                    }
                }

                record.Message = Response.status;
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

            string URL = VastWebIndiaCredentials.DTHPlanNameURL;
            URL = URL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
            URL = URL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
            URL = URL.Replace("[Operator]", GetMobileOperatorName(Operator));

            VastWebIndiaPlanNameResponse Response = new VastWebIndiaPlanNameResponse();

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

                if (httpResponse.ToUpper().Contains(Message.PlanExpired.ToUpper()))
                {
                    return record;
                }

                Response = JsonConvert.DeserializeObject<VastWebIndiaPlanNameResponse>(httpResponse);

                foreach (var item in Response.Response)
                {
                    if (item.name.Equals("Monthly Pack"))
                    {
                        string PlanURL = VastWebIndiaCredentials.DTHPlanURL;
                        PlanURL = PlanURL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
                        PlanURL = PlanURL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
                        PlanURL = PlanURL.Replace("[Operator]", GetMobileOperatorName(Operator));
                        PlanURL = PlanURL.Replace("[Name]", item.name);

                        VastWebIndiaPlanResponse dth = new VastWebIndiaPlanResponse();
                        httpResponse = HttpProcessing.CallGetMethod(PlanURL);

                        dth = JsonConvert.DeserializeObject<VastWebIndiaPlanResponse>(httpResponse);

                        if (dth.Response == null)
                        {
                            continue;
                        }
                        if (dth.Response.Count <= 0)
                        {
                            continue;
                        }
                        var AddOnPack = new AddOnPack[4];
                        for (int i = 0; i < AddOnPack.Length; i++)
                        {
                            //foreach (var plan in Response.RDATA.AddOnPack)
                            //{
                            //    AddOnPack[i].Desc = item.desc;
                            //    AddOnPack[i].LastUpdate = item.last_update;
                            //    AddOnPack[i].PlanName = item.plan_name;

                            //    var addOnPackRs = new AddOnPackRs();
                            //    addOnPackRs.The1Months = item.rs._1MONTHS.ToString();
                            //    AddOnPack[i].Rs.The1Months = addOnPackRs.The1Months;
                            //    AddOnPack[i].Rs.The1Months = item.rs._1MONTHS.ToString();
                            //}
                            record.AddOnPack = AddOnPack;
                        }
                    }

                    if (item.name.Equals("3 Month Packs"))
                    {
                        string PlanURL = VastWebIndiaCredentials.DTHPlanURL;
                        PlanURL = PlanURL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
                        PlanURL = PlanURL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
                        PlanURL = PlanURL.Replace("[Operator]", GetMobileOperatorName(Operator));
                        PlanURL = PlanURL.Replace("[Name]", item.name);

                        VastWebIndiaPlanResponse dth = new VastWebIndiaPlanResponse();
                        httpResponse = HttpProcessing.CallGetMethod(PlanURL);

                        dth = JsonConvert.DeserializeObject<VastWebIndiaPlanResponse>(httpResponse);

                        if (dth.Response == null)
                        {
                            continue;
                        }
                        if (dth.Response.Count <= 0)
                        {
                            continue;
                        }
                        var AddOnPack = new AddOnPack[4];
                        for (int i = 0; i < AddOnPack.Length; i++)
                        {
                            //foreach (var plan in Response.RDATA.AddOnPack)
                            //{
                            //    AddOnPack[i].Desc = item.desc;
                            //    AddOnPack[i].LastUpdate = item.last_update;
                            //    AddOnPack[i].PlanName = item.plan_name;

                            //    var addOnPackRs = new AddOnPackRs();
                            //    addOnPackRs.The1Months = item.rs._1MONTHS.ToString();
                            //    AddOnPack[i].Rs.The1Months = addOnPackRs.The1Months;
                            //    AddOnPack[i].Rs.The1Months = item.rs._1MONTHS.ToString();
                            //}
                            record.AddOnPack = AddOnPack;
                        }
                    }

                    if (item.name.Equals("6 Month Packs"))
                    {
                        string PlanURL = VastWebIndiaCredentials.DTHPlanURL;
                        PlanURL = PlanURL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
                        PlanURL = PlanURL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
                        PlanURL = PlanURL.Replace("[Operator]", GetMobileOperatorName(Operator));
                        PlanURL = PlanURL.Replace("[Name]", item.name);

                        VastWebIndiaPlanResponse dth = new VastWebIndiaPlanResponse();
                        httpResponse = HttpProcessing.CallGetMethod(PlanURL);

                        dth = JsonConvert.DeserializeObject<VastWebIndiaPlanResponse>(httpResponse);

                        if (dth.Response == null)
                        {
                            continue;
                        }
                        if (dth.Response.Count <= 0)
                        {
                            continue;
                        }
                        var AddOnPack = new AddOnPack[4];
                        for (int i = 0; i < AddOnPack.Length; i++)
                        {
                            //foreach (var plan in Response.RDATA.AddOnPack)
                            //{
                            //    AddOnPack[i].Desc = item.desc;
                            //    AddOnPack[i].LastUpdate = item.last_update;
                            //    AddOnPack[i].PlanName = item.plan_name;

                            //    var addOnPackRs = new AddOnPackRs();
                            //    addOnPackRs.The1Months = item.rs._1MONTHS.ToString();
                            //    AddOnPack[i].Rs.The1Months = addOnPackRs.The1Months;
                            //    AddOnPack[i].Rs.The1Months = item.rs._1MONTHS.ToString();
                            //}
                            record.AddOnPack = AddOnPack;
                        }
                    }

                    if (item.name.Equals("Annual Packs"))
                    {
                        string PlanURL = VastWebIndiaCredentials.DTHPlanURL;
                        PlanURL = PlanURL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
                        PlanURL = PlanURL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
                        PlanURL = PlanURL.Replace("[Operator]", GetMobileOperatorName(Operator));
                        PlanURL = PlanURL.Replace("[Name]", item.name);

                        VastWebIndiaPlanResponse dth = new VastWebIndiaPlanResponse();
                        httpResponse = HttpProcessing.CallGetMethod(PlanURL);

                        dth = JsonConvert.DeserializeObject<VastWebIndiaPlanResponse>(httpResponse);

                        if (dth.Response == null)
                        {
                            continue;
                        }
                        if (dth.Response.Count <= 0)
                        {
                            continue;
                        }
                        var AddOnPack = new AddOnPack[4];
                        for (int i = 0; i < AddOnPack.Length; i++)
                        {
                            //foreach (var plan in Response.RDATA.AddOnPack)
                            //{
                            //    AddOnPack[i].Desc = item.desc;
                            //    AddOnPack[i].LastUpdate = item.last_update;
                            //    AddOnPack[i].PlanName = item.plan_name;

                            //    var addOnPackRs = new AddOnPackRs();
                            //    addOnPackRs.The1Months = item.rs._1MONTHS.ToString();
                            //    AddOnPack[i].Rs.The1Months = addOnPackRs.The1Months;
                            //    AddOnPack[i].Rs.The1Months = item.rs._1MONTHS.ToString();
                            //}
                            record.AddOnPack = AddOnPack;
                        }
                    }
                }

                record.Message = Response.status;
            }
            catch (Exception)
            {
                return record;
            }

            return record;
        }

        public static SelectCustomerInfoResponse SelectCustomerInfo(string Operator, string ConsumerNumber)
        {
            SelectCustomerInfoResponse report = new SelectCustomerInfoResponse();

            Operator = GetDTHOperatorName(Operator);
            string URL = VastWebIndiaCredentials.CustomerInfoURL;
            URL = URL.Replace("[ApiUserID]", VastWebIndiaCredentials.ApiUserID);
            URL = URL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
            URL = URL.Replace("[Operator]", Operator);
            URL = URL.Replace("[ConsumerNumber]", ConsumerNumber);
            report.tel = ConsumerNumber;
            report.Operator = Operator;
            report.status = 1;
            report.desc = "SUCCESS";

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
            }
            catch (Exception)
            {
                return report;
            }

            if (Operator.ToUpper().Contains("DishTv".ToUpper()))
            {
                VastWebIndiaDishTVCustomerInfoResponse response = null;

                try
                {
                    response = JsonConvert.DeserializeObject<VastWebIndiaDishTVCustomerInfoResponse>(httpResponse);
                }
                catch (Exception)
                {
                    return report;
                }

                List<Record> records1 = new List<Record>();

                if (response.Response != null)
                {
                    var records = new Record();
                    records.customerName = response.Response.SubscriberName;
                    records.Balance = response.Response.SubsBalance;
                    records.MonthlyRecharge = response.Response.PaytermName;
                    records.NextRechargeDate = response.Response.ActivatedOn;
                    records.planname = response.Response.SchemeName;
                    records.status = response.status;
                    records1.Add(records);
                }
                report.records = records1;
            }
            else if (Operator.ToUpper().Contains("AirtelDTH".ToUpper()))
            {
                VastWebIndiaAirtelDTHCustomerInfoResponse response = null;

                try
                {
                    response = JsonConvert.DeserializeObject<VastWebIndiaAirtelDTHCustomerInfoResponse>(httpResponse);
                }
                catch (Exception)
                {
                    return report;
                }

                List<Record> records1 = new List<Record>();

                if (response.Response != null)
                {
                    var records = new Record();
                    records.customerName = response.Response.name;
                    records.Balance = response.Response.currentbalance;
                    records.MonthlyRecharge = "MONTHLY";
                    records.NextRechargeDate = response.Response.validTill;
                    records.planname = "";
                    records.status = response.status;
                    records1.Add(records);
                }
                report.records = records1;
            }
            else
            {
                VastWebIndiaCustomerInfoResponse response = null;

                try
                {
                    response = JsonConvert.DeserializeObject<VastWebIndiaCustomerInfoResponse>(httpResponse);
                }
                catch (Exception)
                {
                    return report;
                }

                List<Record> records1 = new List<Record>();

                if (response.Response != null)
                {
                    var records = new Record();
                    records.customerName = response.Response.customerName;
                    records.Balance = response.Response.monthlyRecharge.ToString();
                    records.MonthlyRecharge = "MONTHLY";
                    records.NextRechargeDate = response.Response.rechargedueDate.ToString();
                    records.planname = response.Response.customerStatus;
                    records.status = response.status;
                    records1.Add(records);
                }
                report.records = records1;
            }

            return report;
        }

        private static string GetMobileOperatorName(string Operator)
        {
            string @operator = string.Empty;
            if (Operator.Trim().ToUpper().Contains("VODAFONE".ToUpper()))
                @operator = "VODAFONE";
            else if (Operator.Trim().ToUpper().Contains("AIRTEL".Trim().ToUpper()))
                @operator = "AIRTEL";
            else if (Operator.Trim().ToUpper().Contains("Bsnl Topup".Trim().ToUpper()))
                @operator = "BSNL";
            else if (Operator.Trim().ToUpper().Contains("Bsnl Special".Trim().ToUpper()))
                @operator = "BSNL";
            else if (Operator.Trim().ToUpper().Contains("IDEA".Trim().ToUpper()))
                @operator = "IDEA";
            else if (Operator.Trim().ToUpper().Contains("JIO".Trim().ToUpper()))
                @operator = "Jio";

            return @operator;
        }

        private static string GetDTHOperatorName(string Operator)
        {
            string @operator = string.Empty;
            if (Operator.Trim().ToUpper().Contains("Airtel Digital Tv".Trim().ToUpper()))
                @operator = "AirtelDTH";
            else if (Operator.Trim().ToUpper().Contains("Dish Tv".Trim().ToUpper()))
                @operator = "DISHTV";
            else if (Operator.Trim().ToUpper().Contains("RELIANCE BIGTV".Trim().ToUpper()))
                @operator = "26";
            else if (Operator.Trim().ToUpper().Contains("Sun Direct".Trim().ToUpper()))
                @operator = "27";
            else if (Operator.Trim().ToUpper().Contains("TATASKY".Trim().ToUpper()))
                @operator = "TATASKY";
            else if (Operator.Trim().ToUpper().Contains("Videocon D2h".Trim().ToUpper()))
                @operator = "VIDEOD2H";

            return @operator;
        }

        public static ElectricityViewResponse ViewElectricityUserInfo(string Operator, string ConsumerNumber)
        {
            ElectricityViewResponse RResponse = new ElectricityViewResponse();

            string URL = VastWebIndiaCredentials.ElectricityURL;
            URL = URL.Replace("[ApiKey]", VastWebIndiaCredentials.ApiUserID);
            URL = URL.Replace("[ApiToken]", VastWebIndiaCredentials.ApiToken);
            URL = URL.Replace("[Operator]", Operator);
            URL = URL.Replace("[ConsumerNumber]", ConsumerNumber);

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