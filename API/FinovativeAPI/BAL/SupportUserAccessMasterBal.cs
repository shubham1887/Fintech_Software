using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.SupportUserAccessDetail;
using Project.Response.SupportUserAccessDetail;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class SupportUserAccessMasterBal
    {
        internal string Insert(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            updateRequest.MenuGUID = new MenuMasterDal().SelectGUIDByMenuID(updateRequest.MenuMasterID);
            SupportUserAccessMaster ObjModel = new SupportUserAccessMaster
            {
                UserID = updateRequest.UserID,
                MenuGUID = updateRequest.MenuGUID,
                IsActive = updateRequest.IsActive,
                MenuMasterID = updateRequest.MenuMasterID,
            };
            SupportUserAccessMaster model = null;
            try
            {
                model = new SupportUserAccessMasterDal().CheckDuplicate(updateRequest.UserID, updateRequest.MenuGUID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            int rowsAffected;
            if (model == null)
            {
                try
                {
                    rowsAffected = new SupportUserAccessMasterDal().Insert(ObjModel);
                    if (rowsAffected <= 0)
                        return failureMessage = SupportUserMessage.AccessFail;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }
            else
            {
                model.IsActive = updateRequest.IsActive;
                model.MenuGUID = updateRequest.MenuGUID;
                model.UpdatedDOC = DateTime.Now;
                try
                {
                    rowsAffected = new SupportUserAccessMasterDal().Update(model);
                    if (rowsAffected <= 0)
                        return failureMessage = SupportUserMessage.AccessFail;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }
            if (updateRequest.IsActive)
            {
                new SupportUserAccessDetailBal().Insert(updateRequest);
            }
            if (!updateRequest.IsActive)
            {
                Task.Run(() => new SupportUserAccessDetailBal().Delete(updateRequest));
            }
            return failureMessage = StatusType.Ok;
        }

        private List<Parent> GetMenuHeirarchy()
        {
            List<Response.SupportUserAccessDetail.Parent> Menus = new List<Response.SupportUserAccessDetail.Parent>();
            List<MenuMaster> ParentMenuList = null;
            try
            {
                ParentMenuList = new MenuMasterDal().SelectChildMenu("0");
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ParentMenuList == null || ParentMenuList.Count <= 0)
                return Menus;

            List<int> Usertypeid = new List<int> { (int)UserType.Support };

            foreach (MenuMaster p in ParentMenuList)
            {
                Response.SupportUserAccessDetail.Parent parent = new Response.SupportUserAccessDetail.Parent
                {
                    Name = p.Name,
                    Url = p.Url,
                    Icon = p.Icon,
                    Usertypeid = Usertypeid,
                    MenuMasterID = p.MenuMasterID
                };
                List<MenuMaster> ChildMenuListLevel1 = null;
                try
                {
                    ChildMenuListLevel1 = new MenuMasterDal().SelectChildByLevel(p.GUID, 1);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (ChildMenuListLevel1 == null || ChildMenuListLevel1.Count <= 0) continue;
                foreach (MenuMaster c in ChildMenuListLevel1)
                {
                    Response.SupportUserAccessDetail.Child child = new Response.SupportUserAccessDetail.Child
                    {
                        Usertypeid = Usertypeid,
                        Name = c.Name,
                        Url = c.Url,
                        MenuMasterID = c.MenuMasterID
                    };
                    parent.Children.Add(child);
                }
                Menus.Add(parent);
            }

            if (Menus == null || Menus.Count <= 0) return Menus;

            return Menus;
        }

        internal List<Parent> GetMenu(int userID)
        {
            List<Parent> menus = null;
            try
            {
                menus = GetMenuHeirarchy();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (menus == null || menus.Count <= 0)
                return menus;
            List<SupportUserAccessMaster> supportAccessList = null;
            try
            {
                supportAccessList = new SupportUserAccessMasterDal().SelectByUserID(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (supportAccessList == null || supportAccessList.Count <= 0) return menus;

            foreach (SupportUserAccessMaster obj in supportAccessList)
            {
                foreach (Parent menu in menus)
                {
                    if (menu.MenuMasterID == obj.MenuMasterID)
                        menu.IsActive = obj.IsActive;

                    if (menu.Children.Count <= 0)
                        continue;
                    foreach (Child subMenu in menu.Children)
                    {
                        if (subMenu.MenuMasterID == obj.MenuMasterID)
                            subMenu.IsActive = obj.IsActive;
                    }
                }
            }
            return menus;
        }
    }
}