using Project.Model;
using Project.Processing;
using Project.Response.UserChannelSlabMapping;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserChannelSlabMappingDal
    {
        public static string Key
        { get { return "_UserChannelSlabMapping"; } }

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(UserChannelSlabMapping SlabRequest)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.UserChannelSlabMapping.Add(SlabRequest);
                return context.SaveChanges();
            }
        }

        internal UserChannelSlabMapping CheckDuplicate(UserChannelSlabMapping userSlabMapping)
        {
            using (var context = new DatabaseContext())
            {
                return (from userSlab in context.UserChannelSlabMapping
                        where userSlab.UserID == userSlabMapping.UserID &&
                              userSlab.IsDelete == false
                        select userSlab).FirstOrDefault();
            }
        }

        internal int Update(UserChannelSlabMapping objModel)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.UserChannelSlabMapping.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return (from UserSlab in context.UserChannelSlabMapping
                        join userMaster in context.UserMaster
                        on UserSlab.UserID equals userMaster.UserID
                        join SlabMaster in context.ChannelSlabMaster
                        on UserSlab.ChannelSlabMasterID equals SlabMaster.ChannelSlabMasterID
                        join userTypeMaster in context.UserTypeMaster
                        on userMaster.UserTypeID equals userTypeMaster.UserTypeID
                        where UserSlab.IsDelete == false
                        && SlabMaster.IsDelete == false

                        select new ViewModel
                        {
                            UserID = UserSlab.UserMaster.UserID,
                            FirmName = userMaster.FirmName.ConcatNameMobileAliasName(userMaster.MobileNumber, userMaster.AliasName),
                            SlabName = UserSlab.ChannelSlabMaster.SlabName,
                            Role = userTypeMaster.UserType,
                            ChannelSlabMasterID = UserSlab.ChannelSlabMaster.ChannelSlabMasterID,
                            UserChannelSlabMappingID = UserSlab.UserChannelSlabMappingID,
                            UplineUserID = UserSlab.UplineUserID
                        }).ToList();
            }
        }

        internal UserChannelSlabMapping SelectByID(int UserSlabMappingID)
        {
            using (var context = new DatabaseContext())
            {
                return context.UserChannelSlabMapping.Find(UserSlabMappingID);
            }
        }

        internal ViewModel SelectByUserSlabMappingID(int UserChannelSlabMappingID)
        {
            List<ViewModel> list = Select();

            return (from UserSlab in list
                    where UserSlab.UserChannelSlabMappingID == UserChannelSlabMappingID
                    select UserSlab).FirstOrDefault();
        }

        internal ViewModel SelectByUserID(int UserID)
        {
            List<ViewModel> list = Select();

            return (from UserSlab in list
                    where UserSlab.UserID == UserID
                    select UserSlab).FirstOrDefault();
        }

        internal List<ViewModel> SelectbyUplineID(int UplineUserID)
        {
            List<ViewModel> list = Select();

            return (from UserSlab in list
                    where UserSlab.UplineUserID == UplineUserID
                    select UserSlab).ToList();
        }

        internal ViewModel SelectSlabNameByUserID(int UserID)
        {
            List<UserMaster> Userlist = new UserDAL().Select();
            int parentID = Userlist.FirstOrDefault(U => U.UserID == UserID).ParentID;
            int SuperparentID = Userlist.FirstOrDefault(U => U.UserID == parentID).ParentID;
            List<ViewModel> list = Select();

            ViewModel response = null;
            response = (from UserSlab in list
                        where UserSlab.UserID == UserID
                        select UserSlab).FirstOrDefault();
            if (response == null)
            {
                response = (from UserSlab in list
                            where UserSlab.UserID == parentID
                            select UserSlab).FirstOrDefault();
            }
            if (response == null)
            {
                response = (from UserSlab in list
                            where UserSlab.UserID == SuperparentID
                            select UserSlab).FirstOrDefault();
            }

            return response;
        }
    }
}