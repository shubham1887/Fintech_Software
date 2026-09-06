using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.WebHookSetting;
using Project.Response.WebHookSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.BAL
{
    public class WebHookLogBal
    {
        public string Insert(WebHookLogRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            WebHookLog ObjModel = new WebHookLog
            {
                Type = mainRequest.Type,
                RequestURL = mainRequest.RequestURL,
                Response = mainRequest.Response
            };
            try
            {
                int rowsAfftected = new WebHookLogDal().Insert(ObjModel);

                if (rowsAfftected <= 0)
                {
                    return ServiceMessage.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            return StatusType.Ok;
        }

        internal ReportWrapper SelectByFromDateTodate(SelectRequest selectRequest, out string failureMessage)
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
                viewmodel = new WebHookLogDal().SelectByFromDateTodate(selectRequest);
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
            Wrapper.list = viewmodel.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();

            failureMessage = StatusType.Ok;
            return Wrapper;
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
    }
}