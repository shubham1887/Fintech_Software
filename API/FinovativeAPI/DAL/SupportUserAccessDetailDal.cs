using Project.Model;
using Project.Processing;
using Project.Response.SupportUserAccessDetail;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class SupportUserAccessDetailDal
    {
        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(CacheKeys.SupportUserAccessDetail);
        }

        internal int Insert(SupportUserAccessDetail objModel)
        {
            RemoveCache();

            using (var context = new DatabaseContext())
            {
                context.SupportUserAccessDetail.Add(objModel);
                context.SaveChanges();
                return objModel.SupportUserAccessDetailID;
            }
        }

        internal int Update(SupportUserAccessDetail objModel)
        {
            RemoveCache();

            using (var context = new DatabaseContext())
            {
                context.SupportUserAccessDetail.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal SupportUserAccessDetail SelectByID(int SupportUserAccessDetailID)
        {
            using (var context = new DatabaseContext())
            {
                return context.SupportUserAccessDetail.Find(SupportUserAccessDetailID);
            }
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(CacheKeys.SupportUserAccessDetail, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from detail in context.SupportUserAccessDetail

                        select new ViewModel
                        {
                            SupportUserAccessDetailID = detail.SupportUserAccessDetailID,
                            MenuGUID = detail.MenuGUID,
                            Controller = detail.Controller,
                            Action = detail.Action,
                            UserID = detail.UserID,
                        }).ToList();
            }
        }

        internal bool SelectByUserIDwithSupportUserAccessMasterID(int userID, int supportUserAccessMasterID)
        {
            List<ViewModel> list = Select();

            bool obj = (from item in list
                        where item.UserID == userID
                        && item.SupportUserAccessMasterID == supportUserAccessMasterID
                        select item.IsActive).FirstOrDefault();

            return obj;
        }

        internal bool SelectForIsAuthorize(string controller, string action, int userID)
        {
            RemoveCache();
            List<ViewModel> list = Select();

            ViewModel obj = list.FirstOrDefault(item => item.UserID == userID && item.Controller == controller
                                      && item.Action == action);

            return obj == null ? false : true;
        }

        internal SupportUserAccessDetail CheckDuplicate(string MenuGUID, int userID, string controller, string action)
        {
            using (var context = new DatabaseContext())
            {
                return context.SupportUserAccessDetail.FirstOrDefault
                    (S => S.UserID == userID && S.Controller == controller && S.Action == action && S.MenuGUID == MenuGUID);
            }
        }

        internal int Delete(SupportUserAccessDetail objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.SupportUserAccessDetail.Remove(objModel);
            return context.SaveChanges();
        }

        internal int SelectID(string MenuGUID, int UserID, string Controller, string Action)
        {
            List<ViewModel> list = Select();

            return (from item in list
                    where item.UserID == UserID
                    && item.MenuGUID == MenuGUID
                    && item.Controller == Controller
                    && item.Action == Action
                    select item.SupportUserAccessDetailID).FirstOrDefault();
        }
    }
}