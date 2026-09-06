using Project.Model;
using Project.Processing;
using Project.Response.MicroATMSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class MicroATMSettingDal
    {
        public readonly string Key = "_MicroATMSetting";

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

        internal ViewModel SelectMicroATMByName(string name)
        {
            List<ViewModel> list = Select();
            return list.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.MicroATMSetting
                        where setting.IsActiveWeb == true
                        orderby setting.MicroATMSettingID descending

                        select new ViewModel
                        {
                            MicroATMSettingID = setting.MicroATMSettingID,
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
                        }).ToList();
            }
        }

        internal int Update(MicroATMSetting model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.MicroATMSetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal MicroATMSetting SelectByID(int MicroATMSettingID)
        {
            using var context = new DatabaseContext();
            return context.MicroATMSetting.Find(MicroATMSettingID);
        }

        internal ViewModel SelectActiveMicroATM()
        {
            List<ViewModel> list = Select();
            ViewModel pg = list.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        internal List<ViewModel> SelectMultiActiveMicroATM()
        {
            List<ViewModel> list = Select();
            list = list.Where(obj => obj.IsActive == true).ToList();
            return list;
        }

        internal List<MicroATMSetting> SelectActiveMicroATMForDeActive()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                return (from obj in context.MicroATMSetting
                        where obj.IsActiveWeb == true && obj.IsActive == true
                        select obj).ToList();
            }
        }

        internal ViewModel SelectActiveMicroATMByName(string Name)
        {
            List<ViewModel> list = Select();
            ViewModel pg = list.FirstOrDefault(obj => obj.IsActive == true && obj.Name.ToLower() == Name.ToLower());
            return pg;
        }

        internal ViewModel SelectActiveMicroATMByOnlyName(string Name)
        {
            List<ViewModel> list = Select();
            ViewModel pg = list.FirstOrDefault(obj => obj.Name == Name);
            return pg;
        }
    }
}