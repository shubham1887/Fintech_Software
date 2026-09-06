using Project.Model;
using Project.Processing;
using Project.Response.CallbackMaster;
using Project.StaticModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class CallbackMasterDal
    {
        private readonly string key = "_CallbackMaster";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        public int Insert(CallbackMaster Model)
        {
            removeCache();
            int Result = -1;
            using (var context = new DatabaseContext())
            {
                context.CallbackMaster.Add(Model);
                context.SaveChanges();
                Result = Model.CallbackMasterID;
            }
            return Result;
        }

        public int Update(CallbackMaster Model)
        {
            removeCache();
            int Result = -1;
            using (var context = new DatabaseContext())
            {
                context.CallbackMaster.Update(Model);
                context.SaveChanges();
                Result = Model.CallbackMasterID;
            }
            return Result;
        }

        internal List<ViewModel> Select()
        {
            var _List = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<ViewModel> list = null;

            using (var context = new DatabaseContext())
            {
                list = (from Callback in context.CallbackMaster
                        join service in context.ServiceMaster on
                         Callback.ServiceID equals service.ServiceID
                        select new ViewModel
                        {
                            UserID = Callback.UserID,
                            CallbackURL = Callback.CallbackURL,
                            ServiceName = service.Name,
                            CallbackMasterID = Callback.CallbackMasterID,
                            ServiceID = Callback.ServiceID,
                            Date = Callback.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        }).ToList();
            }
            return list;
        }

        internal ViewModel SelectByUserIDAndServiceID(int userID, int serviceID)
        {
            return Select().FirstOrDefault(C => C.UserID == userID && C.ServiceID == serviceID);
        }

        internal List<ViewModel> SelectByUserID(int UserID)
        {
            List<ViewModel> Record = null;
            List<ViewModel> list = Select();
            List<UserMaster> userList = new UserDAL().Select();
            Record = (from Callback in list
                      join user in userList on Callback.UserID equals user.UserID

                      select new ViewModel
                      {
                          UserID = Callback.UserID,
                          CallbackURL = Callback.CallbackURL,
                          ServiceName = Callback.ServiceName,
                          CallbackMasterID = Callback.CallbackMasterID,
                          ServiceID = Callback.ServiceID,
                          Date = Callback.Date,
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

        internal int Delete(int CallbackMasterID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.CallbackMaster.Remove(context.CallbackMaster.Find(CallbackMasterID));
                return context.SaveChanges();
            }
        }

        internal ViewModel SelectByID(int callbackMasterID)
        {
            ViewModel Record = null;
            List<ViewModel> list = Select();
            List<UserMaster> userList = new UserDAL().Select();
            return Record = (from Callback in list

                             join user in userList on Callback.UserID equals user.UserID
                             where Callback.CallbackMasterID == callbackMasterID
                             select new ViewModel
                             {
                                 CallbackURL = Callback.CallbackURL,
                                 ServiceName = Callback.ServiceName,
                                 CallbackMasterID = Callback.CallbackMasterID,
                                 ServiceID = Callback.ServiceID,
                                 Date = Callback.Date,
                                 UserID = Callback.UserID,
                                 FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                             }).FirstOrDefault();
        }

        internal string SelectCallbackURL(int UserID, int ServiceID)
        {
            ViewModel obj = SelectByUserIDAndServiceID(UserID, ServiceID);// Select().FirstOrDefault(U => U.UserID == UserID);
            if (obj == null)
            {
                obj = Select().FirstOrDefault(U => U.UserID == UserID);
                if (obj == null)
                {
                    return "";
                }
            }

            return obj.CallbackURL;
        }
    }
}