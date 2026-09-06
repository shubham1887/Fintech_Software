using Microsoft.EntityFrameworkCore;
using Project.Model;
using Project.Processing;
using Project.Requests.SentSms;
using Project.Response.SentSms;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class SentSmsDAL
    {
        internal void SMSInsert(SentSMS ObjModel)
        {
            try
            {
                if (ObjModel.Response.Length >= 500)
                {
                    ObjModel.Response = ObjModel.Response.Substring(0, 499);
                }
                using (var context = new DatabaseContext())
                {
                    context.SentSms.Add(ObjModel);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal SentSMS SelectForDuplicate(int UserID, string Mobile, string type, string TemplateID)
        {
            int TodaysDateInt = Convert.ToInt32(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
            DateTime currentDateTime = DateTime.Now.AddMinutes(-2);

            SentSMS list = new SentSMS();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                return (from sentSms in context.SentSms
                        where sentSms.DOCInt == TodaysDateInt
                        && sentSms.DOC > currentDateTime
                        && sentSms.UserID == UserID
                        && sentSms.Mobile == Mobile
                        && sentSms.Type == type
                        && sentSms.TemplateID == TemplateID

                        orderby sentSms.SentSMSID descending
                        select sentSms).FirstOrDefault();
            }
        }

        internal SentSMS SelectDuplicateByUrlAndUserID(SentSMS sentSMSModel, string url)
        {
            int TodaysDateInt = Convert.ToInt32(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
            DateTime currentDateTime = DateTime.Now.AddMinutes(-2);

            SentSMS list = new SentSMS();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                return (from sentSms in context.SentSms
                        where sentSms.DOCInt == TodaysDateInt
                        && sentSms.DOC > currentDateTime
                        && sentSms.UserID == sentSMSModel.UserID
                        && sentSms.Mobile == sentSMSModel.Mobile
                        && sentSms.Request == url
                        orderby sentSms.SentSMSID descending
                        select sentSms).FirstOrDefault();
            }
        }

        internal List<ViewModel> SelectForFilter(SelectRequest selectRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                list = (from sentSms in context.SentSms
                        join user in context.UserMaster on sentSms.UserID equals user.UserID into userJoin
                        from user in userJoin.DefaultIfEmpty()
                        join smsmaster in context.SmsMaster on sentSms.SMSMasterID equals smsmaster.SMSMasterID into smsdetails
                        from LS in smsdetails.DefaultIfEmpty()
                        where sentSms.DOCInt >= From && sentSms.DOCInt <= To
                        orderby sentSms.SentSMSID descending
                        select new ViewModel
                        {
                            SentSMSID = sentSms.SentSMSID,
                            APIName = LS.APIName ?? "",
                            UserName = user.FirmName,
                            UserID = sentSms.UserID,
                            Mobile = sentSms.Mobile,
                            Message = sentSms.Message,
                            Type = sentSms.Type,
                            Response = sentSms.Response,
                            Request = sentSms.Request,
                            DOC = sentSms.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        }).ToList();

                if (selectRequest.UserID != 0)
                {
                    list = (from item in list
                            where item.UserID == selectRequest.UserID
                            select item).ToList();
                }
                return list;
            }
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.SentSms.Where(U => U.DOCInt <= DeleteDOCInt);
                context.SentSms.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}