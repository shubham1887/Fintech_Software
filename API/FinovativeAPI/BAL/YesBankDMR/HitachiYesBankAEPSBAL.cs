using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Newtonsoft.Json;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.AEPS.YesBankAEPS;
using Project.Response.AEPS;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL.YesBankDMR
{
    public class HitachiYesBankBAL
    {
        internal YesBank_AuthenticationResponse Authentication(YesBank_AuthenticationRequest mainRequest, int UserID, out string failureMessage)
        {
            YesBank_AuthenticationResponse response = new YesBank_AuthenticationResponse();
            response.Status = StatusType.Failure;
            failureMessage = mainRequest.IsValidModalState();

            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);

            string requestID = new Util().GetRandomNumberWithDatePrefix();
            if (requestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }

            try
            {
                YesBankCore.Response.AEPS.AuthenticationResponse resp = new Processing.YesBank.Hitachi_AEPSProcessing().Authentication(mainRequest, requestID, mainRequest.IPAddess, user);

                try
                {
                    if (resp.Status.EqualsIgnoreCase(StatusType.Success))
                    {
                        new DailyAuthenticationBal().Insert(UserID, (int)ServiceCode.AEPS, Company.YesBankProviderID, 0, "");
                    }
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                response.Status = resp.Status;
                response.Message = resp.Message;
                response.AuthReferenceNo = resp.ReferenceNo;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal AEPSViewModelResponse YesBank_CashWithdrawal(YesBank_CashWithdrawalRequest mainRequest, int UserID, string iPAddress, string userAgent, out string failureMessage)
        {
            AEPSViewModelResponse response = new AEPSViewModelResponse();
            response.Status = StatusType.Failure;

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            //1.Check for user exists
            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            failureMessage = Message.UserMessage.InvalidUser;

            if (user == null)
                return response;

            if (mainRequest.ConsumerNumber.Length <= 5)
                mainRequest.ConsumerNumber = user.MobileNumber;

            if (Company.YesBankProviderID <= 0)
            {
                response.Message = TransactionMessage.InvalidProviderID;
                failureMessage = response.Message;
                return response;
            }

            Response.AEPSSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new AEPSSettingDal().SelectAEPSByName(AEPSCode.YesBank);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            failureMessage = AEPSMessage.NotActive;
            if (viewModel == null)
                return response;

            int ServiceID = (int)ServiceCode.AEPS;
            int OperatorID = (int)OperatorCode.Withdraw_AEPS;

            int DBServiceID = new OperatorMasterBAL().SelectServiceID(OperatorID);

            if (ServiceID != DBServiceID)
            {
                failureMessage = Message.TransactionMessage.InvalidAEPSServiceCode;
                return response;
            }

            decimal tempAmount = mainRequest.Amount;

            if (OperatorID == (int)OperatorCode.Withdraw_AEPS && !isAEPSValidAmount(tempAmount, viewModel))
            {
                failureMessage = Message.TransactionMessage.InvalidAEPSAmountInBetween
                    .Replace("[From]", viewModel.MinimumAmount.ToString())
                    .Replace("[To]", viewModel.MaximumAmount.ToString());
                return response;
            }

            failureMessage = FunctionalValidationProcessing.FunctionalValidation(UserID, OperatorID, ServiceID, tempAmount, "", "", out decimal ServiceCharge);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;

                return response;
            }

            string requestID = new Util().GetRandomNumberWithDatePrefix();
            if (requestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }
            string Through = new BrowserUserAgentBal().GetThrough(userAgent);

            string maskedAadharNumber = mainRequest.AadhaarNumber.MaskedLastNDigit(4);

            failureMessage = new AEPSBal().AEPSTransactionInsert(user.UserID, maskedAadharNumber, ServiceID, OperatorID, iPAddress, tempAmount, requestID, "", Company.YesBankProviderID,
                Through, mainRequest.APIUserRequestID, mainRequest.BankName, mainRequest.ConsumerNumber);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            try
            {
                response.Status = StatusType.Process;

                YesBankCore.Response.AEPS.CashWithdrawalResponse resp = new Processing.YesBank.Hitachi_AEPSProcessing().Withdraw(mainRequest, iPAddress, requestID, user);

                TransactionMaster transaction = new TransactionDAL().SelectByProviderRequestID(requestID);
                response.Status = resp.Status;
                response.Message = resp.Message;
                response.AvailableBalance = resp.Balance;
                response.BankRRN = resp.BankRRN;
                response.BankName = mainRequest.BankName;
                response.Amount = mainRequest.Amount.TwoDigitDecimal();
                response.ConsumerNumber = mainRequest.ConsumerNumber;
                response.AadhaarNumber = maskedAadharNumber;
                response.TransactionID = resp.TransactionID;
                response.DOC = resp.DateTime;
                response.AgentID = resp.AgentID;
                response.AgentName = user.PersonName;
                response.VendorID = transaction.TransactionMasterID;
                response.Stan = resp.SysTraceNo;

                mainRequest.FingerPrint = "";
                mainRequest.AadhaarNumber = maskedAadharNumber;
                string ProviderRequest = JsonConvert.SerializeObject(mainRequest).ToString();
                string ProviderResponse = JsonConvert.SerializeObject(resp).ToString();

                new AEPSBal().ProviderRequestResponseLog(Company.YesBankProviderID, UserID, transaction.TransactionMasterID, ProviderRequest, ProviderResponse, iPAddress, Through);

                if (transaction == null)
                {
                    return response;
                }

                transaction.AEPSResponseJSON = resp.Balance;
                transaction.Status = StatusType.Process;
                transaction.ReferenceNo = resp.BankRRN;
                transaction.ProviderRefNo = resp.TransactionID;
                transaction.Bill_Optional1 = resp.SysTraceNo;
                transaction.Bill_Optional2 = resp.AgentID;
                transaction.Description = resp.Message;
                transaction.Bill_Optional3 = $"{resp.DeviceType}-{resp.DeviceModel}";

                if (resp.Status.EqualsIgnoreCase(StatusType.Failure))
                {
                    transaction.Status = StatusType.Failure;
                    failureMessage = response.Message;
                    return response;
                }
                if (resp.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    transaction.Status = StatusType.Success;
                }

                new AEPSBal().UpdateAEPS(transaction, user);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            failureMessage = StatusType.Ok;
            return response;
        }

        internal AEPSViewModelResponse YesBank_BalanceEnquiry(YesBank_BalanceEnquiryRequest mainRequest, int UserID, string iPAddress, string userAgent, out string failureMessage)
        {
            AEPSViewModelResponse response = new AEPSViewModelResponse();
            response.Status = StatusType.Failure;

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            //1.Check for user exists
            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            failureMessage = Message.UserMessage.InvalidUser;

            if (user == null)
                return response;

            if (mainRequest.ConsumerNumber.Length <= 5)
                mainRequest.ConsumerNumber = user.MobileNumber;

            if (Company.YesBankProviderID <= 0)
            {
                response.Message = TransactionMessage.InvalidProviderID;
                failureMessage = response.Message;
                return response;
            }

            Response.AEPSSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new AEPSSettingDal().SelectAEPSByName(AEPSCode.YesBank);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            failureMessage = AEPSMessage.NotActive;
            if (viewModel == null)
                return response;

            int ServiceID = (int)ServiceCode.AEPS;
            int OperatorID = (int)OperatorCode.Balance_AEPS;

            int DBServiceID = new OperatorMasterBAL().SelectServiceID(OperatorID);

            if (ServiceID != DBServiceID)
            {
                failureMessage = Message.TransactionMessage.InvalidAEPSServiceCode;
                return response;
            }

            decimal tempAmount = 0;

            failureMessage = FunctionalValidationProcessing.FunctionalValidation(UserID, OperatorID, ServiceID, tempAmount, "", "", out decimal ServiceCharge);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;

                return response;
            }

            string requestID = new Util().GetRandomNumberWithDatePrefix();
            if (requestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }
            string Through = new BrowserUserAgentBal().GetThrough(userAgent);

            string maskedAadharNumber = mainRequest.AadhaarNumber.MaskedLastNDigit(4);

            failureMessage = new AEPSBal().AEPSTransactionInsert(user.UserID, maskedAadharNumber, ServiceID, OperatorID, iPAddress, tempAmount, requestID, "", Company.YesBankProviderID,
                Through, mainRequest.APIUserRequestID, mainRequest.BankName, mainRequest.ConsumerNumber);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            try
            {
                response.Status = StatusType.Process;

                YesBankCore.Response.AEPS.CashWithdrawalResponse resp = new Processing.YesBank.Hitachi_AEPSProcessing().BalanceEnquiry(mainRequest, requestID, iPAddress, user);

                response.Status = resp.Status;
                response.Message = resp.Message;
                response.AvailableBalance = resp.Balance;
                response.BankRRN = resp.BankRRN;
                response.BankName = mainRequest.BankName;
                response.ConsumerNumber = mainRequest.ConsumerNumber;
                response.AadhaarNumber = maskedAadharNumber;
                response.TransactionID = resp.TransactionID;
                response.DOC = resp.DateTime;
                response.Stan = resp.SysTraceNo;
                response.AgentID = resp.AgentID;
                response.AgentName = user.PersonName;
                TransactionMaster transaction = new TransactionDAL().SelectByProviderRequestID(requestID);
                response.VendorID = transaction.TransactionMasterID;

                mainRequest.FingerPrint = "";
                mainRequest.AadhaarNumber = maskedAadharNumber;
                string ProviderRequest = JsonConvert.SerializeObject(mainRequest).ToString();
                string ProviderResponse = JsonConvert.SerializeObject(resp).ToString();

                new AEPSBal().ProviderRequestResponseLog(Company.YesBankProviderID, UserID, transaction.TransactionMasterID, ProviderRequest, ProviderResponse, iPAddress, Through);

                if (transaction == null)
                {
                    return response;
                }

                transaction.AEPSResponseJSON = resp.Balance;
                transaction.Status = StatusType.Process;
                transaction.ReferenceNo = resp.BankRRN;
                transaction.ProviderRefNo = resp.TransactionID;
                transaction.Bill_Optional1 = resp.SysTraceNo;
                transaction.Bill_Optional2 = resp.AgentID;
                transaction.Description = resp.Message;
                transaction.Bill_Optional3 = $"{resp.DeviceType}-{resp.DeviceModel}";

                if (resp.Status.EqualsIgnoreCase(StatusType.Failure))
                {
                    transaction.Status = StatusType.Failure;
                    failureMessage = response.Message;
                    return response;

                }
                if (resp.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    transaction.Status = StatusType.Success;
                }

                new AEPSBal().UpdateAEPS(transaction, user);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            failureMessage = StatusType.Ok;
            return response;
        }

        internal AEPSViewModelResponse YesBank_MiniStatement(YesBank_MinistatementRequest mainRequest, int UserID, string iPAddress, string userAgent, out string failureMessage)
        {
            AEPSViewModelResponse response = new AEPSViewModelResponse();
            response.Status = StatusType.Failure;

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            //1.Check for user exists
            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            failureMessage = Message.UserMessage.InvalidUser;

            if (user == null)
                return response;

            if (mainRequest.ConsumerNumber.Length <= 5)
                mainRequest.ConsumerNumber = user.MobileNumber;

            if (Company.YesBankProviderID <= 0)
            {
                response.Message = TransactionMessage.InvalidProviderID;
                failureMessage = response.Message;
                return response;
            }

            Response.AEPSSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new AEPSSettingDal().SelectAEPSByName(AEPSCode.YesBank);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            failureMessage = AEPSMessage.NotActive;
            if (viewModel == null)
                return response;

            int ServiceID = (int)ServiceCode.AEPS;
            int OperatorID = (int)OperatorCode.MiniStatement_AEPS;

            int DBServiceID = new OperatorMasterBAL().SelectServiceID(OperatorID);

            if (ServiceID != DBServiceID)
            {
                failureMessage = Message.TransactionMessage.InvalidAEPSServiceCode;
                return response;
            }

            decimal tempAmount = 0;

            failureMessage = FunctionalValidationProcessing.FunctionalValidation(UserID, OperatorID, ServiceID, tempAmount, "", "", out decimal ServiceCharge);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;

                return response;
            }

            failureMessage = new Project.Processing.YesBank.Hitachi_AEPSProcessing().isValidCapturedFingerPrint(mainRequest.FingerPrint);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;

                return response;
            }

            string requestID = new Util().GetRandomNumberWithDatePrefix();
            if (requestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }
            string Through = new BrowserUserAgentBal().GetThrough(userAgent);

            string maskedAadharNumber = mainRequest.AadhaarNumber.MaskedLastNDigit(4);

            failureMessage = new AEPSBal().AEPSTransactionInsert(user.UserID, maskedAadharNumber, ServiceID, OperatorID, iPAddress, tempAmount, requestID, "", Company.YesBankProviderID,
                Through, mainRequest.APIUserRequestID, mainRequest.BankName, mainRequest.ConsumerNumber);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            try
            {
                response.Status = StatusType.Process;

                YesBankCore.Response.AEPS.MiniStatementResponse resp = new Processing.YesBank.Hitachi_AEPSProcessing().MiniStatement(mainRequest, requestID, iPAddress, user);

                TransactionMaster transaction = new TransactionDAL().SelectByProviderRequestID(requestID);
                response.Status = resp.Status;
                response.Message = resp.Message;
                response.AvailableBalance = resp.Balance;
                response.BankName = mainRequest.BankName;
                response.BankRRN = resp.BankRRN;
                response.ConsumerNumber = mainRequest.ConsumerNumber;
                response.AadhaarNumber = maskedAadharNumber;
                response.DOC = resp.DateTime;
                response.AgentID = resp.AgentID;
                response.AgentName = user.PersonName;
                response.VendorID = transaction.TransactionMasterID;
                response.Stan = resp.SysTraceNo;
                response.TransactionID = resp.TransactionID;


                List<Transaction> transactionlist = ParseMinistatement(resp.MiniStatement);

                response.ministatements = new List<Response.AEPS.Ministatement>();

                if (transactionlist != null || transactionlist.Count > 0)
                {
                    foreach (var item in transactionlist)
                    {
                        Response.AEPS.Ministatement list = new Response.AEPS.Ministatement();
                        list.Amount = item.Amount.ToString();
                        list.Date = item.Date;
                        list.Narration = item.Narration;
                        list.TxnType = item.TxnType;
                        response.ministatements.Add(list);
                    }
                }

                mainRequest.FingerPrint = "";
                mainRequest.AadhaarNumber = maskedAadharNumber;
                string ProviderRequest = JsonConvert.SerializeObject(mainRequest).ToString();
                string ProviderResponse = JsonConvert.SerializeObject(resp).ToString();

                if (transaction == null)
                {
                    new AEPSBal().ProviderRequestResponseLog(Company.YesBankProviderID, UserID, 0, ProviderRequest, ProviderResponse, iPAddress, Through);
                    return response;
                }

                new AEPSBal().ProviderRequestResponseLog(Company.YesBankProviderID, UserID, transaction.TransactionMasterID, ProviderRequest, ProviderResponse, iPAddress, Through);
                transaction.AEPSResponseJSON = ProviderResponse;
                transaction.Status = StatusType.Process;
                transaction.ReferenceNo = resp.BankRRN;
                transaction.ProviderRefNo = resp.TransactionID;
                transaction.Bill_Optional1 = resp.SysTraceNo;
                transaction.Bill_Optional2 = resp.AgentID;
                transaction.Description = resp.Message;
                transaction.Bill_Optional3 = $"{resp.DeviceType}-{resp.DeviceModel}";

                if (resp.Status.EqualsIgnoreCase(StatusType.Failure))
                {
                    transaction.Status = StatusType.Failure;
                    failureMessage = response.Message;
                    return response;
                }
                if (resp.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    transaction.Status = StatusType.Success;
                }

                new AEPSBal().UpdateAEPS(transaction, user);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        private bool isAEPSValidAmount(decimal tempAmount, Response.AEPSSetting.ViewModel viewModel)
        {
            if (tempAmount <= viewModel.MinimumAmount)
            {
                return false;
            }
            if (tempAmount > viewModel.MaximumAmount)
            {
                return false;
            }
            return true;
        }

        public List<Transaction> ParseMinistatement(string ministatement)
        {
            List<Transaction> transactions = new List<Transaction>();

            // Split the ministatement by the '|' character
            string[] entries = ministatement.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string entry in entries)
            {

                if (entry.Contains("Balance"))
                    continue;
                string date = entry.Substring(0, 5);

                int lastSpaceIndex = entry.LastIndexOf(' ');

                string type = entry.Substring(lastSpaceIndex - 5, 1);


                //string description = entry.Substring(6, lastSpaceIndex - 7).Trim();
                string description = entry.Substring(6, 17).Trim();

                decimal amount = decimal.Parse(entry.Substring(lastSpaceIndex + 1));




                transactions.Add(new Transaction
                {
                    Date = date,
                    Narration = description,
                    TxnType = type,
                    Amount = amount
                });
            }

            return transactions;
        }

    }
}