using Project.Model;
using Project.Processing;
using Project.Response.WebHookSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class WebHookSettingDal
    {
        public static readonly string key = "_WebHookSetting";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        internal WebHookSetting SelectByID(int WebHookSettingID)
        {
            using (var context = new DatabaseContext())
            {
                return context.WebHookSetting.Find(WebHookSettingID);
            }
        }

        internal int Update(WebHookSetting objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.WebHookSetting.Update(objModel);
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
                return (from webhookSetting in context.WebHookSetting
                        orderby webhookSetting.Type
                        select new ViewModel
                        {
                            WebHookSettingID = webhookSetting.WebHookSettingID,
                            Type = webhookSetting.Type,
                            URL = webhookSetting.URL,
                            HeaderJson = webhookSetting.HeaderJson,
                            MethodType = webhookSetting.MethodType,
                            IsActive = webhookSetting.IsActive,
                        }).ToList();
            }
        }

        internal ViewModel SelectByType(string Type)
        {
            return Select().FirstOrDefault(row => row.Type == Type);
        }
    }
}