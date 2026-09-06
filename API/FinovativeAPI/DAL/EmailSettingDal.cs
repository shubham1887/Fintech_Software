using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class EmailSettingDal
    {
        private readonly string key = " _EmailSetting";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<EmailSettingMaster>>();
            cache.Remove(key);
        }

        internal int Insert(EmailSettingMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.EmailSettingMaster.Add(objModel);
            context.SaveChanges();
            return objModel.EmailSettingMasterID;
        }

        internal EmailSettingMaster SelectByEmail(string email)
        {
            List<EmailSettingMaster> list = SelectAll();
            return (from emails in list
                    where emails.Email == email
                    select emails).FirstOrDefault();
        }

        internal int Update(EmailSettingMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.EmailSettingMaster.Update(objModel);
            context.SaveChanges();
            return objModel.EmailSettingMasterID;
        }

        internal List<EmailSettingMaster> Select()
        {
            RemoveCache();
            var _emailSettingList = new MemoryCacheProcessing<List<EmailSettingMaster>>();
            List<EmailSettingMaster> settingList = _emailSettingList.GetOrCreate(key, () => SelectWithoutCache());
            return settingList;
        }

        internal EmailSettingMaster SelectByID(int emailSettingID)
        {
            List<EmailSettingMaster> list = SelectAll();
            return (from emails in list
                    where emails.EmailSettingMasterID == emailSettingID
                    select emails).FirstOrDefault();
        }

        private List<EmailSettingMaster> SelectWithoutCache()
        {
            List<EmailSettingMaster> list = new List<EmailSettingMaster>();
            using (var context = new DatabaseContext())
            {
                list = context.EmailSettingMaster.ToList();
            }
            return list;
        }

        internal List<EmailSettingMaster> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<EmailSettingMaster>>();

            List<EmailSettingMaster> list = _list.GetOrCreate(CacheKeys.EmailSettingMaster, () => SelectWithoutCache());

            return list;
        }
    }
}