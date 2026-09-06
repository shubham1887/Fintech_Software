using Project.Model;
using Project.Processing;
using Project.Requests.Overdraft;
using Project.Response.Overdraft;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OverdraftMasterDAL
    {
        private readonly string key = "_OverdraftMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<OverdraftMaster>>();
            cache.Remove(key);
        }

        internal int Insert(OverdraftMaster ObjModel)
        {
            RemoveCache();
            int result = -1;
            try
            {
                using (var context = new DatabaseContext())
                {
                    context.OverdraftMaster.Add(ObjModel);
                    context.SaveChanges();
                    result = ObjModel.OverdraftMasterID;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal int Update(OverdraftMaster ObjModel)
        {
            RemoveCache();
            int result = -1;
            try
            {
                using (var context = new DatabaseContext())
                {
                    context.OverdraftMaster.Update(ObjModel);
                    context.SaveChanges();
                    result = ObjModel.OverdraftMasterID;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal int Delete(OverdraftMaster ObjModel)
        {
            RemoveCache();
            int result = -1;
            try
            {
                using (var context = new DatabaseContext())
                {
                    context.OverdraftMaster.Remove(ObjModel);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal float SelectOverdraftForUIByUserID(int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            float Overdraft =
            (UserTypeID == (int)UserType.Retailer ||
               UserTypeID == (int)UserType.APIUser ||
               UserTypeID == (int)UserType.Distributer ||
               UserTypeID == (int)UserType.SuperDistributer) ?
                SelectOverdraftByUserID(UserID) :
              SelectOverdraftForDownLine(UserID);

            return Overdraft;
        }

        private float SelectOverdraftByUserID(int userID)
        {
            OverdraftMaster cash = Select().FirstOrDefault(C => C.UserID == userID && C.IsActive == true);
            if (cash == null)
            {
                return 0;
            }

            return (float)System.Math.Round(cash.Overdraft, 2);
        }

        private float SelectTotalOverdraftByUserIDs(List<int> userIDs)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            float totalOverdraft = (float)Select().Where(c => userIDs.Contains(c.UserID))
                .Select(c => c.Overdraft)
                .DefaultIfEmpty()
                .Sum();

            return (float)System.Math.Round(totalOverdraft, 2);
        }

        private float SelectOverdraftForDownLine(int UserID)
        {
            float Overdraft = 0;
            List<UserMaster> users = new UserDAL().Select();

            List<int> userids = users.Select(u => u.UserID).ToList();
            Overdraft = SelectTotalOverdraftByUserIDs(userids);

            return Overdraft;
        }

        internal List<ReverseViewModel> SelectReverseReport()
        {
            List<ReverseViewModel> list = new List<ReverseViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from overdraftDetail in context.OverdraftMaster
                        join Wallet in context.UserWalletMaster on overdraftDetail.UserID equals Wallet.UserID
                        join WallteMaster in context.WalletMaster on overdraftDetail.WalletID equals WallteMaster.WalletID
                        where overdraftDetail.Overdraft > 0
                        && Wallet.WalletID == overdraftDetail.WalletID
                        orderby overdraftDetail.DOC descending

                        select new ReverseViewModel
                        {
                            OverdraftMasterID = overdraftDetail.OverdraftMasterID,
                            User = overdraftDetail.User.FirmName.ConcatNameMobileAliasName(overdraftDetail.User.MobileNumber, overdraftDetail.User.AliasName),
                            Amount = overdraftDetail.Overdraft,
                            Wallet = WallteMaster.Name,
                            DOC = overdraftDetail.DOC.ToString(Messages.DOCDisplayFormat),
                            WalletBalance = Wallet.Balance,
                        }).ToList();
            }
            return list;
        }

        internal OverdraftMaster SelectByUserID(int UserID)
        {
            List<OverdraftMaster> list = Select();
            OverdraftMaster collectionObj = list.FirstOrDefault(C => C.UserID == UserID);
            return collectionObj;
        }

        internal List<OverdraftMaster> Select()
        {
            var _overDraftList = new MemoryCacheProcessing<List<OverdraftMaster>>();
            List<OverdraftMaster> collectionList = _overDraftList.GetOrCreate(key, () => SelectWithoutCache());
            return collectionList;
        }

        private List<OverdraftMaster> SelectWithoutCache()
        {
            List<OverdraftMaster> list = new List<OverdraftMaster>();
            using (var context = new DatabaseContext())
            {
                list = context.OverdraftMaster.ToList();
            }
            return list;
        }

        internal decimal SelectSumByUserIDAndWalletID(int UserID, int WalletID)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            var allBal = (from overdraftMaster in context.OverdraftMaster
                          where overdraftMaster.UserID == UserID
                          && overdraftMaster.WalletID == WalletID
                          group overdraftMaster by 1 into grp
                          select new
                          {
                              OpeningBalance = grp.Sum(x => x.Overdraft)
                          }).FirstOrDefault();

            decimal bal = allBal?.OpeningBalance ?? 0;
            return bal;
        }

        internal OverdraftMaster SelectByOverdraftMasterID(int overdraftMasterID)
        {
            using var context = new DatabaseContext();
            return context.OverdraftMaster.FirstOrDefault(W => W.OverdraftMasterID == overdraftMasterID);
        }

        internal ReverseViewModel SelectByID(int overdraftMasterID)
        {
            OverdraftMaster master = null;
            using (var context = new DatabaseContext())
            {
                master = context.OverdraftMaster.FirstOrDefault(o => o.OverdraftMasterID == overdraftMasterID);
            }

            decimal WalletBalance = new UserWalletMasterDal().SelectSumByUserIDAndWalletID(master.UserID, master.WalletID);

            ReverseViewModel list = new ReverseViewModel();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from overdraftDetail in context.OverdraftMaster
                        where overdraftDetail.OverdraftMasterID == overdraftMasterID
                        orderby overdraftDetail.DOC descending

                        select new ReverseViewModel
                        {
                            OverdraftMasterID = overdraftDetail.OverdraftMasterID,
                            User = overdraftDetail.User.FirmName.ConcatNameMobileAliasName(overdraftDetail.User.MobileNumber, overdraftDetail.User.AliasName),
                            Amount = overdraftDetail.Overdraft,
                            Wallet = overdraftDetail.Wallet.Name,
                            WalletBalance = WalletBalance,
                            DOC = overdraftDetail.DOC.ToString(Messages.DOCDisplayFormat),
                        }).FirstOrDefault();
            }

            return list;
        }

        internal List<SelectRolewiseOverdraftResponse> SelectRolewiseOverdraftUser(SelectRolewiseOverdraftRequest mainRequest)
        {
            List<SelectRolewiseOverdraftResponse> list = new List<SelectRolewiseOverdraftResponse>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from overdraftDetail in context.OverdraftMaster
                        where mainRequest.UserTypeID != 0 ? overdraftDetail.User.UserTypeID == mainRequest.UserTypeID : 2 > 1
                        orderby overdraftDetail.DOC descending

                        select new SelectRolewiseOverdraftResponse
                        {
                            OverdraftMasterID = overdraftDetail.OverdraftMasterID,
                            FirmName = overdraftDetail.User.FirmName.ConcatNameMobileAliasName(overdraftDetail.User.MobileNumber, overdraftDetail.User.AliasName),
                            Overdraft = overdraftDetail.Overdraft,
                            MaxOverdraft = overdraftDetail.MaxOverdraft,
                            IsActive = overdraftDetail.IsActive
                        }).ToList();
            }

            return list;
        }

        internal bool SelectActiveOverdraft(int userID)
        {
            using var context = new DatabaseContext();
            OverdraftMaster master = context.OverdraftMaster.FirstOrDefault(W => W.UserID == userID && W.IsActive == true);

            if (master == null)
                return false;

            return true;
        }
    }
}