using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Requests.Transaction;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class DMTStatusCheckProcessing
    {
        public void StatusCheckForNEFT()
        {
            try
            {
                bool isDevelopment = new Util().isDevelopment();
                if (isDevelopment == true)
                {
                    return;
                }

                string failureMessage = new Validation().ServerDownTime();

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return; // "At ServerDownTime, status check not allowed";
                }

                failureMessage = new Validation().BankDownTime();

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return; // "At BankDownTime, mostly bank down at night
                }

                List<PendingResponse> list = null;
                try
                {
                    list = new TransactionDAL().SelectDMTNEFTPendingTransaction();
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return;
                }

                if (list == null)
                    return;

                if (list.Count <= 0)
                    return;

                List<PendingResponse> randomLast20 = list.OrderBy(x => Guid.NewGuid()).Take(30).ToList();

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(randomLast20.AsEnumerable(), parallelOptions, item =>
                {
                    try
                    {
                        double seconds = DateTimeExtensions.DifferenceInSeconds(item.OriginalDOC);

                        double ExpectedSecond = 1800.0D; //30 min = 30*60

                        if (seconds <= ExpectedSecond)
                        {
                            return;
                        }

                        TransactionMaster transactionMaster = new TransactionDAL().SelectByID(item.TransactionMasterID);

                        if (transactionMaster == null)
                        {
                            return;
                        }

                        string faileureMessage = new TransactionBal().isValidForBulkStatusCheck(transactionMaster);

                        if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                            return;

                        DMTTransctionStatusRequest mainRequest = new DMTTransctionStatusRequest();
                        mainRequest.ID = transactionMaster.TransactionMasterID.ToString();
                        mainRequest.IsAutoStatusCheck = true;
                        DMTTransactionStatusCheck(mainRequest, Company.AdminUserID);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                }
                );
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        public void StatusCheckForIMPS()
        {
            try
            {
                bool isDevelopment = new Util().isDevelopment();
                if (isDevelopment)
                {
                    return;
                }

                string failureMessage = new Validation().ServerDownTime();

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return; // "At ServerDownTime, status check not allowed";
                }

                failureMessage = new Validation().BankDownTime();

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return; // "At BankDownTime, mostly bank down at night
                }

                List<PendingResponse> list = null;
                try
                {
                    list = new TransactionDAL().SelectDMTIMPSPendingTransaction();
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (list == null)
                    return;

                if (list.Count <= 0)
                    return;

                List<PendingResponse> randomLast80 = list.OrderBy(x => Guid.NewGuid()).Take(80).ToList();

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(randomLast80.AsEnumerable(), parallelOptions, item =>
                {
                    try
                    {
                        double seconds = DateTimeExtensions.DifferenceInSeconds(item.OriginalDOC);

                        double ExpectedSecond = 300.0D; //5 min = 5 * 60

                        if (seconds <= ExpectedSecond)
                        {
                            return;
                        }

                        TransactionMaster transactionMaster = new TransactionDAL().SelectByID(item.TransactionMasterID);

                        if (transactionMaster == null)
                        {
                            return;
                        }

                        string faileureMessage = new TransactionBal().isValidForBulkStatusCheck(transactionMaster);
                        new Logger().Info("StatusCheckJOB", "IMPS", $"{item.ConsumerNumber}, {item.ProviderRequestID}, {faileureMessage}");

                        if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                            return;

                        DMTTransctionStatusRequest mainRequest = new DMTTransctionStatusRequest();
                        mainRequest.ID = transactionMaster.TransactionMasterID.ToString();
                        mainRequest.IsAutoStatusCheck = true;

                        DMTTransactionResponse resp = DMTTransactionStatusCheck(mainRequest, Company.AdminUserID);

                        new Logger().Info("StatusCheckJOB", "After IMPS", $"{item.ConsumerNumber}, {item.ProviderRequestID}, {resp.Status} {resp.Message}");

                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                }
                );
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal DMTTransactionResponse DMTTransactionStatusCheck(DMTTransctionStatusRequest mainRequest, int UserID)
        {
            DMTTransactionResponse resp = new DMTTransactionResponse();

            string failureMessage = mainRequest.IsValidModalState();
            resp.Status = StatusType.Failure;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Message = failureMessage;
                return resp;
            }

            try
            {
                TransactionMaster model = null;
                model = new TransactionDAL().SelectByID(Convert.ToInt32(mainRequest.ID));

                if (model == null)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "No transaction found";
                    return resp;
                }

                if (model.IsProviderLowBalance == true)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "Transaction not allowed for status check";
                    return resp;
                }

                if (!new ResponseProcess().checkProcess(model.Status))
                {
                    resp.Status = model.Status;
                    resp.Message = $"Transaction already {model.Status}";
                    return resp;
                }

                if (model.ProviderRequestID == null || model.ProviderRequestID.Trim().Length <= 0)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "No request id found";
                    return resp;
                }

                failureMessage = new DMTProcessing().CheckIsOnlineDMTForStatusCheck(model);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    resp.Status = StatusType.Process;
                    resp.Message = failureMessage;
                    return resp;
                }

                if (!model.IsOnlineDMT && model.Description.ToLower().Contains("Offline DMT".ToLower()))
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "Transaction is not allowed for status check";
                    return resp;
                }

                failureMessage = new DMTProcessing().isValidDMTTranactionDelayForStatusCheck(model);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    resp.Status = StatusType.Process;
                    resp.Message = failureMessage;
                    return resp;
                }

                resp.Status = StatusType.Process;

                if (model.ProviderID == Company.YesBankProviderID)
                {
                    YesBank.MoneyTransferProcessing proc = new YesBank.MoneyTransferProcessing();

                    YesBankCore.Response.MoneyTransfer.TransferResponse resStatusCheck = proc.GetTransctionStatus(model);
                    if (resStatusCheck == null)
                    {
                        resp.Message = TransactionMessage.InvalidRequestID;
                        return resp;
                    }
                    resp.Status = resStatusCheck.Status;
                    resp.Message = resStatusCheck.Message;

                    new DMTProcessing().UpdateYesBankDMTStatus(resStatusCheck, model.TransactionMasterID, resp);
                }

                Task.Run(() => new TransactionSmsPosting().select());
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                resp.Status = StatusType.Process;
                resp.Message = Messages.ExceptionOccured;
                return resp;
            }

            resp.Status = StatusType.Ok;
            return resp;
        }

        private bool IsValidForStatusCheckNovity(bool IsAutoStatusCheck, TransactionMaster transactionMaster)
        {
            if (!IsAutoStatusCheck)
            {
                return true;
            }

            if (transactionMaster.Bill_Optional1 == null || transactionMaster.Bill_Optional1.Length <= 0)
                return true;

            if (transactionMaster.Bill_Optional1.ToLower().Contains("OTP sent to customer mobile".ToLower()))
            {
                return false;
            }

            return true;
        }
    }
}