using Project.Model;
using Project.Processing;
using Project.Response.Slab;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserSlabMappingDal
    {
        public readonly string UserSlabMapping = "_UserSlabMapping";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<UserSlabMappingViewModel>>();
            cache.Remove(UserSlabMapping);
        }

        internal int Insert(UserSlabMapping SlabRequest)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.UserSlabMapping.Add(SlabRequest);
                return context.SaveChanges();
            }
        }

        internal UserSlabMapping CheckDuplicate(UserSlabMapping userSlabMapping)
        {
            using (var context = new DatabaseContext())
            {
                return (from userSlab in context.UserSlabMapping
                        where userSlab.UserID == userSlabMapping.UserID &&
                              userSlab.IsDelete == false
                        select userSlab).FirstOrDefault();
            }
        }

        internal int Update(UserSlabMapping objModel)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.UserSlabMapping.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal List<UserSlabMappingViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<UserSlabMappingViewModel>>();

            List<UserSlabMappingViewModel> list = _list.GetOrCreate(UserSlabMapping, () => SelectWithoutCache());

            return list;
        }
       
        private List<UserSlabMappingViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return (from UserSlab in context.UserSlabMapping
                        join userMaster in context.UserMaster
                        on UserSlab.UserID equals userMaster.UserID
                        join userTypeMaster in context.UserTypeMaster
                        on userMaster.UserTypeID equals userTypeMaster.UserTypeID
                        where UserSlab.IsDelete == false
                        select new UserSlabMappingViewModel
                        {
                            UserID = UserSlab.UserMaster.UserID,
                            FirmName = UserSlab.UserMaster.FirmName.ConcatNameMobileAliasName(UserSlab.UserMaster.MobileNumber, UserSlab.UserMaster.AliasName),
                            SlabName = UserSlab.SlabMaster.SlabName,
                            Role = userTypeMaster.UserType,
                            SlabID = UserSlab.SlabMaster.SlabMasterID,
                            UserSlabMappingID = UserSlab.UserSlabMappingID
                        }).ToList();
            }
        }

        internal UserSlabMapping SelectByID(int UserSlabMappingID)
        {
            using (var context = new DatabaseContext())
            {
                return context.UserSlabMapping.Find(UserSlabMappingID);
            }
        }
        internal UserSlabMappingViewModel SelectByIDWithCashe(int UserID)
        {
            List<UserSlabMappingViewModel> list = Select();

                return list.FirstOrDefault(Txn => Txn.UserID == UserID);
            
        }

        internal UserSlabMappingViewModel SelectByUserSlabMappingID(int UserSlabMappingID)
        {
            List<UserSlabMappingViewModel> list = Select();

            return (from UserSlab in list
                    where UserSlab.UserSlabMappingID == UserSlabMappingID
                    select UserSlab).FirstOrDefault();
        }

        internal UserSlabMappingViewModel SelectByUserID(int UserID)
        {
            List<UserSlabMappingViewModel> list = Select();

            return (from UserSlab in list
                    where UserSlab.UserID == UserID
                    select UserSlab).FirstOrDefault();
        }

        internal int SelectBySlabMasterID(int SlabMasterID)
        {
            List<UserSlabMappingViewModel> list = Select();

            var lists = (from UserSlab in list
                         where UserSlab.SlabID == SlabMasterID
                         select UserSlab).ToList();

            return lists.Count();
        }

        internal UserSlabMappingViewModel SelectSlabNameByUserID(int UserID)
        {
            List<UserSlabMappingViewModel> list = Select();
            List<UserMaster> Userlist = new UserDAL().Select();
            int parentID = Userlist.FirstOrDefault(U => U.UserID == UserID).ParentID;
            int SuperparentID = Userlist.FirstOrDefault(U => U.UserID == parentID).ParentID;

            UserSlabMappingViewModel response = null;
            response = (from UserSlab in list
                        where UserSlab.UserID == UserID
                        select UserSlab).FirstOrDefault();
            if (response == null)
                response = (from UserSlab in list
                            where UserSlab.UserID == parentID
                            select UserSlab).FirstOrDefault();
            if (response == null)
                response = (from UserSlab in list
                            where UserSlab.UserID == SuperparentID
                            select UserSlab).FirstOrDefault();
            return response;
        }

        internal bool IsValidForDelete(int SlabID)
        {
            List<UserSlabMappingViewModel> list = Select();
            UserSlabMappingViewModel slablist = new UserSlabMappingViewModel();
            slablist = (from UserSlab in list
                        where UserSlab.SlabID == SlabID
                        select UserSlab).FirstOrDefault();

            if (slablist == null)
                return true;
            return false;
        }
    }
}