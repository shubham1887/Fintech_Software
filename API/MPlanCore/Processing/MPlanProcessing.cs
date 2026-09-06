using MplanCore.Request;
using MplanCore.Response;
using MplanCore.StaticModal;
using Newtonsoft.Json;

namespace MplanCore.Processing
{
    public class MPlanProcessing
    {
        private static string GetOperatorName(string opName)
        {
            if (opName.ToLower().Contains("voda".ToLower()))
            {
                return "vodafone";
            }
            if (opName.ToLower().Contains("idea".ToLower()))
            {
                return "vodafone";
            }
            return opName;
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

            Operator = GetMobileOperatorName(Operator);
            string URL = MplanCredentials.ROfferURL;
            URL = URL.Replace("[ApiKey]", MplanCredentials.ApiKey);
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
                report.desc = Message.PlanExpired;
                return details;
            }

            ROfferResponse.Response Response = null;
            try
            {
                Response = JsonConvert.DeserializeObject<ROfferResponse.Response>(httpRseponse);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().Write(ex);
                return details;
            }

            if (Response.status == 1 && Response.records.Count > 0)
            {
                try
                {
                    details = Response.records;
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

        public static Records SelectPlan(string Operator, string Circle)
        {
            Records record = new Records
            {
                Message = Message.PlanNotAvailable
            };

            string URL = MplanCredentials.PlanURL;

            URL = URL.Replace("[ApiKey]", MplanCredentials.ApiKey);
            URL = URL.Replace("[Operator]", GetMobileOperatorName(GetOperatorName(Operator)));
            URL = URL.Replace("[Circle]", Circle);

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

                PlanResponse Response = JsonConvert.DeserializeObject<PlanResponse>(httpResponse);
               
               if (Response.Status==0)
                {
                    record.Message = Message.PlanNotAvailable;
                    return record;

                }
                record = Response.Records;
                record.Message = "Ok";
            }
            catch (Exception)
            {
                return record;
            }

            return record;
        }

        public static DTHRecords SelectDTHPlan(string Operator, string Circle)
        {
            DTHRecords record = new DTHRecords();
            Operator = GetDTHOperatorName(Operator);

            string URL = MplanCredentials.DTHPlanURL;
            URL = URL.Replace("[ApiKey]", MplanCredentials.ApiKey);
            URL = URL.Replace("[Operator]", Operator);

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

            if (httpResponse.ToUpper().Contains(Message.PlanExpired.ToUpper()))
            {
                record.Message = Message.PlanExpired;
                return record;
            }

            DTHPlansRecords Response = new DTHPlansRecords();

            Response = JsonConvert.DeserializeObject<DTHPlansRecords>(httpResponse);
            if (Response == null || Response.DTHRecord.Plan==null)

            {
                record.Message = Message.PlanNotAvailable;
                return record;
            }
            try
            {
                record = Response.DTHRecord;
                record.Message = "";


                int i = 0;
                foreach (var item in record.Plan)
                {
                    record.Plan[i].month1 = record.Plan[i].Rs.The1Months;
                    i++;
                }
                i = 0;
                foreach (var item in record.AddOnPack)
                {
                    record.AddOnPack[i].month1 = record.AddOnPack[i].Rs.The1Months;
                    i++;
                }
            }
            catch (Exception ex)
            {
            }
            return record;
        }

        public static SelectCustomerInfoResponse SelectCustomerInfo(string Operator, string ConsumerNumber)
        {
            SelectCustomerInfoResponse report = new SelectCustomerInfoResponse();

            Operator = GetDTHOperatorNameForCustInfo(Operator);
            string URL = MplanCredentials.CustomerInfoURL;
            URL = URL.Replace("[ApiKey]", MplanCredentials.ApiKey);
            URL = URL.Replace("[Operator]", Operator);
            URL = URL.Replace("[ConsumerNumber]", ConsumerNumber);

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

                report = JsonConvert.DeserializeObject<SelectCustomerInfoResponse>(httpResponse);
                if (report == null)
                {
                    return report;
                }
            }
            catch (Exception)
            {
                return report;
            }

            return report;
        }

        public static HeavyRefreshResult DTHHeavyRefresh(string Operator, string ConsumerNumber)
        {
            HeavyRefreshResult reportOutput = new HeavyRefreshResult();
            HeavyRefreshResponse report = null;

            Operator = GetDTHOperatorNameForCustInfo(Operator);
            string URL = MplanCredentials.HeavyRefreshURL;
            URL = URL.Replace("[ApiKey]", MplanCredentials.ApiKey);
            URL = URL.Replace("[Operator]", Operator);
            URL = URL.Replace("[ConsumerNumber]", ConsumerNumber);

            string httpResponse;
            try
            {
                httpResponse = HttpProcessing.CallGetMethod(URL);

                if (!new Validation().IsValidString(httpResponse))
                {
                    return reportOutput;
                }

                if (httpResponse.ToUpper().Contains(Message.PlanNotAvailable.ToUpper()))
                {
                    reportOutput.desc = Message.PlanExpired;
                    return reportOutput;
                }

                if (httpResponse.ToUpper().Contains(Message.PlanExpired.ToUpper()))
                {
                    reportOutput.desc = Message.PlanExpired;
                    return reportOutput;
                }

                report = JsonConvert.DeserializeObject<HeavyRefreshResponse>(httpResponse);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().Write(ex);
                return reportOutput;
            }

            reportOutput.tel = ConsumerNumber;
            reportOutput.opName = Operator;
            reportOutput.desc = "";
            reportOutput.customerName = "";

            if (report.records == null)
            {
                return reportOutput;
            }

            reportOutput.desc = report.records.desc;
            reportOutput.customerName = report.records.customerName;

            return reportOutput;
        }

        private static string GetMobileOperatorName(string Operator)
        {
            string @operator = string.Empty;
            if (Operator.Trim().ToUpper().Contains("VODAFONE".ToUpper()))
                @operator = "Vodafone";
            else if (Operator.Trim().ToUpper().Contains("AIRTEL".Trim().ToUpper()))
                @operator = "Airtel";
            else if (Operator.Trim().ToUpper().Contains("BSNL".Trim().ToUpper()))
                @operator = "BSNL";
            else if (Operator.Trim().ToUpper().Contains("DOCOMO".Trim().ToUpper()))
                @operator = "Tata Docomo";
            else if (Operator.Trim().ToUpper().Contains("IDEA".Trim().ToUpper()))
                @operator = "Idea";
            else if (Operator.Trim().ToUpper().Contains("JIO".Trim().ToUpper()))
                @operator = "Jio";
            else if (Operator.Trim().ToUpper().Contains("TATA Indi".Trim().ToUpper()))
                @operator = "Tata Indicom";
            else if (Operator.Trim().ToUpper().Contains("TATAIndi".Trim().ToUpper()))
                @operator = "Tata Indicom";
            else if (Operator.Trim().ToUpper().Contains("VODAFONE".ToUpper()))
                @operator = "Vodafone";
            else if (Operator.Trim().ToUpper().Contains("AIRTEL".Trim().ToUpper()))
                @operator = "Airtel";
            else if (Operator.Trim().ToUpper().Contains("Aircel".Trim().ToUpper()))
                @operator = "Aircel";
            else if (Operator.Trim().ToUpper().Contains("BSNL".Trim().ToUpper()))
                @operator = "Bsnl";
            else if (Operator.Trim().ToUpper().Contains("DOCOMO".Trim().ToUpper()))
                @operator = "Tata Docomo";
            else if (Operator.Trim().ToUpper().Contains("voda".Trim().ToUpper()))
                @operator = "vodafone";
            else if (Operator.Trim().ToUpper().Contains("MTS".Trim().ToUpper()))
                @operator = "MTS";
            else if (Operator.Trim().ToUpper().Contains("JIO".Trim().ToUpper()))
                @operator = "Jio";
            else if (Operator.Trim().ToUpper().Contains("TATA".Trim().ToUpper()))
                @operator = "Tata Indicom";
            else if (Operator.Trim().ToUpper().Contains("TATA Indi".Trim().ToUpper()))
                @operator = "Tata Indicom";
            else if (Operator.Trim().ToUpper().Contains("TATAIndi".Trim().ToUpper()))
                @operator = "Tata Indicom";
            else if (Operator.Trim().ToUpper().Contains("MTNL".Trim().ToUpper()))
                @operator = "MTNL";

            return @operator;
        }

        private static string GetDTHOperatorName(string Operator)
        {
            string @operator = string.Empty;
            if (Operator.ToLower().Contains("Airtel".ToLower()))
                @operator = "Airtel dth";
            else if (Operator.ToLower().Contains("Tata".ToLower()))
                @operator = "Tata Sky";
            else if (Operator.ToLower().Contains("Videocon".ToLower()))
                @operator = "Videocon";
            else if (Operator.ToLower().Contains("sun".ToLower()))
                @operator = "Sun Direct";
            else if (Operator.ToLower().Contains("Dish".ToLower()))
                @operator = "Dish tv";

            return @operator;
        }

        private static string GetDTHOperatorNameForCustInfo(string Operator)
        {
            string @operator = Operator;
            if (Operator.ToLower().Contains("Airtel".ToLower()))
                @operator = "Airteldth";
            else if (Operator.ToLower().Contains("Tata".ToLower()))
                @operator = "TataSky";
            else if (Operator.ToLower().Contains("Videocon".ToLower()))
                @operator = "Videocon";
            else if (Operator.ToLower().Contains("sun".ToLower()))
                @operator = "Sundirect";
            else if (Operator.ToLower().Contains("Dish".ToLower()))
                @operator = "Dishtv";
            return @operator;
        }

        public ElectricityViewResponse ViewElectricityUserInfo(string Operator, string ConsumerNumber)
        {
            ElectricityViewResponse RResponse = new ElectricityViewResponse();

            string URL = MplanCredentials.CustomerInfoURL;
            URL = URL.Replace("[ApiKey]", MplanCredentials.ApiKey);
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

            Validation valid = new Validation();
            if (!valid.IsValidString(httpResponse) ||
                (httpResponse.ToUpper().Contains(Message.CustomerNotFound.ToUpper())))
            {
                RResponse.Message = Message.CustomerNotFound;
                return RResponse;
            }

            ElectricityResponse Response = JsonConvert.DeserializeObject<ElectricityResponse>(httpResponse);
            if (Response.status == 1)
            {
                RResponse.Status = Message.SUCCESS;
                RResponse.Message = "";
            }

            return RResponse;
        }

        public static CircleResponse CircleApi(string MobileNumber)
        {
            
            CircleResponse response = new CircleResponse();

            string URL = CircleApiCredential.CircleApiURL;

            URL = URL.Replace("[apimember_id]", CircleApiCredential.ApiKey);
            URL = URL.Replace("[api_password]", CircleApiCredential.Password);
            URL = URL.Replace("[Number]", MobileNumber);

            string httpResponse;

            httpResponse = HttpProcessing.CallGetMethod(URL);
            

            CircleApiResponse? apiResponse = JsonConvert.DeserializeObject<CircleApiResponse>(httpResponse);

            if (apiResponse.STATUS == "3")
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.Message;
                return response;
            }
            if (apiResponse.STATUS == "1")
            {
                response.Status = Message.SUCCESS;
                response.Message = apiResponse.Message;
                response.Circle = apiResponse.Circle;
                response.Mobile = apiResponse.Mobile;
                response.Operator = apiResponse.Operator;

                return response;
            }

            return response;

        }

    }
}