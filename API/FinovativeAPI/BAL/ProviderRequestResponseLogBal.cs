using Project.DAL;
using Project.Processing;
using Project.Requests.APILog;
using Project.Requests.Transaction;
using Project.Response.ProviderRequestResponseLogResponse;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.BAL
{
    public class ProviderRequestResponseLogBal
    {
        internal List<RequestResponseViewModel> SelectByConsumerNumber(SearchRequest searchRequest, out string failureMessage)
        {
            List<RequestResponseViewModel> list = new List<RequestResponseViewModel>();
            failureMessage = searchRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return list;
            }

            try
            {
                list = new ProviderRequestResponseLogDal().SelectByConsumerNumber(searchRequest);
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

            foreach (RequestResponseViewModel model in list)
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
            failureMessage = StatusType.Ok;
            return list;
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

        internal List<RequestResponseViewModel> SelectByTansactionMasterID(SelectByIDRequest selectByIDRequest, out string failureMessage)
        {
            List<RequestResponseViewModel> viewmodel = new List<RequestResponseViewModel>();

            failureMessage = selectByIDRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return viewmodel;
            }

            try
            {
                viewmodel = new ProviderRequestResponseLogDal().SelectByTansactionMasterID(selectByIDRequest.ID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return viewmodel;
            }

            if (viewmodel == null)
            {
                failureMessage = Messages.DataNotFound;
                return viewmodel;
            }
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
            failureMessage = StatusType.Ok;
            return viewmodel;
        }
        internal List<RequestResponseViewModel> SelectByUserID(SelectByIDRequest selectByIDRequest, out string failureMessage)
        {
            List<RequestResponseViewModel> viewmodel = new List<RequestResponseViewModel>();

            failureMessage = selectByIDRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return viewmodel;
            }

            try
            {
                viewmodel = new ProviderRequestResponseLogDal().SelectByUserID(selectByIDRequest.ID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return viewmodel;
            }

            if (viewmodel == null)
            {
                failureMessage = Messages.DataNotFound;
                return viewmodel;
            }
            failureMessage = StatusType.Ok;
            return viewmodel;
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
                viewmodel = new ProviderRequestResponseLogDal().SelectByFromDateTodate(selectRequest);
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