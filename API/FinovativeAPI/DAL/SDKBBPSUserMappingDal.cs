using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Response.SDKBBPSUserMapping;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinovativeAPI.DAL
{
    public class SDKBBPSUserMappingDal
    {
        public static readonly string Key = "_SDKBBPSUserMapping";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<SDKBBPSUserMapping>>();
            cache.Remove(Key);
        }

        public int Insert(SDKBBPSUserMapping Objmodel)
        {
            RemoveCache();
            int Result = -1;
            using (var context = new DatabaseContext())
            {
                context.SDKBBPSUserMapping.Add(Objmodel);
                context.SaveChanges();
                Result = Objmodel.SDKBBPSUserMappingID;
            }
            return Result;
        }

        public int Update(SDKBBPSUserMapping Objmodel)
        {
            RemoveCache();
            int Result = -1;
            using (var context = new DatabaseContext())
            {
                context.SDKBBPSUserMapping.Update(Objmodel);
                context.SaveChanges();
                Result = Objmodel.SDKBBPSUserMappingID;
            }
            return Result;
        }

        internal List<SDKBBPSUserMapping> Select()
        {
            var _ipList = new MemoryCacheProcessing<List<SDKBBPSUserMapping>>();
            List<SDKBBPSUserMapping> ipList = _ipList.GetOrCreate(Key, () => SelectWithoutCache());
            return ipList;
        }

        private List<SDKBBPSUserMapping> SelectWithoutCache()
        {
            List<SDKBBPSUserMapping> List = null;
            try
            {
                using var context = new DatabaseContext();
                List = context.SDKBBPSUserMapping.ToList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return List;
        }

        internal SDKBBPSUserMapping SelectByUserID(int UserID)
        {
            return Select().Where(IP => IP.UserID == UserID && IP.IsActive == true && IP.IsDelete == false).FirstOrDefault();
        }

        internal int Delete(int SDKBBPSUserMappingID)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.SDKBBPSUserMapping.Remove(context.SDKBBPSUserMapping.Find(SDKBBPSUserMappingID));
            return context.SaveChanges();
        }

        internal List<ViewModel> SelectByUser(int UserID)
        {
            List<ViewModel> Record = null;
            List<SDKBBPSUserMapping> list = Select();
            List<UserMaster> userList = new UserDAL().Select();
            Record = (from Callback in list
                      join user in userList on Callback.UserID equals user.UserID

                      select new ViewModel
                      {
                          UserID = Callback.UserID,
                          ConfirmationCallbackURL = Callback.ConfirmationCallbackURL,
                          SDKBBPSUserMappingID = Callback.SDKBBPSUserMappingID,
                          FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                      }).ToList();

            if (UserID != Company.AdminUserID)
            {
                Record = (from item in Record
                          where item.UserID == UserID
                          select item).ToList();
            }
            return Record;
        }

        internal ViewModel SelectByID(int sDKBBPSUserMappingID)
        {
            List<SDKBBPSUserMapping> list = Select();
            List<UserMaster> userList = new UserDAL().Select();
            return (from Callback in list
                    join user in userList on Callback.UserID equals user.UserID
                    where Callback.SDKBBPSUserMappingID == sDKBBPSUserMappingID
                    select new ViewModel
                    {
                        UserID = Callback.UserID,
                        ConfirmationCallbackURL = Callback.ConfirmationCallbackURL,
                        SDKBBPSUserMappingID = Callback.SDKBBPSUserMappingID,
                        FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                    }).FirstOrDefault();
        }
    }
}