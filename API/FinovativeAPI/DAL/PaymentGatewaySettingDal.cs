using Project.Model;
using Project.Processing;
using Project.Response.PaymentGateway;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class PaymentGatewaySettingDal
    {
        private static string Key = "_PaymentGatewaySetting";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> list = SelectWithoutCache();
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.PaymentGatewaySetting
                        orderby setting.PaymentGatewaySettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            PaymentGatewaySettingID = setting.PaymentGatewaySettingID,
                            Name = setting.Name,
                            MerchantID = setting.MerchantID,
                            Slat = setting.Slat,
                            Token = setting.Token,
                            SuccessURL = setting.SuccessURL,
                            FailURL = setting.FailURL,
                            ActionURL = setting.ActionURL,
                            StatusCheckURL = setting.StatusCheckURL,
                            MinimumAmount = setting.MinimumAmount,
                            MaximumAmount = setting.MaximumAmount,
                            IsActive = setting.IsActive,
                            IsUPI = setting.IsUPI,
                            IsManualStatusChange = setting.IsManualStatusChange,
                           SettlementHoldInMinutes = setting.SettlementHoldInMinutes

                        }).ToList();
            }
        }

        internal int Update(PaymentGatewaySetting model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.PaymentGatewaySetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal PaymentGatewaySetting SelectByID(int PaymentGatewaySettingID)
        {
            using var context = new DatabaseContext();
            return context.PaymentGatewaySetting.Find(PaymentGatewaySettingID);
        }

        internal ViewModel SelectByPGSettingID(int PaymentGatewaySettingID)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.PaymentGatewaySettingID == PaymentGatewaySettingID);
            return pg;
        }

        internal ViewModel SelectActivePaymentGateway()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        internal ViewModel SelectPaymentGatewayByName(string name)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.Name.ToLower() == name.ToLower());
            return pg;
        }

        internal List<ViewModel> SelectActivePaymentGatewayV2()
        {
            List<ViewModel> pg = Select().FindAll(obj => obj.IsActive == true);
            return pg;
        }

        internal ViewModel SelectActivePaymentGatewayBYIsUPI(bool IsUPI)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsUPI != IsUPI && obj.IsActive == true);
            return pg;
        }
    }
}