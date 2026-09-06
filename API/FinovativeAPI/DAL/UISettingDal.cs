using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UISettingDal
    {
        private readonly string Key = "_UISetting";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UISettingMaster>>();
            cache.Remove(Key);
        }

        internal List<UISettingMaster> Select()
        {
            var _messageList = new MemoryCacheProcessing<List<UISettingMaster>>();
            List<UISettingMaster> messageList = _messageList.GetOrCreate(Key, () => SelectWithoutCache());
            return messageList;
        }

        private List<UISettingMaster> SelectWithoutCache()
        {
            using DatabaseContext context = new DatabaseContext();
            return context.UISettingMaster.ToList();
        }

        internal string SelectByKey(string key, string DefaultValue)
        {
            UISettingMaster obj = Select().FirstOrDefault(S => S.Key == key);
            if (obj == null)
                return DefaultValue;
            return obj.Value;
        }

        internal UISettingMaster SelectByID(int UISettingMasterID)
        {
            return Select().FirstOrDefault(S => S.UISettingMasterID == UISettingMasterID);
        }

        internal int Update(UISettingMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UISettingMaster.Update(objModel);
            context.SaveChanges();
            return objModel.UISettingMasterID;
        }
    }
}