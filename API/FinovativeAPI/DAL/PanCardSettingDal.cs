using Project.Model;
using Project.Processing;
using Project.Response.PanCardSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class PanCardSettingDal
    {
        private readonly string Key = "_PanCardSetting";

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

        internal ViewModel SelectPanCardByName(string name)
        {
            List<ViewModel> viewModel = Select();
            return viewModel.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.PanCardSetting
                        orderby setting.PanCardSettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            PanCardSettingID = setting.PanCardSettingID,
                            Name = setting.Name,
                            MerchantID = setting.MerchantID,
                            Slat = setting.Slat,
                            PublicKey = setting.PublicKey,
                            Token = setting.Token,
                            IsActive = setting.IsActive,
                            Mode = setting.Mode
                        }).ToList();
            }
        }

        internal int Update(PanCardSetting model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.PanCardSetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal PanCardSetting SelectByID(int PanCardSettingID)
        {
            using var context = new DatabaseContext();
            return context.PanCardSetting.Find(PanCardSettingID);
        }

        internal ViewModel SelectActivePanCard()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        public string SelectActivePanCardByName()
        {
            removeCache();
            string ActivePanCard = "";
            List<ViewModel> List = Select();

            ViewModel sett = List.FirstOrDefault(d => d.IsActive == true);
            if (sett == null)
            {
                return "";
            }
            ActivePanCard = sett.Name;
            return string.IsNullOrEmpty(ActivePanCard) ? "" : ActivePanCard;
        }
    }
}