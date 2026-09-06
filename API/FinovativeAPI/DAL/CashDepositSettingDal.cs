using Project.Model;
using Project.Processing;
using Project.Response.CashDepositSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CashDepositSettingDal
    {
        private readonly string Key = "_CashDepositSetting";

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
                return (from setting in context.CashDepositSetting
                        orderby setting.CashDepositSettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            CashDepositSettingID = setting.CashDepositSettingID,
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

        internal int Update(CashDepositSetting model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.CashDepositSetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal CashDepositSetting SelectByID(int CashDepositSettingID)
        {
            using var context = new DatabaseContext();
            return context.CashDepositSetting.Find(CashDepositSettingID);
        }

        internal ViewModel SelectActiveCashDeposit()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }
    }
}