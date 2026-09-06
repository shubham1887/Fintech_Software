using Newtonsoft.Json;
using Project.Message;
using Project.Processing;
using Project.Response.VersionHistory;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class VersionHistoryBal
    {
        public List<VersionHistoryViewModel> VersionHistorySelect(out string failureMessage)
        {
            List<VersionHistoryViewModel> viewresponse = new List<VersionHistoryViewModel>();

            try
            {
                failureMessage = VersionHistoryMessage.NotFound;
                string VersionHistoryURL = ConsistySupportSystem.VersionHistoryURL;

                // VersionHistoryURL = VersionHistoryURL.Replace("[Keydata]", "");
                //  VersionHistoryURL = VersionHistoryURL.Replace("[Token]", Company.ConsistyLoginToken);
                if (VersionHistoryURL == null || VersionHistoryURL.Trim().Length <= 5)
                {
                    return viewresponse;
                }

                string VersionHistoryresponse = HttpProcessing.CallGetMethod(VersionHistoryURL, 30);
                if (VersionHistoryresponse == null || VersionHistoryresponse.Trim().Length <= 0 || VersionHistoryresponse == "")
                {
                    return viewresponse;
                }

                ViewModel view = null;
                view = JsonConvert.DeserializeObject<ViewModel>(VersionHistoryresponse);
                if (view.Status.EqualsIgnoreCase(StatusType.Failure))
                    return viewresponse;

                viewresponse = JsonConvert.DeserializeObject<List<VersionHistoryViewModel>>(view.Response);
                viewresponse = viewresponse.OrderByDescending(x => Convert.ToDateTime(x.DOC)).ToList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return viewresponse;
        }
    }
}