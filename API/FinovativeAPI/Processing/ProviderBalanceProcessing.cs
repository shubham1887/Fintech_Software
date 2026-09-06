using Project.BAL;
using Project.DAL;
using Project.Model;
using Project.Response.Provider;
using Project.Response.ProviderResponse;
using Project.StaticModel;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class ProviderBalanceProcessing
    {
        internal void ProviderBalanceCheck(Response.Provider.ViewModel providerMaster, DetailsViewModel objModel, BalanceViewModel response)
        {
            string HttpResponse = string.Empty;
            string urlWithParam = "";



            if (providerMaster.ProviderID == Company.YesBankProviderID)
            {
                YesBank.Processing yesMoney = new YesBank.Processing();
                YesBankCore.Response.BalanceResponse yesbankResp = yesMoney.GetBalance();

                if (!yesbankResp.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    return;
                }

                decimal APIBalance = Convert.ToDecimal(yesbankResp.Balance);
                response.RechargeBalance = APIBalance;
                response.DMTBalance = APIBalance;


                objModel = new DetailsViewModel();
                objModel.ProviderID = providerMaster.ProviderID;
                objModel.ServiceID = 0;

            }
            else
            {
                objModel.ProviderID = providerMaster.ProviderID;
                try
                {
                    urlWithParam = new ProviderProcessing().URLFormatting(methodType: objModel.MethodType, baseURL: objModel.ProviderURL, OpCode: "", ConsumerNumber: "",
                                                     Amount: 0, ServiceID: 0, RequestID: "", Type: "",
                                                     RefID: "", DateStr: "", LoginID: providerMaster.LoginID, Token: providerMaster.Token);

                    if (objModel.MethodType.EqualsIgnoreCase("GET"))
                    {
                        HttpResponse = HttpProcessing.CallGetMethod(urlWithParam, 10);
                    }
                    else
                    {
                        HttpResponse = HttpProcessing.CallPostMethod(urlWithParam, objModel.HeaderJson);
                    }
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                ExtractBalance(HttpResponse, objModel.ProviderID, response);
            }

            ProviderRequestResponseLog providerRequestResponseLog = new ProviderRequestResponseLog
            {
                ProviderID = objModel.ProviderID,
                UserID = Company.AdminUserID,
                TransactionID = 0,
                ServiceID = objModel.ServiceID,
                Request = urlWithParam,
                Response = HttpResponse,
                IPAddress = "Balance check",
                Through = PlatformCode.Web
            };

            string RechargeBalance = response.RechargeBalance.ToString();
            string DMTBalance = response.DMTBalance.ToString();

            Task.Run(() => new ProviderMasterBAL().UpdateProviderBalance(objModel.ProviderID, RechargeBalance, DMTBalance));
            Task.Run(() => new ProviderRequestResponseLogDal().Insert(providerRequestResponseLog));
        }

        public void ExtractBalance(string HttpResponse, int ProviderID, BalanceViewModel response)
        {
            try
            {
                int logID = 0;
                ProviderReturnResponse providerResponse = new ProviderReturnResponse();
                TransactionResponseProcessing PRP = new TransactionResponseProcessing();
                PRP.ExtractResponseFromText(HttpResponse, "BalanceCheck", ProviderID, "", providerResponse);

                if (providerResponse == null)
                {
                    return;
                }

                decimal RechargeBalance = 0;
                decimal DMTBalance = 0;
                try
                {
                    RechargeBalance = decimal.Parse(providerResponse.Balance, CultureInfo.InvariantCulture);
                }
                catch
                {
                    RechargeBalance = 0;
                }
                try
                {
                    DMTBalance = decimal.Parse(providerResponse.DMTBalance, CultureInfo.InvariantCulture);
                }
                catch
                {
                    DMTBalance = 0;
                }

                response.RechargeBalance = RechargeBalance;
                response.DMTBalance = DMTBalance;
            }
            catch (Exception)
            {
            }
        }
    }
}