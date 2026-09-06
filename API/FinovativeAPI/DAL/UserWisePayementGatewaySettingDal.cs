using Project.Model;
using Project.Processing;

using Project.Response.PaymentGateway;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserWisePayementGatewaySettingDal
    {
        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UserwiseGatewayViewModel>>();
            cache.Remove(CacheKeys.UserWisePayementGatewaySetting);
        }

        internal int Insert(UserWisePayementGatewaySetting objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserWisePayementGatewaySetting.Add(objModel);
            return context.SaveChanges();
        }

        internal int Update(UserWisePayementGatewaySetting objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserWisePayementGatewaySetting.Update(objModel);
            return context.SaveChanges();
        }

        internal int Delete(UserWisePayementGatewaySetting objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserWisePayementGatewaySetting.Remove(objModel);
            context.SaveChanges();
            return objModel.UserWisePayementGatewaySettingID;
        }

        internal UserWisePayementGatewaySetting SelectByID(int UserWisePayementGatewaySettingID)
        {
            using var context = new DatabaseContext();
            return context.UserWisePayementGatewaySetting.Find(UserWisePayementGatewaySettingID);
        }

        internal UserwiseGatewayViewModel SelectPGByUserID(int UserID)
        {
            UserwiseGatewayViewModel list = null;
            using var context = new DatabaseContext();
            return list = (from gateway in context.UserWisePayementGatewaySetting
                           where gateway.UserID == UserID
                           select new UserwiseGatewayViewModel
                           {
                               UserWisePayementGatewaySettingID = gateway.UserWisePayementGatewaySettingID,
                               UserID = gateway.UserID,
                               MaximumAmount = gateway.MaximumAmount,
                               MinimumAmount = gateway.MinimumAmount,
                               PaymentGatewaySettingID = gateway.PaymentGatewaySettingID,
                               FirmName = gateway.UserMaster.FirmName.ConcatNameMobileAliasName(gateway.UserMaster.MobileNumber, gateway.UserMaster.AliasName),
                               PaymentGateway = gateway.PaymentGatewaySetting.Name,
                           }).FirstOrDefault();
        }

        internal UserwiseGatewayViewModel SelectByUserWisePayementGatewaySettingID(int UserWisePayementGatewaySettingID)
        {
            using var context = new DatabaseContext();
            return (from gateway in context.UserWisePayementGatewaySetting
                    where gateway.UserWisePayementGatewaySettingID == UserWisePayementGatewaySettingID

                    select new UserwiseGatewayViewModel
                    {
                        UserWisePayementGatewaySettingID = gateway.UserWisePayementGatewaySettingID,
                        UserID = gateway.UserID,
                        MaximumAmount = gateway.MaximumAmount,
                        MinimumAmount = gateway.MinimumAmount,
                        PaymentGatewaySettingID = gateway.PaymentGatewaySettingID,
                        FirmName = gateway.UserMaster.FirmName.ConcatNameMobileAliasName(gateway.UserMaster.MobileNumber, gateway.UserMaster.AliasName),
                        PaymentGateway = gateway.PaymentGatewaySetting.Name,
                    }).FirstOrDefault();
        }

        internal List<UserwiseGatewayViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<UserwiseGatewayViewModel>>();

            List<UserwiseGatewayViewModel> list = _list.GetOrCreate(CacheKeys.UserWisePayementGatewaySetting, () => SelectWithoutCache());

            return list;
        }

        private List<UserwiseGatewayViewModel> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return (from gateway in context.UserWisePayementGatewaySetting

                    orderby gateway.UserWisePayementGatewaySettingID descending
                    select new UserwiseGatewayViewModel
                    {
                        UserWisePayementGatewaySettingID = gateway.UserWisePayementGatewaySettingID,
                        UserID = gateway.UserID,
                        MaximumAmount = gateway.MaximumAmount,
                        MinimumAmount = gateway.MinimumAmount,
                        PaymentGatewaySettingID = gateway.PaymentGatewaySettingID,
                        FirmName = gateway.UserMaster.FirmName.ConcatNameMobileAliasName(gateway.UserMaster.MobileNumber, gateway.UserMaster.AliasName),
                        PaymentGateway = gateway.PaymentGatewaySetting.Name,
                    }).ToList();
        }

        internal UserWisePayementGatewaySetting SelectForDuplicate(int UserID)
        {
            using var context = new DatabaseContext();
            return (from gateway in context.UserWisePayementGatewaySetting
                    where gateway.UserID == UserID
                    select new UserWisePayementGatewaySetting
                    {
                        UserWisePayementGatewaySettingID = gateway.UserWisePayementGatewaySettingID,
                        UserID = gateway.UserID,
                        PaymentGatewaySettingID = gateway.PaymentGatewaySettingID,
                    }).FirstOrDefault();
        }
    }
}