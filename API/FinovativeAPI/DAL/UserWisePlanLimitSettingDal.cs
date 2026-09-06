using Microsoft.EntityFrameworkCore;
using Project.Model;
using Project.Processing;
using Project.Response.UserWisePlanLimitSetting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class UserWisePlanLimitSettingDal
    {
        public static string Key = "_UserWisePlanLimitSettingDal";
        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }
        internal int Insert(UserWisePlanLimit objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserWisePlanLimit.Add(objModel);
            context.SaveChanges();
            return objModel.UserWisePlanLimitID;
        }

        internal int Delete(UserWisePlanLimit ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserWisePlanLimit.Remove(ObjModel);
            context.SaveChanges();
            return ObjModel.UserWisePlanLimitID;
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }


        internal int Update(UserWisePlanLimit model)
        {
            RemoveCache();
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.UserWisePlanLimit.Update(model);
                context.SaveChanges();
                result = model.UserWisePlanLimitID;
            }
            return result;
        }

        internal ViewModel SelectDuplicate(int UserID)
        {
            List<ViewModel> list = Select();
            return (from item in list
                    where item.UserID == UserID
                    select item).FirstOrDefault();
        }


        internal void UpdateRange(List<UserWisePlanLimit> objModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.UserWisePlanLimit.UpdateRange(objModel);
                context.SaveChanges();
                return;
            }

        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.UserWisePlanLimit
                        orderby setting.UserWisePlanLimitID descending
                        select new ViewModel
                        {
                            UserWisePlanLimitID = setting.UserWisePlanLimitID,
                            CircleHitCount = setting.CircleHitCount,
                            PlanHitCount = setting.PlanHitCount,
                            BillFetchHitCount = setting.BillFetchHitCount,
                            UserID = setting.UserID,
                            UserTypeID = setting.UserTypeID,
                            IsActive = setting.IsActive,
                        }).ToList();
            }
        }

        internal async Task<List<UserWisePlanLimit>> SelectAll()
        {
            using (var context = new DatabaseContext())
                return await context.UserWisePlanLimit.ToListAsync();
        }

        public List<ViewModel> SelectReport()
        {
            List<ViewModel> mainList = Select();
            List<UserMaster> userList = new UserDAL().Select();

            List<ViewModel> list = (from setting in mainList
                                    join user in userList on setting.UserID equals user.UserID into joinedList
                                    from user in joinedList.DefaultIfEmpty()

                                    select new ViewModel
                                    {
                                        UserWisePlanLimitID = setting.UserWisePlanLimitID,
                                        CircleHitCount = setting.CircleHitCount,
                                        PlanHitCount = setting.PlanHitCount,
                                        BillFetchHitCount = setting.BillFetchHitCount,
                                        UserID = user?.UserID ?? 0,
                                        FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),

                                        UserTypeID = setting.UserTypeID,
                                        IsActive = setting.IsActive,
                                    }).ToList();

            return list;
        }

        internal UserWisePlanLimit SelectByID(int UserWisePlanLimitID)
        {
            using var context = new DatabaseContext();
            return context.UserWisePlanLimit.Find(UserWisePlanLimitID);
        }

    }
}
