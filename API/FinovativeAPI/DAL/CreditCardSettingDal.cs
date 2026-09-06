using Project.Model;
using Project.Processing;
using Project.Response.CreditCardSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CreditCardSettingDal
    {
        private readonly string Key = "_CreditCardSetting";

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

        internal ViewModel SelectCreditCardByName(string name)
        {
            List<ViewModel> CreditCardList = Select();
            return CreditCardList.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.CreditCardSetting
                        orderby setting.CreditCardSettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            CreditCardSettingID = setting.CreditCardSettingID,
                            Name = setting.Name,
                            MerchantID = setting.MerchantID,
                            Slat = setting.Slat,
                            PublicKey = setting.PublicKey,
                            Token = setting.Token,
                            MinimumAmount = setting.MinimumAmount,
                            MaximumAmount = setting.MaximumAmount,
                            IsActive = setting.IsActive,
                            Mode = setting.Mode
                        }).ToList();
            }
        }

        internal int Update(CreditCardSetting model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.CreditCardSetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal CreditCardSetting SelectByID(int CreditCardSettingID)
        {
            using var context = new DatabaseContext();
            return context.CreditCardSetting.Find(CreditCardSettingID);
        }

        internal ViewModel SelectActiveCreditCard()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        public string SelectActiveCreditCardName()
        {
            removeCache();
            string ActiveCreditCard = "";
            List<ViewModel> List = Select();

            ViewModel sett = List.FirstOrDefault(d => d.IsActive == true);
            if (sett == null)
            {
                return "";
            }
            ActiveCreditCard = sett.Name;
            return string.IsNullOrEmpty(ActiveCreditCard) ? "" : ActiveCreditCard;
        }
    }
}