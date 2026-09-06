using Project.Model;
using Project.Processing;
using Project.Response.PanMitra;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class PanMitraOnboardedUserDal
    {
        public readonly string key = "_PanMitraOnboardedUser";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<PanMitraOnboardedUser>>();
            cache.Remove(key);
        }

        internal int Insert(PanMitraOnboardedUser objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.PanMitraOnboardedUser.Add(objModel);
                return context.SaveChanges();
            }
        }

        internal List<PanMitraOnboardedUser> Select()
        {
            var _list = new MemoryCacheProcessing<List<PanMitraOnboardedUser>>();
            List<PanMitraOnboardedUser> list = _list.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        private List<PanMitraOnboardedUser> SelectWithoutCache()
        {
            List<PanMitraOnboardedUser> list;
            using (var context = new DatabaseContext())
            {
                list = (from item in context.PanMitraOnboardedUser
                        select item).ToList();
            }
            return list;
        }

        internal PanMitraOnboardedUser SelectByID(int UserID)
        {
            PanMitraOnboardedUser item = Select().FirstOrDefault(S => S.UserID == UserID);
            return item;
        }
        internal List<RegistrationViewModel> SelectByUserID(int UserID)
        {
            List<PanMitraOnboardedUser> list = Select();
            List<UserMaster> userList = new UserDAL().Select();
            return (from item in list
                    join user in userList on item.UserID equals user.UserID
                    where item.UserID == UserID
                    select new RegistrationViewModel
                    {
                        FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                        VleID = item.VleID,
                        EmailID = user.EmailID,
                        Status = item.Status
                    }).ToList();
        }
       
    }
}
