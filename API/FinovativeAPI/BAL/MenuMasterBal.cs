using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.MenuMaster;
using Project.Response.MenuMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class MenuMasterBal
    {
        private readonly IMapper _mapper;

        public MenuMasterBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        public MenuMasterBal()
        {
        }

        internal List<ViewModel> Select(out string failureMessage)
        {
            failureMessage = MenuMessages.NotFound;
            List<ViewModel> response = null;
            List<MenuMaster> list = new List<MenuMaster>();

            try
            {
                list = new MenuMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list == null || list.Count <= 0) return response;

            try
            {
                response = _mapper.Map<List<ViewModel>>(list);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<ViewModel> SelectParentMenu(out string failureMessage)
        {
            failureMessage = MenuMessages.NotFound;
            List<ViewModel> response = null;
            List<MenuMaster> list = new List<MenuMaster>();

            try
            {
                list = new MenuMasterDal().SelectParentMenu();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list == null || list.Count <= 0) return response;

            try
            {
                response = _mapper.Map<List<ViewModel>>(list);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Insert(InsertRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            failureMessage = MenuMessages.AlreadyExists;
            if (AlreadyExists(mainRequest)) return failureMessage;

            failureMessage = MenuMessages.InsertFail;
            MenuMaster ObjModel = new MenuMaster
            {
                Name = mainRequest.Name,
                Url = mainRequest.Url,
                Description = mainRequest.Description,
                MenuSequence = Convert.ToDecimal(mainRequest.MenuSequence),
                Icon = mainRequest.Icon,
                IsAdmin = mainRequest.IsAdmin,
                IsSuperDistributer = mainRequest.IsSuperDistributer,
                Level = mainRequest.Level,
                IsDistributer = mainRequest.IsDistributer,
                GUID = Guid.NewGuid().ToString("D").ToUpper(),
                ParentMenuGUID = new MenuMasterDal().SelectGUIDByMenuID(mainRequest.ParentMenuID),
                IsRetailer = mainRequest.IsRetailer,
                IsAPIUser = mainRequest.IsAPIUser,
                IsSupport = mainRequest.IsSupport
            };

            int Result = -1;
            try
            {
                Result = new MenuMasterDal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (Result <= 0) return failureMessage;
            failureMessage = StatusType.Ok;
            return MenuMessages.InsertSuccess;
        }

        private bool AlreadyExists(InsertRequest mainRequest)
        {
            bool flag = false;
            List<MenuMaster> response = null;

            try
            {
                response = new MenuMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response == null || response.Count <= 0)
                return flag;

            MenuMaster ObjModel = null;
            try
            {
                ObjModel = response.FirstOrDefault(r => r.Name == mainRequest.Name
                                                && r.Url == mainRequest.Url
                                                && r.Description == mainRequest.Description && r.IsAdmin == mainRequest.IsAdmin
                                                && r.IsSuperDistributer == mainRequest.IsSuperDistributer && r.IsDistributer == mainRequest.IsDistributer && r.IsRetailer == mainRequest.IsRetailer && r.IsSupport == mainRequest.IsSupport);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ObjModel == null)
                return flag;

            return true;
        }

        internal string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            failureMessage = MenuMessages.NotFound;

            MenuMaster ObjModel = null;
            try
            {
                ObjModel = new MenuMasterDal().SelectByID(mainRequest.MenuMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            failureMessage = MenuMessages.UpdateFail;

            ObjModel.Name = mainRequest.Name;
            ObjModel.Url = mainRequest.Url;
            ObjModel.Description = mainRequest.Description;
            ObjModel.Icon = mainRequest.Icon;
            ObjModel.IsAdmin = mainRequest.IsAdmin;
            ObjModel.IsSuperDistributer = mainRequest.IsSuperDistributer;
            ObjModel.IsDistributer = mainRequest.IsDistributer;
            ObjModel.IsRetailer = mainRequest.IsRetailer;
            ObjModel.IsAPIUser = mainRequest.IsAPIUser;
            ObjModel.IsSupport = mainRequest.IsSupport;
            //ObjModel.ParentMenuID = mainRequest.ParentMenuID;
            ObjModel.MenuSequence = Convert.ToDecimal(mainRequest.MenuSequence);
            ObjModel.Level = mainRequest.Level;

            int Result = -1;
            try
            {
                Result = new MenuMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (Result <= 0) return failureMessage;
            failureMessage = StatusType.Ok;
            return MenuMessages.UpdateSuccess;
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel response = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;
            failureMessage = MenuMessages.NotFound;
            try
            {
                response = new MenuMasterDal().SelectByMenuMasterID(mainRequest.MenuMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Delete(DeleteRequest mainRequest, out string failureMessage)
        {
            string response = MenuMessages.NotFound;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            MenuMaster ObjModel = null;
            try
            {
                ObjModel = new MenuMasterDal().SelectByID(mainRequest.MenuMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ObjModel == null) return failureMessage;
            int Result = -1;
            try
            {
                Result = new MenuMasterDal().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = MenuMessages.DeleteFail;
            if (Result <= 0)

                return failureMessage;

            failureMessage = StatusType.Ok;
            return MenuMessages.DeleteSuccess;
        }

        public List<Response.SupportUserAccessDetail.Parent> GetMenuHeirarchy()
        {
            List<Response.SupportUserAccessDetail.Parent> Menus = new List<Response.SupportUserAccessDetail.Parent>();
            List<MenuMaster> ParentMenuList = new MenuMasterDal().SelectChildMenu("0");
            if (ParentMenuList == null)
                return Menus;

            foreach (MenuMaster p in ParentMenuList)
            {
                Response.SupportUserAccessDetail.Parent parent = new Response.SupportUserAccessDetail.Parent
                {
                    Name = p.Name,
                    Url = p.Url,
                    Icon = p.Icon,
                    Usertypeid = GetUserTypeIDsFormMenu(p)
                };
                List<MenuMaster> ChildMenuListLevel1 = new MenuMasterDal().SelectChildByLevel(p.GUID, 1);

                foreach (MenuMaster c in ChildMenuListLevel1)
                {
                    Response.SupportUserAccessDetail.Child child = new Response.SupportUserAccessDetail.Child
                    {
                        Usertypeid = GetUserTypeIDsFormMenu(c),
                        Name = c.Name,
                        Url = c.Url
                    };
                    parent.Children.Add(child);
                }
                Menus.Add(parent);
            }
            return Menus;
        }

        private List<int> GetUserTypeIDsFormMenu(MenuMaster menu)
        {
            List<int> Usertypeids = new List<int>();
            if (menu.IsAdmin) Usertypeids.Add((int)UserType.Admin);
            if (menu.IsSuperDistributer) Usertypeids.Add((int)UserType.SuperDistributer);
            if (menu.IsDistributer) Usertypeids.Add((int)UserType.Distributer);
            if (menu.IsRetailer) Usertypeids.Add((int)UserType.Retailer);
            if (menu.IsAPIUser) Usertypeids.Add((int)UserType.APIUser);
            if (menu.IsSupport) Usertypeids.Add((int)UserType.Support);
            return Usertypeids;
        }
    }
}