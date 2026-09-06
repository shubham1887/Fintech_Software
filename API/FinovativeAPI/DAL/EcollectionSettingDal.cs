using Project.Model;
using Project.Processing;
using Project.Response.EcollectionSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class EcollectionSettingDal
    {
        public static string Key = "_EcollectionSetting";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal int Update(EcollectionSetting model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.EcollectionSetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal EcollectionSetting SelectByID(int EcollectionSettingID)
        {
            using var context = new DatabaseContext();
            return context.EcollectionSetting.Find(EcollectionSettingID);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<WalletMaster> walletslist = new WalletMasterDal().Select();

            using (var context = new DatabaseContext())
            {
                return (from setting in context.EcollectionSetting
                        join wallet in context.WalletMaster on setting.WalletID equals wallet.WalletID
                        orderby setting.EcollectionSettingID descending
                        select new ViewModel
                        {
                            EcollectionSettingID = setting.EcollectionSettingID,
                            Name = setting.Name,
                            Prefix = setting.Prefix,
                            IFSCCode = setting.IFSCCode,
                            PrivateKeyPath = setting.PrivateKeyPath,
                            PublicKeyPath = setting.PublicKeyPath,
                            PrivateKeyPassword = setting.PrivateKeyPassword,
                            WalletID = setting.WalletID,
                            WalletName = wallet.Name,
                            IsActive = setting.IsActive,
                        }).ToList();
            }
        }

        internal ViewModel SelectByName(string name)
        {
            List<ViewModel> list = Select();
            return list.FirstOrDefault(d => d.Name == name);
        }
    }
}