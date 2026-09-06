using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.APILog;
using Project.Requests.Transaction;
using Project.Response.APILog;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class APIUserCallbackRequestResponseLogBal
    {

        internal void Insert(int TransactionMasterID, string APIUserRequestID, int UserID, string ConsumerNumber, string Url, string Responce)
        {
            Task.Run(() => InsertCore(TransactionMasterID, APIUserRequestID, UserID, ConsumerNumber, Url, Responce));
        }
        private void InsertCore(int TransactionMasterID, string APIUserRequestID, int UserID, string ConsumerNumber, string Url, string Responce)
        {
            APIUserCallbackRequestResponseLog ObjModel = new APIUserCallbackRequestResponseLog
            {
                TransactionMasterID = TransactionMasterID,
                APIUserRequestID = APIUserRequestID,
                UserID = UserID,
                ConsumerNumber = ConsumerNumber,
                Request = Url,
                Response = Responce ?? "",
                DOC = DateTime.Now,
                UpdatedDOC = DateTime.Now,
                DOCInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat)),
            };
            try
            {
                //int rowsAffected = new APIUserCallbackRequestResponseLogDal().Insert(ObjModel);
                int rowsAffected = new APIUserCallbackRequestResponseLogDal().Insert(ObjModel).Result;
                if (rowsAffected <= 0)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return;
            }
            return;
        }

        internal string InsertRange(List<APIUserCallbackRequestResponseLog> list)
        {
            try
            {
                int rowsAffected = new APIUserCallbackRequestResponseLogDal().InsertRange(list);
                //int rowsAffected = new APIUserCallbackRequestResponseLogDal().InsertRange(ObjModel).Result;
                if (rowsAffected <= 0)
                {
                    return Messages.InsertLogFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        //check
        internal APIUserWrapper SelectByFromDateToDate(SelectRequest mainRequest, int UserID)
        {
            APIUserWrapper wrapper = new APIUserWrapper();
            List<APIUserViewModel> modelObj = new List<APIUserViewModel>();
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                modelObj = new APIUserCallbackRequestResponseLogDal().SelectByFromDateToDate(mainRequest, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return wrapper;
            }

            if (modelObj == null || modelObj.Count <= 0)
            {
                return wrapper;
            }
            wrapper.Count = modelObj.Count();
            if (!mainRequest.IsExcelDownload)
            {
                modelObj = modelObj.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }
            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = modelObj;

            return wrapper;
        }

        internal List<APIUserCallbackViewModel> SelectByConsumerNumber(SearchRequest searchRequest, out string failureMessage)
        {
            failureMessage = searchRequest.IsValidModalState();

            List<APIUserCallbackViewModel> list = new List<APIUserCallbackViewModel>();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return list;

            try
            {
                list = new APIUserCallbackRequestResponseLogDal().SelectByConsumerNumber(searchRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }

            if (list == null || list.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return list;
            }
            failureMessage = StatusType.Ok;
            return list;
        }

        internal string ResendCallback(ResendCallbackRequest resendCallbackRequest)
        {
            try
            {
                Response.TransactionMaster.CallbackResponse model = new TransactionDAL().TransactionSelectForAPIUserCallbackByTransactionID(resendCallbackRequest.TransactionMasterID);

                if (model == null)
                    return Message.TransactionMessage.InvalidTransaction;

                if (model.UserTypeID != (int)UserType.APIUser)
                {
                    return APIUserMessage.NotAPIUser;
                }

                TransactionResponseProcessing providerRes = new TransactionResponseProcessing();

                if (!providerRes.IsValidForCallBack(model.Status))
                {
                    return APIUserMessage.TansactionInProcess;
                }

                new TransactionBal().UpdateHitBack(model.TransactionMasterID);

                string CallBackUrl = new CallbackMasterDal().SelectCallbackURL(model.UserID, model.ServiceID);

                Validation valid = new Validation();
                if (!valid.IsValidString(CallBackUrl))
                {
                    Insert(model.TransactionMasterID, model.APIUserRequestID, model.UserID, model.ConsumerNumber, Messages.NotSetCallbackUrl, "");
                    return Messages.NotSetCallbackUrl;
                }

                string Url = "[CallBackUrl]?RequestID=[APIUserRequestID]&ReferenceNo=[ReferenceNo]&Status=[Status]&Number=[ConsumerNumber]&Amount=[Amount]&Balance=[ClosingBalance]";

                Url = Url.Replace("[CallBackUrl]", CallBackUrl.TrimIfNotNull());
                Url = Url.Replace("[APIUserRequestID]", model.APIUserRequestID.TrimIfNotNull());
                Url = Url.Replace("[ReferenceNo]", model.ReferenceNo.TrimIfNotNull());
                Url = Url.Replace("[Status]", model.Status.TrimIfNotNull());
                Url = Url.Replace("[ConsumerNumber]", model.ConsumerNumber.TrimIfNotNull());
                Url = Url.Replace("[Amount]", model.Amount.TwoDigitDecimal());
                Url = Url.Replace("[ClosingBalance]", model.ClosingBalance.TwoDigitDecimal());
                Post(model.TransactionMasterID, model.APIUserRequestID, model.UserID, model.ConsumerNumber, Url);
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return StatusType.Ok;
        }

        private void Post(int TransactionMasterID, string APIUserRequestID, int UserID, string ConsumerNumber, string Url)
        {
            string message = HttpProcessing.CallGetMethod(Url, 30);

            Insert(TransactionMasterID, APIUserRequestID, UserID, ConsumerNumber, Url, message);
        }

        internal List<APIUserCallbackViewModel> SelectCallBackByUserIDAndConsumerNumber(SearchRequest searchRequest, int UserID, out string failureMessage)
        {
            failureMessage = searchRequest.IsValidModalState();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<APIUserCallbackViewModel> list = new List<APIUserCallbackViewModel>();
            if (UserTypeID != (int)UserType.APIUser)
            {
                failureMessage = Messages.InvalidUser;
                return list;
            }
            try
            {
                list = new APIUserCallbackRequestResponseLogDal().SelectCallBackByUserIDAndConsumerNumber(searchRequest, UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }

            if (list == null || list.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return list;
            }
            failureMessage = StatusType.Ok;
            return list;
        }
    }
}