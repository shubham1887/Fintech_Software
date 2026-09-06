using Project.Model;
using Project.Processing;
using Project.Response.POSSettingMaster;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class POSSettingMasterDal
    {
        public readonly string Key = "_POSSettingMaster";

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

        internal ViewModel SelectPOSByName(string name)
        {
            List<ViewModel> list = Select();
            return list.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.POSSettingMaster
                        where setting.IsActiveWeb == true
                        orderby setting.POSSettingMasterID descending

                        select new ViewModel
                        {
                            POSSettingMasterID = setting.POSSettingMasterID,
                            Name = setting.Name,
                            MerchantID = setting.MerchantID,
                            Slat = setting.Slat,
                            PublicKey = setting.PublicKey,
                            Token = setting.Token,
                            Password = setting.Password,
                            UserName = setting.UserName,
                            MinimumAmount = setting.MinimumAmount,
                            MaximumAmount = setting.MaximumAmount,
                            DisplayName = setting.DisplayName,
                            Sequence = setting.Sequence,
                            IsActive = setting.IsActive,
                            AmountWithCommission = setting.AmountWithCommission,
                        }).ToList();
            }
        }

        internal int Update(POSSettingMaster model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.POSSettingMaster.Update(model);
                return context.SaveChanges();
            }
        }

        internal POSSettingMaster SelectByID(int POSSettingMasterID)
        {
            using var context = new DatabaseContext();
            return context.POSSettingMaster.Find(POSSettingMasterID);
        }

        internal ViewModel SelectActivePOS()
        {
            List<ViewModel> list = Select();
            ViewModel pg = list.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        internal List<ViewModel> SelectMultiActive()
        {
            List<ViewModel> list = Select();
            list = list.Where(obj => obj.IsActive == true).ToList();
            return list;
        }

        internal List<POSSettingMaster> SelectActivePOSForDeActive()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                return (from obj in context.POSSettingMaster
                        where obj.IsActiveWeb == true && obj.IsActive == true
                        select obj).ToList();
            }
        }

        internal ViewModel SelectActivePOSByName(string Name)
        {
            List<ViewModel> list = Select();
            ViewModel pg = list.FirstOrDefault(obj => obj.IsActive == true && obj.Name == Name);
            return pg;
        }

        internal ViewModel SelectPOSByOnlyNameForStatusCheck(string Name)
        {
            List<ViewModel> list = Select();
            ViewModel pg = list.FirstOrDefault(obj => obj.Name == Name);
            return pg;
        }
    }
}