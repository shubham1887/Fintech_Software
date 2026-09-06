using Newtonsoft.Json;
using System.Text.RegularExpressions;
using YesBankCore.Controller;
using YesBankCore.Request;
using YesBankCore.Response;
using YESBankCore.Controller;
using YESBankCore.Model;
using YESBankCore.Response;

namespace YesBankCore
{
    public class YesBankPayout
    {
        public void SetCredential(string MerchantId, string MerchantKey,
            string IBMClientID, string XIBMClientSecret,
            string PrivateKeyPassword, string PrivateKeyFilePath,
            string UserName, string Password, string Mode)
        {
            PayoutCredential.MerchantId = MerchantId;
            PayoutCredential.MerchantKey = MerchantKey;

            PayoutCredential.XIBMClientID = IBMClientID;
            PayoutCredential.XIBMClientSecret = XIBMClientSecret;

            PayoutCredential.PrivateKeyPassword = PrivateKeyPassword;

            string[] arr = PrivateKeyFilePath.Split(',');
            PayoutCredential.PrivateKeyFilePath = arr[0].Trim();

            PayoutCredential.AuthUserName = UserName;
            PayoutCredential.AuthPassword = Password;

            PayoutCredential.Mode = Mode;

        }

        public PayoutResponse DomesticPayment(PayoutRequest input)
        {
            PayoutResponse response = new PayoutResponse();
            response.Status = Message.FAILURE;
            response.Amount = input.amount;
            response.RequestId = input.orderID;
            response.AccountNumber = input.accountNumber;
            response.Message = "";

            try
            {
                PayoutCredential cred = new PayoutCredential();
                string failureMessage = cred.IsValidModalState();

                if (!failureMessage.EqualsIgnoreCase(Message.OK))
                {
                    response.Status = Message.FAILURE;
                    response.Message = failureMessage;
                    return response;
                }

                failureMessage = input.IsValidModalState();

                if (!failureMessage.EqualsIgnoreCase(Message.OK))
                {
                    response.Status = Message.FAILURE;
                    response.Message = failureMessage;
                    return response;
                }

                PayoutAPIRequest apiInput = new PayoutAPIRequest();
                apiInput.Data = new Data();
                apiInput.Data.ConsentId = PayoutCredential.MerchantId;

                apiInput.Data.Initiation = new Initiation();
                apiInput.Data.Initiation.OrderID = input.orderID;
                apiInput.Data.Initiation.EndToEndIdentification = "";
                apiInput.Data.Initiation.ClearingSystemIdentification = input.transactionMode;

                apiInput.Data.Initiation.InstructedAmount = new InstructedAmount();
                apiInput.Data.Initiation.InstructedAmount.Amount = input.amount;
                apiInput.Data.Initiation.InstructedAmount.Currency = "INR";

                apiInput.Data.Initiation.DebtorAccount = new DebtorAccount();
                apiInput.Data.Initiation.DebtorAccount.Identification = PayoutCredential.MerchantKey;
                apiInput.Data.Initiation.DebtorAccount.SecondaryIdentification = PayoutCredential.MerchantId;

                apiInput.Data.Initiation.CreditorAccount = new CreditorAccount();
                apiInput.Data.Initiation.CreditorAccount.SchemeName = input.beneIfscCode;
                apiInput.Data.Initiation.CreditorAccount.Identification = input.accountNumber;
                apiInput.Data.Initiation.CreditorAccount.Name = new Validation().GetValidName(input.customerName);

                apiInput.Data.Initiation.CreditorAccount.Unstructured = new CreditorAccountUnstructured();
                apiInput.Data.Initiation.CreditorAccount.Unstructured.ContactInformation = new ContactInformation();
                apiInput.Data.Initiation.CreditorAccount.Unstructured.ContactInformation.EmailAddress = input.email;
                apiInput.Data.Initiation.CreditorAccount.Unstructured.ContactInformation.MobileNumber = input.customerMobileNumber;

                apiInput.Data.Initiation.RemittanceInformation = new RemittanceInformation();
                apiInput.Data.Initiation.RemittanceInformation.Reference = $"{input.orderID} for {input.accountNumber}";

                apiInput.Data.Initiation.RemittanceInformation.Unstructured = new RemittanceInformationUnstructured();
                apiInput.Data.Initiation.RemittanceInformation.Unstructured.CreditorReferenceInformation = removeSpeicalChar(input.orderID);

                string pincode = input.pincode ?? "400001";

                apiInput.Risk = new Risk();
                apiInput.Risk.DeliveryAddress = new DeliveryAddress();
                apiInput.Risk.DeliveryAddress.AddressLine = new List<string> { "AddressLine1" };
                apiInput.Risk.DeliveryAddress.StreetName = "StreetName";
                apiInput.Risk.DeliveryAddress.BuildingNumber = "BuildingNumber";
                apiInput.Risk.DeliveryAddress.PostCode = string.IsNullOrWhiteSpace(pincode) ? "400001" : pincode;
                apiInput.Risk.DeliveryAddress.TownName = "TownName";
                apiInput.Risk.DeliveryAddress.CountySubDivision = new List<string> { "MH" };
                apiInput.Risk.DeliveryAddress.Country = "IN";

                PayoutHTTPProcessing http = new PayoutHTTPProcessing();

                string JSONText = JsonConvert.SerializeObject(apiInput);

                response.Status = Message.PROCESS;
                NetworkResponse network = http.Post(PayoutURL.getURL(PayoutURL.DomesticPayment), JSONText, input.orderID);

                if (!network.Status.EqualsIgnoreCase(Message.OK))
                {
                    response.Status = Message.PROCESS;
                    response.Message = network.Response;
                    return response;
                }

                PayoutAPIResponse? apiResponse = JsonConvert.DeserializeObject<PayoutAPIResponse>(network.Response);
                if (apiResponse.Data == null)
                {
                    response.Message = Message.PROCESS;
                    return response;
                }

                if (apiResponse.Data.Status.EqualsIgnoreCase("SettlementInProcess") ||
                    apiResponse.Data.Status.EqualsIgnoreCase("Pending"))
                {
                    response.Message = apiResponse.Meta?.ErrorSeverity ?? "";
                    return response;
                }

                if (apiResponse.Data.Status.EqualsIgnoreCase("FAILED") ||
                    apiResponse.Data.Status.EqualsIgnoreCase("Duplicate") ||
                    apiResponse.Data.Status.EqualsIgnoreCase("SettlementReversed"))
                {
                    response.Status = Message.FAILURE;
                    response.Message = apiResponse.Meta?.ErrorSeverity ?? "";
                    return response;
                }

                if (apiResponse.Data.Status.EqualsIgnoreCase("SettlementCompleted"))
                {
                    response.Status = Message.SUCCESS;
                    response.Message = apiResponse.Data?.Status ?? "";
                    response.BankRRN = apiResponse.Data?.Initiation?.BankRRN ?? "";
                    response.BeneficiaryName = apiResponse.Data?.Initiation?.CreditorAccount?.Name ?? "";
                    return response;
                }

                if (apiResponse.Data.Status.EqualsIgnoreCase("Received"))
                {
                    Thread.Sleep(15 * 1000);// delay for 15 second

                    PayoutStatusCheckRequest statusInput = new PayoutStatusCheckRequest();

                    statusInput.orderID = input.orderID;

                    PayoutResponse statusRepose = StatusCheck(statusInput);

                    if (statusRepose.Status.EqualsIgnoreCase(Message.SUCCESS))
                    {
                        response.Status = Message.SUCCESS;
                        response.Message = statusRepose.Message;
                        response.BankRRN = statusRepose.BankRRN;
                        response.BeneficiaryName = statusRepose.BeneficiaryName;
                    }


                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        private string removeSpeicalChar(string inString)
        {
            string result = Regex.Replace(inString, "[^a-zA-Z0-9 ]", "");
            return result;
        }

        private string getAmount(string numberString)
        {
            try
            {
                double result = double.Parse(numberString, System.Globalization.NumberStyles.Float);
                return result.ToString();
            }
            catch (FormatException)
            {
                return "";
            }
        }

        public PayoutResponse StatusCheck(PayoutStatusCheckRequest input)
        {
            PayoutResponse response = new PayoutResponse();
            response.Status = Message.PROCESS;
            response.Message = "";
            response.RequestId = input.orderID;
            response.AccountNumber = input.accountNumber ?? "";

            try
            {
                PayoutCredential cred = new PayoutCredential();
                string failureMessage = cred.IsValidModalState();

                if (!failureMessage.EqualsIgnoreCase(Message.OK))
                {
                    response.Status = Message.PROCESS;
                    response.Message = failureMessage;
                    return response;
                }

                failureMessage = input.IsValidModalState();

                if (!failureMessage.EqualsIgnoreCase(Message.OK))
                {
                    response.Status = Message.PROCESS;
                    response.Message = failureMessage;
                    return response;
                }

                StatusCheckAPIRequest apiInput = new StatusCheckAPIRequest();
                apiInput.Data.ConsentId = PayoutCredential.MerchantId;
                apiInput.Data.SecondaryIdentification = PayoutCredential.MerchantId;

                apiInput.Data.InstrId = input.orderID;
                PayoutHTTPProcessing http = new PayoutHTTPProcessing();
                string JSONText = JsonConvert.SerializeObject(apiInput);

                NetworkResponse network = http.Post(PayoutURL.getURL(PayoutURL.PaymentStatus), JSONText, input.orderID);
                if (!network.Status.EqualsIgnoreCase(Message.OK))
                {
                    response.Status = Message.PROCESS;
                    response.Message = network.Response;
                    return response;
                }

                PayoutAPIResponse? apiResponse = JsonConvert.DeserializeObject<PayoutAPIResponse>(network.Response);
                if (apiResponse.Data == null)
                {
                    response.Message = Message.PROCESS;
                    return response;
                }

                response.Amount = getAmount(apiResponse.Data?.Initiation?.InstructedAmount?.Amount ?? "0");
                response.AccountNumber = apiResponse.Data?.Initiation?.CreditorAccount?.Identification ?? "";
                response.BankRRN = apiResponse.Data?.Initiation?.BankRRN ?? "";


                if (apiResponse.Data.Status.EqualsIgnoreCase("SettlementInProcess") ||
                    apiResponse.Data.Status.EqualsIgnoreCase("Pending"))
                {
                    response.Message = apiResponse.Meta?.ErrorSeverity ?? "";
                    return response;
                }
               if(apiResponse.Data.Status.EqualsIgnoreCase("FAILED") && apiResponse.Meta.ErrorCode.EqualsIgnoreCase("ns:E307"))
                {
                    response.Message = apiResponse.Meta?.ErrorSeverity ?? "";
                    return response;
                }

                if (apiResponse.Data.Status.EqualsIgnoreCase("FAILED") ||
                    apiResponse.Data.Status.EqualsIgnoreCase("SettlementReversed"))
                {
                    response.Status = Message.FAILURE;
                    response.Message = apiResponse.Meta?.ErrorSeverity ?? "";
                    return response;
                }

                if (apiResponse.Data.Status.EqualsIgnoreCase("SettlementCompleted"))
                {
                    response.Status = Message.SUCCESS;
                    response.Message = apiResponse.Data?.Status ?? "";
                    response.BeneficiaryName = apiResponse.Data?.Initiation?.CreditorAccount?.Name ?? "";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public BalanceResponse GetBalance()
        {
            BalanceResponse response = new BalanceResponse();
            response.Status = Message.FAILURE;
            response.Message = "";

            try
            {
                PayoutCredential cred = new PayoutCredential();
                string failureMessage = cred.IsValidModalState();

                if (!failureMessage.EqualsIgnoreCase(Message.OK))
                {
                    response.Status = Message.FAILURE;
                    response.Message = failureMessage;
                    return response;
                }

                BalanceAPIRequest apiInput = new BalanceAPIRequest();
                apiInput.Data.DebtorAccount.ConsentId = PayoutCredential.MerchantId;
                apiInput.Data.DebtorAccount.SecondaryIdentification = PayoutCredential.MerchantId;

                apiInput.Data.DebtorAccount.Identification = PayoutCredential.MerchantKey;

                PayoutHTTPProcessing http = new PayoutHTTPProcessing();

                string JSONText = JsonConvert.SerializeObject(apiInput);

                NetworkResponse network = http.Post(PayoutURL.getURL(PayoutURL.AccountBalance), JSONText, "Balance");

                if (network.Status.EqualsIgnoreCase(Message.PROCESS))
                {
                    response.Status = Message.FAILURE;
                    response.Message = network.Response;
                    return response;
                }

                BalanceAPIResponse? apiResponse = JsonConvert.DeserializeObject<BalanceAPIResponse>(network.Response);
                if (apiResponse.Code != null && apiResponse.Code.Length >= 1)
                {
                    response.Status = Message.FAILURE;
                    response.Message = apiResponse.ActionDescription;
                    return response;
                }

                response.Status = Message.SUCCESS;
                response.Balance = getAmount(apiResponse?.Data?.FundsAvailableResult?.BalanceAmount ?? "0");
                response.Message = Message.SUCCESS;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}