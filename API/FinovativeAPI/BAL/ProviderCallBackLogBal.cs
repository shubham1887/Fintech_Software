using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Transaction;
using Project.Response.ProviderCallBackResponse;
using Project.Response.ProviderRequestResponseLogResponse;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Project.Requests.APILog;

namespace Project.BAL
{
    public class ProviderCallBackLogBal
    {
        internal void UpdateTansactionID(int logID, int TransactionMasterID)
        {
            try
            {
                ProviderCallBackLog obj = new ProviderCallBackLogDal().SelectByID(logID);
                if (obj == null)
                    return;

                obj.TransactionMasterID = TransactionMasterID;
                Task.Run(() => new ProviderCallBackLogDal().Update(obj));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal int Insert(int providerID, string response)
        {
            int logID = 0;

            ProviderCallBackLog Model = new ProviderCallBackLog
            {
                ProviderID = providerID,
                ProviderReply = response,
                Type = TransactionMessage.Callback,
            };

            try
            {
                logID = new ProviderCallBackLogDal().Insert(Model);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return logID;
        }

        internal void Update(int LogID, string response)
        {
            try
            {
                ProviderCallBackLog obj = new ProviderCallBackLogDal().SelectByID(LogID);
                if (obj == null)
                    return;

                obj.ProviderReply = response;
                Task.Run(() => new ProviderCallBackLogDal().Update(obj));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal List<ProviderCallbackViewModel> SelectByConsumerNumber(SearchRequest searchRequest, out string failureMessage)
        {
            List<ProviderCallbackViewModel> providerCallbackViewModel = new List<ProviderCallbackViewModel>();

            failureMessage = searchRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return providerCallbackViewModel;
            }

            try
            {
                providerCallbackViewModel = new ProviderCallBackLogDal().SelectByConsumerNumber(searchRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return providerCallbackViewModel;
            }

            if (providerCallbackViewModel.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return providerCallbackViewModel;
            }

            failureMessage = StatusType.Ok;
            return providerCallbackViewModel;
        }

        internal List<ProviderCallbackViewModel> SelectByTansactionMasterID(SelectByIDRequest selectByIDRequest, out string failureMessage)
        {
            List<ProviderCallbackViewModel> viewmodel = new List<ProviderCallbackViewModel>();
            failureMessage = selectByIDRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return viewmodel;
            }

            try
            {
                viewmodel = new ProviderCallBackLogDal().SelectByTansactionMasterID(selectByIDRequest.ID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return viewmodel;
            }
            failureMessage = Messages.DataNotFound;
            if (viewmodel == null)
                return viewmodel;

            failureMessage = StatusType.Ok;
            return viewmodel;
        }

        public bool IsValidURI(string uri)
        {
            Uri tmp;
            if (!Uri.TryCreate(uri, UriKind.Absolute, out tmp))
            {
                return false;
            }
            return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
        }

        internal ReportWrapper SelectByFromDateTodate(SelectRequest selectRequest, int userID, out string failureMessage)
        {
            List<RequestResponseViewModel> viewmodel = null;
            ReportWrapper Wrapper = new ReportWrapper();

            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return Wrapper;
            }
            try
            {
                viewmodel = new ProviderCallBackLogDal().SelectByFromDateTodate(selectRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return Wrapper;
            }

            if (viewmodel == null || viewmodel.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return Wrapper;
            }
            viewmodel = viewmodel.AsQueryable().WhereDynamic(selectRequest.Search).ToList();
            foreach (RequestResponseViewModel model in viewmodel)
            {
                try
                {
                    bool b = IsValidURI(model.Request);
                    if (b == true)
                    {
                        Uri url = new Uri(model.Request);
                        model.Request = url.LocalPath + HttpUtility.UrlDecode(url.Query);
                    }
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }
            Wrapper.Count = viewmodel.Count();
            if (!selectRequest.IsExcelDownload)
            {
                viewmodel = viewmodel.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();
            }
            Wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            Wrapper.list = viewmodel;
            failureMessage = StatusType.Ok;
            return Wrapper;
        }
    }
}