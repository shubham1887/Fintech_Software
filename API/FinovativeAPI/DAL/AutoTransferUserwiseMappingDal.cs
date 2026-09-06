using Project.Model;
using Project.Processing;
using Project.Response.AutoTransferUserwiseMapping;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class AutoTransferUserwiseMappingDal
    {
        private readonly string Key = "_AutoTransferUserwiseMapping";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<AutoTransferUserwiseMapping>>();
            cache.Remove(Key);
        }

        internal AutoTransferUserwiseMapping SelectByUserID(int UserID)
        {
            List<AutoTransferUserwiseMapping> response = SelectAll();

            return response.FirstOrDefault(config => config.UserID == UserID);
        }

        internal AutoTransferUserwiseMapping SelectByID(int AutoTransferUserwiseMappingID)
        {
            using DatabaseContext context = new DatabaseContext();
            return context.AutoTransferUserwiseMapping.Find(AutoTransferUserwiseMappingID);
        }

        internal List<AutoTransferUserwiseMapping> SelectAll()
        {
            var _messageList = new MemoryCacheProcessing<List<AutoTransferUserwiseMapping>>();
            List<AutoTransferUserwiseMapping> messageList = _messageList.GetOrCreate(Key, () => SelectWithoutCache());
            return messageList;
        }

        private List<AutoTransferUserwiseMapping> SelectWithoutCache()
        {
            using DatabaseContext context = new DatabaseContext();
            return context.AutoTransferUserwiseMapping.ToList();
        }

        public int Insert(AutoTransferUserwiseMapping autobalance)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.AutoTransferUserwiseMapping.Add(autobalance);
            context.SaveChanges();
            return autobalance.AutoTransferUserwiseMappingID;
        }

        internal int Update(AutoTransferUserwiseMapping objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.AutoTransferUserwiseMapping.Update(objModel);
            context.SaveChanges();
            return objModel.AutoTransferUserwiseMappingID;
        }

        internal List<ViewModel> Select(int ParentID)
        {
            List<AutoTransferUserwiseMapping> allList = SelectAll();
            List<UserMaster> usersList = new UserDAL().Select();
            List<ViewModel> list = new List<ViewModel>();

            list = (from setting in allList
                    join user in usersList on setting.UserID equals user.UserID
                    where setting.IsOTPVerified == true &&
                    setting.ParentID == ParentID

                    orderby user.UserTypeID ascending
                    select new ViewModel
                    {
                        AutoTransferUserwiseMappingID = setting.AutoTransferUserwiseMappingID,
                        FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                        DMTAmountIncreasedBy = setting.DMTAmountIncreasedBy,
                        DMTLowBalanceReached = setting.DMTLowBalanceReached,
                        RechargeAmountIncreasedBy = setting.RechargeAmountIncreasedBy,
                        RechargeLowBalanceReached = setting.RechargeLowBalanceReached,
                        MaxOutstanding = setting.MaxOutstanding,
                        IsOTPVerified = setting.IsOTPVerified,
                        UserID = setting.UserID,
                        ParentID = setting.ParentID
                    }).ToList();

            return list;
        }

        internal AutoTransferUserwiseMapping SelectByUserIDOTPVerified(int UserID)
        {
            List<AutoTransferUserwiseMapping> response = SelectAll();

            return response.FirstOrDefault(config => config.UserID == UserID && config.IsOTPVerified == true);
        }

        internal int Delete(AutoTransferUserwiseMapping objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.AutoTransferUserwiseMapping.Remove(objModel);
            context.SaveChanges();
            return objModel.AutoTransferUserwiseMappingID;
        }
    }
}