using Project.Model;
using Project.Processing;
using Project.Response.MenuMaster;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class MenuMasterDal
    {
        public static readonly string CacheKey = "_MenuMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<MenuMaster>>();
            cache.Remove(CacheKey);
        }

        internal List<MenuMaster> Select()
        {
            RemoveCache();
            var _list = new MemoryCacheProcessing<List<MenuMaster>>();

            List<MenuMaster> list = _list.GetOrCreate(CacheKey, () => SelectWithoutCache());

            return list;
        }

        private List<MenuMaster> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.MenuMaster.ToList();
        }

        internal MenuMaster SelectByID(int MenuMasterID)
        {
            return Select().FirstOrDefault(M => M.MenuMasterID == MenuMasterID);
        }

        internal ViewModel SelectByMenuMasterID(int MenuMasterID)
        {
            List<MenuMaster> menulist = Select();
            return (from menu in menulist
                    where menu.MenuMasterID == MenuMasterID
                    select new ViewModel
                    {
                        MenuMasterID = menu.MenuMasterID,
                        MenuSequence = menu.MenuSequence,
                        ParentMenuID = SelectMenuIDByGUID(menu.ParentMenuGUID),
                        Name = menu.Name,
                        Level = menu.Level,
                        Url = menu.Url,
                        Icon = menu.Icon,
                        IsAdmin = menu.IsAdmin,
                        IsSuperDistributer = menu.IsSuperDistributer,
                        IsDistributer = menu.IsDistributer,
                        IsRetailer = menu.IsRetailer,
                        IsAPIUser = menu.IsRetailer,
                        IsSupport = menu.IsSupport,
                        Description = menu.Description
                    }).FirstOrDefault();
        }

        internal List<MenuMaster> SelectByUserTypeID(int UserTypeID)
        {
            List<MenuMaster> list = Select();
            if (UserTypeID == (int)UserType.Admin)
                return list.Where(l => l.IsAdmin == true).ToList();

            if (UserTypeID == (int)UserType.SuperDistributer)
                return list.Where(l => l.IsSuperDistributer == true).ToList();

            if (UserTypeID == (int)UserType.Distributer)
                return list.Where(l => l.IsDistributer == true).ToList();

            if (UserTypeID == (int)UserType.Retailer)
                return list.Where(l => l.IsRetailer == true).ToList();

            if (UserTypeID == (int)UserType.APIUser)
                return list.Where(l => l.IsAPIUser == true).ToList();

            if (UserTypeID == (int)UserType.Support)
                return list.Where(l => l.IsSupport == true).ToList();

            return list;
        }

        internal List<MenuMaster> SelectChildByLevel(string ParentMenuGUID, int Level)
        {
            return Select().Where(m => m.ParentMenuGUID == ParentMenuGUID && m.Level == Level).ToList();
        }

        internal List<MenuMaster> SelectChildMenu(string ParentMenuGUID)
        {
            return Select().Where(m => m.ParentMenuGUID == ParentMenuGUID).ToList();
        }

        internal int Delete(MenuMaster objModel)
        {
            using var context = new DatabaseContext();
            context.MenuMaster.Remove(objModel);
            context.SaveChanges();
            return objModel.MenuMasterID;
        }

        internal int Insert(MenuMaster ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.MenuMaster.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.MenuMasterID;
        }

        internal int Update(MenuMaster ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.MenuMaster.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.MenuMasterID;
        }

        internal List<MenuMaster> SelectParentMenu()
        {
            List<MenuMaster> list = Select();

            List<MenuMaster> response = new List<MenuMaster>();

            response = list.Where(m => m.MenuSequence == 0 && m.Level == 1).ToList();
            return response;
        }

        internal string SelectGUIDByMenuID(int MenuMasterID)
        {
            MenuMaster menu = Select().FirstOrDefault(m => m.MenuMasterID == MenuMasterID);
            if (menu == null)
                return "0";
            return menu.GUID;
        }

        internal int SelectMenuIDByGUID(string ParentMenuGUID)
        {
            MenuMaster menu = Select().FirstOrDefault(m => m.GUID == ParentMenuGUID);
            if (menu == null)
                return 0;
            return menu.MenuMasterID;
        }
    }
}