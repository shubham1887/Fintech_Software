using MplanCore.Response;
using MplanCore.Response.InfoAPI;
using MplanCore.StaticModal;
using Newtonsoft.Json;

namespace MplanCore.Processing
{
    public class InfoAPIProcessing
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
            string URL = InfoapiCredentials.ROfferURL;
            URL = URL.Replace("[ApiKey]", InfoapiCredentials.ApiKey);
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

            if (httpRseponse.ToUpper().Contains(Message.InvalidUser.ToUpper()))
            {
                return details;
            }

            if (httpRseponse.ToUpper().Contains(Message.PlanExpired.ToUpper()))
            {
                report.desc = Message.PlanExpired;
                return details;
            }
            if (httpRseponse.ToUpper().Contains(Message.CheckAPIDocument.ToUpper()))
            {
                report.desc = Message.CheckAPIDocument;
                return details;
            }

            InfoAPIRofferResponse infoAPIRofferResponse = null;
            try
            {
                infoAPIRofferResponse = JsonConvert.DeserializeObject<InfoAPIRofferResponse>(httpRseponse);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().Write(ex);
                return details;
            }

            if (infoAPIRofferResponse.status == "success" && infoAPIRofferResponse.data.Count > 0)
            {
                try
                {
                    foreach (var items in infoAPIRofferResponse.data)
                    {
                        var records = new ROfferResponse.Record();
                        records.desc = items.description;
                        records.status = items.commission;
                        records.rs = items.price;
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

        private static string GetMobileOperatorName(string Operator)
        {
            string @operator = string.Empty;
            if (Operator.Trim().ToUpper().Contains("Airtel".ToUpper()))
                @operator = "1";
            else if (Operator.Trim().ToUpper().Contains("BSNL".Trim().ToUpper()))
                @operator = "2";
            else if (Operator.Trim().ToUpper().Contains("JIO".Trim().ToUpper()))
                @operator = "3";
            else if (Operator.Trim().ToUpper().Contains("VODAFONE IDEA VI".Trim().ToUpper()))
                @operator = "4";
            else if (Operator.Trim().ToUpper().Contains("IDEA".Trim().ToUpper()))
                @operator = "4";
            else if (Operator.Trim().ToUpper().Contains("VODAFONE".Trim().ToUpper()))
                @operator = "4";
            else if (Operator.Trim().ToUpper().Contains("AIRTEL POSTPAID".Trim().ToUpper()))
                @operator = "10";
            else if (Operator.Trim().ToUpper().Contains("BSNL POSTPAID".Trim().ToUpper()))
                @operator = "11";
            else if (Operator.Trim().ToUpper().Contains("JIO POSTPAID".Trim().ToUpper()))
                @operator = "12";
            else if (Operator.Trim().ToUpper().Contains("VODAFONE IDEA VI POSTPAID".Trim().ToUpper()))
                @operator = "13";
            else if (Operator.Trim().ToUpper().Contains("VODAFONE POSTPAID".Trim().ToUpper()))
                @operator = "13";
            else if (Operator.Trim().ToUpper().Contains("IDEA POSTPAID".Trim().ToUpper()))
                @operator = "13";

            return @operator;
        }

        public static Records SelectPlan(string Operator, string ConsumerNumber)
        {
            Records record = new Records
            {
                Message = Message.PlanNotAvailable
            };

            //Operator = GetMobileOperatorName(Operator);

            string URL = InfoapiCredentials.PlanURL;

            URL = URL.Replace("[ApiKey]", InfoapiCredentials.ApiKey);
            URL = URL.Replace("[ConsumerNumber]", ConsumerNumber);
            URL = URL.Replace("[Operator]", GetMobileOperatorName(Operator));

            InfoAPIPlan Response = new InfoAPIPlan();

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

                Response = JsonConvert.DeserializeObject<InfoAPIPlan>(httpResponse);
                record.Message = "Ok";

                Data data = Response?.data;

                int i = 0;
                var Romaing = new The2G[data.internationalroaming.Count];
                foreach (var plan in data?.internationalroaming)
                {
                    Romaing[i] = new The2G();
                    Romaing[i].Desc = plan?.description;
                    Romaing[i].LastUpdate = "";
                    Romaing[i].Rs = plan?.amount.ToString();
                    Romaing[i].Validity = plan?.validity.ToString();
                    record.Roaming = Romaing;
                    i++;
                }

                i = 0;
                var TOPUP = new The2G[data.topup.Count];
                foreach (var plan in data?.topup)
                {
                    TOPUP[i] = new The2G();
                    TOPUP[i].Desc = plan.description;
                    TOPUP[i].LastUpdate = "";
                    TOPUP[i].Rs = plan.amount.ToString();
                    TOPUP[i].Validity = plan.validity.ToString();
                    record.TOPUP = TOPUP;
                    i++;
                }

                i = 0;
                var FULLTT = new The2G[data.cricketplans.Count];
                foreach (var plan in data?.cricketplans)
                {
                    FULLTT[i] = new The2G();
                    FULLTT[i].Desc = plan.description;
                    FULLTT[i].LastUpdate = "";
                    FULLTT[i].Rs = plan.amount.ToString();
                    FULLTT[i].Validity = plan.validity.ToString();
                    record.FULLTT = FULLTT;
                    i++;
                }

                i = 0;
                var dataaddon = new The2G[data.dataaddon.Count];
                foreach (var plan in data?.dataaddon)
                {
                    dataaddon[i] = new The2G();
                    dataaddon[i].Desc = plan.description;
                    dataaddon[i].LastUpdate = "";
                    dataaddon[i].Rs = plan.amount.ToString();
                    dataaddon[i].Validity = plan.validity.ToString();
                    record.COMBO = dataaddon;
                    i++;
                }

                i = 0;
                var trulyunlimited = new The2G[data.trulyunlimited.Count];
                foreach (var plan in data?.trulyunlimited)
                {
                    trulyunlimited[i] = new The2G();
                    trulyunlimited[i].Desc = plan.description;
                    trulyunlimited[i].LastUpdate = "";
                    trulyunlimited[i].Rs = plan.amount.ToString();
                    trulyunlimited[i].Validity = plan.validity.ToString();
                    record._3G4G = trulyunlimited;
                    i++;
                }
            }
            catch (Exception)
            {
                return record;
            }
            record.Message = Response.status;

            return record;
        }

        public static SelectCustomerInfoResponse SelectBillInfo(string Operator, string ConsumerNumber)
        {
            SelectCustomerInfoResponse report = new SelectCustomerInfoResponse();

            Operator = GetMobileOperatorName(Operator);
            string URL = InfoapiCredentials.PostpaidBillInfoURL;
            URL = URL.Replace("[ApiKey]", InfoapiCredentials.ApiKey);
            URL = URL.Replace("[ConsumerNumber]", ConsumerNumber);
            URL = URL.Replace("[Operator]", GetMobileOperatorName(Operator));

            InfoApiBillResponse response = null;
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
                    report.desc = Message.PlanNotAvailable;
                    return report;
                }

                if (httpResponse.ToUpper().Contains(Message.PlanExpired.ToUpper()))
                {
                    report.desc = Message.PlanExpired;
                    return report;
                }

                response = JsonConvert.DeserializeObject<InfoApiBillResponse>(httpResponse);
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

            if (response.data != null)
            {
                var records = new Record();
                records.customerName = response.data.name;
                records.Balance = response.data.billAmount.ToString();
                records.MonthlyRecharge = response.data.billDate;
                records.NextRechargeDate = response.data.billDueDate;
                //records.planname = response.data.cu;
                records.status = response.message;
                records1.Add(records);
            }
            report.records = records1;

            return report;
        }


        private static string GetDTHOperatorName(string Operator)
        {
            string @operator = string.Empty;
            if (Operator.Trim().ToUpper().Contains("AIRTEL DTH".Trim().ToUpper()))
                @operator = "5";
            else if (Operator.Trim().ToUpper().Contains("Dish Tv".Trim().ToUpper()))
                @operator = "6";
            else if (Operator.Trim().ToUpper().Contains("SunDirect".Trim().ToUpper()))
                @operator = "7";
            else if (Operator.Trim().ToUpper().Contains("TATASKY".Trim().ToUpper()))
                @operator = "8";
            else if (Operator.Trim().ToUpper().Contains("Videocon D2h".Trim().ToUpper()))
                @operator = "9";

            return @operator;
        }

        public static DTHRecords SelectDTHPlan(string Operator, string ConsumerNumber)
        {
            DTHRecords record = new DTHRecords();
            // Operator = GetDTHOperatorName(Operator);

            string URL = InfoapiCredentials.DTHPlanURL;
            URL = URL.Replace("[ApiKey]", InfoapiCredentials.ApiKey);
            URL = URL.Replace("[ConsumerNumber]", ConsumerNumber);
            URL = URL.Replace("[Operator]", GetDTHOperatorName(Operator));

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

            InfoApiDTHPlan Response = JsonConvert.DeserializeObject<InfoApiDTHPlan>(httpResponse);
            try
            {
                record.Message = "";

                int i = 0;
                if (Response.data._1month != null)
                {
                    var AddOnPack = new AddOnPack[Response.data._1month.Count];

                    foreach (var item in Response?.data._1month)
                    {
                        AddOnPack[i] = new AddOnPack();
                        AddOnPack[i].Desc = item?.description;
                        AddOnPack[i].LastUpdate = item?.validity;
                        AddOnPack[i].PlanName = item?.category;

                        var addOnPackRs = new AddOnPackRs();
                        addOnPackRs.The1Months = item?.amount.ToString();
                        AddOnPack[i].Rs.The1Months = addOnPackRs?.The1Months;
                        // AddOnPack[i].Rs.The1Months = item.amount._1Month.ToString();
                        record.AddOnPack = AddOnPack;
                        i++;
                    }
                }

                record.Message = Response.message;
            }
            catch (Exception ex)
            {
            }
            return record;
        }

        public static List<ROfferResponse.Record> SelectDTHROffer(string Operator, string ConsumerNumber)
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
            string URL = InfoapiCredentials.DTHROfferURL;
            URL = URL.Replace("[ApiKey]", InfoapiCredentials.ApiKey);
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

            if (httpRseponse.ToUpper().Contains(Message.InvalidUser.ToUpper()))
            {
                return details;
            }

            if (httpRseponse.ToUpper().Contains(Message.PlanExpired.ToUpper()))
            {
                report.desc = Message.PlanExpired;
                return details;
            }
            if (httpRseponse.ToUpper().Contains(Message.CheckAPIDocument.ToUpper()))
            {
                report.desc = Message.CheckAPIDocument;
                return details;
            }

            DTHRofferResponse infoAPIRofferResponse = null;
            try
            {
                infoAPIRofferResponse = JsonConvert.DeserializeObject<DTHRofferResponse>(httpRseponse);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().Write(ex);
                return details;
            }

            if (infoAPIRofferResponse.status == "success")
            {
                try
                {
                    //foreach (var items in infoAPIRofferResponse.data)

                    var records = new ROfferResponse.Record();
                    records.desc = infoAPIRofferResponse.message;
                    records.status = infoAPIRofferResponse.status;
                    //records.rs = infoAPIRofferResponse.price;
                    details.Add(records);

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

        public static ElectricityViewResponse ViewElectricityUserInfo(string Operator, string ConsumerNumber)
        {
            ElectricityViewResponse RResponse = new ElectricityViewResponse();

            string URL = InfoapiCredentials.ElectricityURL;
            URL = URL.Replace("[ApiKey]", InfoapiCredentials.ApiKey);
            URL = URL.Replace("[ConsumerNumber]", ConsumerNumber);
            URL = URL.Replace("[Operator]", Operator);

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

            Validation valid = new Validation();
            if (!valid.IsValidString(httpResponse) ||
                (httpResponse.ToUpper().Contains(Message.CustomerNotFound.ToUpper())))
            {
                RResponse.Message = Message.CustomerNotFound;
                return RResponse;
            }

            InfoApiElectricityResponse Response = JsonConvert.DeserializeObject<InfoApiElectricityResponse>(httpResponse);
            if (Response.status == "success")
            {
                RResponse.Status = Message.SUCCESS;
                RResponse.Message = "";
                RResponse.CustomerName = Response.data.name;
                RResponse.Duedate = Response.data.billDueDate;
                RResponse.Billdate = Response.data.billDate;
                RResponse.Billamount = Convert.ToString(Response.data.billAmount);
            }

            return RResponse;
        }

    }
}