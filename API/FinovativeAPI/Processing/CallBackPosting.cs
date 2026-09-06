using Project.BAL;
using Project.Model;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class CallBackPosting
    {
        private static object lockObj = new object();
        private List<APIUserCallbackRequestResponseLog> listLog = new List<APIUserCallbackRequestResponseLog>();

        public void select()
        {
            if (Monitor.TryEnter(lockObj, 0))
            {
                try
                {
                    SendCallback();
                    new APIUserCallbackRequestResponseLogBal().InsertRange(listLog);
                }
                finally
                {
                    Monitor.Exit(lockObj);
                }
            }
        }

        private void SendCallback()
        {
            RandomNumberProcessing.RandomDelay(2, 30);
            try
            {
                List<CallbackResponse> listResponse = new List<CallbackResponse>();
                listResponse = new TransactionBal().TransactionSelectForAPIUserCallback();

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                if (listResponse == null)
                    return;

                if (listResponse.Count <= 0)
                    return;

                List<CallbackResponse> randomLast50 = listResponse.OrderBy(x => Guid.NewGuid()).Take(50).ToList();

                Parallel.ForEach(randomLast50.AsEnumerable(), parallelOptions, singleTxn =>
                    {
                        TransactionMaster model = new TransactionMaster();
                        model.TransactionMasterID = singleTxn.TransactionMasterID;
                        model.APIUserRequestID = singleTxn.APIUserRequestID;
                        model.Status = singleTxn.Status;
                        model.ConsumerNumber = singleTxn.ConsumerNumber;
                        model.Amount = singleTxn.Amount;
                        model.ReferenceNo = singleTxn.ReferenceNo;
                        model.ServiceID = singleTxn.ServiceID;
                        model.UserID = singleTxn.UserID;
                        model.ProviderRequestID = singleTxn.ProviderRequestID;
                        model.ClosingBalance = singleTxn.ClosingBalance;

                        string CallBackUrl = new DAL.CallbackMasterDal().SelectCallbackURL(model.UserID, model.ServiceID);
                        bulk_postCallBackToAPIUser(model, singleTxn.UserTypeID, CallBackUrl);
                    }

                );
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        private void bulk_postCallBackToAPIUser(TransactionMaster model, int UserTypeID, string CallBackUrl)
        {
            try
            {
                TransactionResponseProcessing providerRes = new TransactionResponseProcessing();

                if (!providerRes.IsValidForCallBack(model.Status))
                {
                    return;
                }

                if (UserTypeID != (int)UserType.APIUser)
                {
                    return;
                }
                new TransactionBal().UpdateHitBack(model.TransactionMasterID);

                APIUserCallbackRequestResponseLog item = new APIUserCallbackRequestResponseLog();
                item.TransactionMasterID = model.TransactionMasterID;
                item.APIUserRequestID = model.APIUserRequestID;
                item.UserID = model.UserID;
                item.ConsumerNumber = model.ConsumerNumber;

                Validation valid = new Validation();
                if (!valid.IsValidString(CallBackUrl))
                {
                    item.Request = Messages.NotSetCallbackUrl;
                    item.Response = "";
                    listLog.Add(item);

                    return;
                }

                string Url = getURL(model, CallBackUrl);

                string message = HttpProcessing.CallGetMethod(Url, 30);

                item.Request = Url;
                item.Response = message;

                listLog.Add(item);
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
        }

        public static void postCallBackToAPIUser(TransactionMaster model, int UserTypeID, string CallBackUrl)
        {
            try
            {
                TransactionResponseProcessing providerRes = new TransactionResponseProcessing();

                if (!providerRes.IsValidForCallBack(model.Status))
                {
                    return;
                }

                if (UserTypeID != (int)UserType.APIUser)
                {
                    return;
                }

                new TransactionBal().UpdateHitBack(model.TransactionMasterID);

                Validation valid = new Validation();
                if (!valid.IsValidString(CallBackUrl))
                {
                    new APIUserCallbackRequestResponseLogBal().Insert(model.TransactionMasterID, model.APIUserRequestID,
                       model.UserID, model.ConsumerNumber, Messages.NotSetCallbackUrl, "");
                    return;
                }
                string Url = getURL(model, CallBackUrl);
                Post(model.TransactionMasterID, model.APIUserRequestID, model.UserID, model.ConsumerNumber, Url);
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
        }

        private static string getURL(TransactionMaster model, string CallBackUrl)
        {
            string Url = "[CallBackUrl]?RequestID=[APIUserRequestID]&ReferenceNo=[ReferenceNo]&Status=[Status]&Number=[ConsumerNumber]&Amount=[Amount]&Balance=[ClosingBalance]";

            Url = Url.Replace("[CallBackUrl]", CallBackUrl.TrimIfNotNull());
            Url = Url.Replace("[APIUserRequestID]", model.APIUserRequestID.TrimIfNotNull());
            Url = Url.Replace("[ReferenceNo]", model.ReferenceNo.TrimIfNotNull());
            Url = Url.Replace("[Status]", model.Status.TrimIfNotNull());
            Url = Url.Replace("[ConsumerNumber]", model.ConsumerNumber.TrimIfNotNull());
            Url = Url.Replace("[Amount]", model.Amount.TwoDigitDecimal());
            Url = Url.Replace("[ClosingBalance]", model.ClosingBalance.TwoDigitDecimal());
            return Url;
        }

        private static void Post(int TransactionMasterID, string APIUserRequestID, int UserID, string ConsumerNumber, string Url)
        {
            string message = HttpProcessing.CallGetMethod(Url, 30);
            new APIUserCallbackRequestResponseLogBal().Insert(TransactionMasterID, APIUserRequestID, UserID, ConsumerNumber, Url, message);
        }
    }
}