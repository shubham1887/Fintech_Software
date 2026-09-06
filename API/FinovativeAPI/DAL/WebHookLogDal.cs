using Project.Model;
using Project.Processing;
using Project.Requests.WebHookSetting;
using Project.Response.WebHookSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class WebHookLogDal
    {
        internal int Insert(WebHookLog objModel)
        {
            using (var context = new DatabaseContext())
            {
                context.WebHookLog.Add(objModel);
                context.SaveChanges();
                return objModel.WebHookLogID;
            }
        }

        internal WebHookLog SelectByID(int WebHookLogID)
        {
            using (var context = new DatabaseContext())
            {
                return context.WebHookLog.Find(WebHookLogID);
            }
        }

        internal List<RequestResponseViewModel> SelectByFromDateTodate(SelectRequest selectRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<RequestResponseViewModel> Record = null;

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            using (var context = new DatabaseContext())
            {
                Record = (from ObjRR in context.WebHookLog
                          where ObjRR.DOCInt >= FromDate && ObjRR.DOCInt <= ToDate
                          orderby ObjRR.WebHookLogID descending
                          select new RequestResponseViewModel
                          {
                              Request = ObjRR.RequestURL,
                              Response = ObjRR.Response,
                              Type = ObjRR.Type,

                              DOC = ObjRR.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                          }).ToList();
            }

            if (Record == null || Record.Count <= 0)
            {
                return Record;
            }

            if (!selectRequest.Type.EqualsIgnoreCase("0"))
            {
                Record = (from log in Record
                          where log.Type == selectRequest.Type
                          select log).ToList();
            }

            return Record;
        }
    }
}