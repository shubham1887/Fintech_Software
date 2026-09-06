using Project.Model;
using Project.Processing;
using Project.Response.CyberPlate;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CyberPlateSettingDAL
    {
        public static string Key
        { get { return "_CyberPlateSetting"; } }

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.CyberPlateSetting
                        orderby setting.CyberPlateSettingID descending
                        select new ViewModel
                        {
                            CyberPlateSettingID = setting.CyberPlateSettingID,
                            Name = setting.Name,
                            AP = setting.AP,
                            SD = setting.SD,
                            OP = setting.OP,
                            TermID = setting.TermID,
                            AID = setting.AID,
                            Password = setting.Password,
                            SecretKeyPath = setting.SecretKeyPath,
                            IsActiveWeb = setting.IsActiveForWebsite,
                            IsActive = setting.IsActive
                        }).ToList();
            }
        }

        internal CyberPlateSetting SelectByID(int CyberPlateSettingID)
        {
            using var context = new DatabaseContext();
            return context.CyberPlateSetting.Find(CyberPlateSettingID);
        }

        internal int Update(CyberPlateSetting model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.CyberPlateSetting.Update(model);
                return context.SaveChanges();
            }
        }
    }
}