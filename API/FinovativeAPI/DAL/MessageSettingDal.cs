using Project.Model;
using Project.Processing;
using Project.Requests.MessageSetting;
using Project.Response.MessageSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class MessageSettingDal
    {
        public static readonly string key = "_MessageSetting";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        internal MessageSetting SelectByID(int MessageSettingID)
        {
            using (var context = new DatabaseContext())
            {
                return context.MessageSetting.Find(MessageSettingID);
            }
        }

        internal int Update(MessageSetting objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.MessageSetting.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from messageSetting in context.MessageSetting
                        orderby messageSetting.Type
                        select new ViewModel
                        {
                            MessageSettingID = messageSetting.MessageSettingID,
                            Type = messageSetting.Type,
                            SmsMessage = messageSetting.SmsMessage,
                            EmailMessage = messageSetting.EmailMessage,
                            TemplateID = messageSetting.TemplateID,
                            IsSMS = messageSetting.IsSMS,
                            ISOTP = messageSetting.IsOTP,
                            IsMail = messageSetting.IsMail,
                            IsWhatsUp = messageSetting.IsWhatsUp,
                            WhatsUpMessage = messageSetting.WhatsUpMessage,
                        }).ToList();
            }
        }

        internal ViewModel SelectByType(string Type)
        {
            return Select().FirstOrDefault(row => row.Type == Type);
        }

        internal List<ViewModel> SelectForFilter(SelectRequest selectRequest)
        {
            List<ViewModel> list = new List<ViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from Obj in context.MessageSetting
                        orderby Obj.DOC descending
                        select new ViewModel
                        {
                            Type = Obj.Type,
                            SmsMessage = Obj.SmsMessage,
                            TemplateID = Obj.TemplateID,
                            WhatsUpMessage = Obj.WhatsUpMessage,
                            //EmailMessage = Obj.EmailMessage,
                        }).ToList();

                return list;
            }
        }
    }
}