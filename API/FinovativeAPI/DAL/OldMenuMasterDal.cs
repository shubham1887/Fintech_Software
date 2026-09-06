using Project.Model;
using Project.Processing;
using Project.Response.MenuMaster;

using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OldMenuMasterDal
    {
        public static string CacheKey
        { get { return "_MenuMaster"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(CacheKey);
        }

        internal List<ViewModel> Select()
        {
            RemoveCache();
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(CacheKey, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<ViewModel> list = new List<ViewModel>();
            using var context = new DatabaseContext();
            try
            {
                list = (from menuMaster in context.MenuMaster
                            //  orderby menuMaster.MenuMasterID
                        select new ViewModel
                        {
                            MenuMasterID = menuMaster.MenuMasterID,
                            Name = menuMaster.Name,
                            Url = menuMaster.Url,
                            Icon = menuMaster.Icon,
                            //ParentMenuID = menuMaster.ParentMenuID,
                            IsAdmin = menuMaster.IsAdmin,
                            IsSuperDistributer = menuMaster.IsSuperDistributer,
                            IsDistributer = menuMaster.IsDistributer,
                            IsRetailer = menuMaster.IsRetailer,
                            IsAPIUser = menuMaster.IsAPIUser,
                            IsSupport = menuMaster.IsSupport,
                            Description = menuMaster.Description,
                            MenuSequence = menuMaster.MenuSequence,
                            Level = menuMaster.Level
                        }).ToList();
            }
            catch (System.Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        internal List<ViewModel> SelectByUsertypeID(int UserTypeID)
        {
            List<ViewModel> selectlist = Select();

            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
            {
                list = (from menuMaster in selectlist
                        where menuMaster.ParentMenuID == 0 && menuMaster.Level == 0
                        orderby menuMaster.MenuMasterID
                        select menuMaster).ToList();
            }

            if (UserTypeID == (int)UserType.Admin)
            {
                list = (from item in list
                        where item.IsAdmin == true
                        select item).ToList();
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                list = (from item in list
                        where item.IsSuperDistributer == true
                        select item).ToList();
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                list = (from item in list
                        where item.IsDistributer == true
                        select item).ToList();
            }
            if (UserTypeID == (int)UserType.Retailer)
            {
                list = (from item in list
                        where item.IsRetailer == true
                        select item).ToList();
            }
            if (UserTypeID == (int)UserType.APIUser)
            {
                list = (from item in list
                        where item.IsAPIUser == true
                        select item).ToList();
            }
            if (UserTypeID == (int)UserType.Support)
            {
                list = (from item in list
                        where item.IsSupport == true
                        select item).ToList();
            }
            return list;
        }

        internal List<ChildrenList> SelectChildernList(int MenuMasterID, int UserTypeID)
        {
            List<ViewModel> selectlist = Select();
            List<ChildrenList> list = new List<ChildrenList>();
            if (UserTypeID == -99)
            {
                list = (from menuMaster in selectlist
                        where menuMaster.ParentMenuID == MenuMasterID
                        orderby menuMaster.MenuMasterID
                        select new ChildrenList
                        {
                            Name = menuMaster.Name,
                            Url = menuMaster.Url,
                            Icon = menuMaster.Icon,
                            MenuSequence = menuMaster.MenuSequence
                        }).ToList();
                return list;
            }
            if (UserTypeID == (int)UserType.Admin)
            {
                list = (from menuMaster in selectlist
                        where menuMaster.ParentMenuID == MenuMasterID
                        && menuMaster.IsAdmin == true
                        orderby menuMaster.MenuMasterID

                        select new ChildrenList
                        {
                            Name = menuMaster.Name,
                            Url = menuMaster.Url,
                            Icon = menuMaster.Icon,
                            MenuSequence = menuMaster.MenuSequence
                        }).ToList();
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                list = (from menuMaster in selectlist
                        where menuMaster.ParentMenuID == MenuMasterID
                        && menuMaster.IsSuperDistributer == true
                        orderby menuMaster.MenuMasterID

                        select new ChildrenList
                        {
                            Name = menuMaster.Name,
                            Url = menuMaster.Url,
                            Icon = menuMaster.Icon,
                            MenuSequence = menuMaster.MenuSequence
                        }).ToList();
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                list = (from menuMaster in selectlist
                        where menuMaster.ParentMenuID == MenuMasterID
                        && menuMaster.IsDistributer == true
                        orderby menuMaster.MenuMasterID

                        select new ChildrenList
                        {
                            Name = menuMaster.Name,
                            Url = menuMaster.Url,
                            Icon = menuMaster.Icon,
                            MenuSequence = menuMaster.MenuSequence
                        }).ToList();
            }
            if (UserTypeID == (int)UserType.Retailer)
            {
                list = (from menuMaster in selectlist
                        where menuMaster.ParentMenuID == MenuMasterID
                        && menuMaster.IsRetailer == true
                        orderby menuMaster.MenuMasterID

                        select new ChildrenList
                        {
                            Name = menuMaster.Name,
                            Url = menuMaster.Url,
                            Icon = menuMaster.Icon,
                            MenuSequence = menuMaster.MenuSequence
                        }).ToList();
            }
            if (UserTypeID == (int)UserType.APIUser)
            {
                list = (from menuMaster in selectlist
                        where menuMaster.ParentMenuID == MenuMasterID
                        && menuMaster.IsAPIUser == true
                        orderby menuMaster.MenuMasterID

                        select new ChildrenList
                        {
                            Name = menuMaster.Name,
                            Url = menuMaster.Url,
                            Icon = menuMaster.Icon,
                            MenuSequence = menuMaster.MenuSequence
                        }).ToList();
            }
            if (UserTypeID == (int)UserType.Support)
            {
                list = (from menuMaster in selectlist
                        where menuMaster.ParentMenuID == MenuMasterID
                        && menuMaster.IsSupport == true
                        orderby menuMaster.MenuMasterID

                        select new ChildrenList
                        {
                            Name = menuMaster.Name,
                            Url = menuMaster.Url,
                            Icon = menuMaster.Icon,
                            MenuSequence = menuMaster.MenuSequence
                        }).ToList();
            }

            return list;
        }

        internal MenuMaster SelectByID(int menuMasterID)
        {
            using var context = new DatabaseContext();
            return context.MenuMaster.Find(menuMasterID);
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
    }
}