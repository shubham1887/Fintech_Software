using Project.Model;
using Project.Processing;
using Project.Requests.ServiceWiseSliderSetting;
using Project.Response.ServiceWiseSliderSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ServiceWiseSliderSettingDal
    {
        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ServiceWiseSliderSetting>>();
            cache.Remove(CacheKeys.ServiceWiseSliderSetting);
        }

        internal int Insert(ServiceWiseSliderSetting objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ServiceWiseSliderSetting.Add(objModel);
            context.SaveChanges();
            return objModel.ServiceWiseSliderSettingID;
        }

        internal int Update(ServiceWiseSliderSetting objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ServiceWiseSliderSetting.Update(objModel);
            context.SaveChanges();
            return objModel.ServiceWiseSliderSettingID;
        }

        internal List<ServiceWiseSliderSetting> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<ServiceWiseSliderSetting>>();
            List<ServiceWiseSliderSetting> list = _list.GetOrCreate(CacheKeys.ServiceWiseSliderSetting, () => SelectWithoutCache());
            return list;
        }

        internal ServiceWiseSliderSetting SelectByID(int ServiceWiseSliderSettingID)
        {
            using (var context = new DatabaseContext())
            {
                return context.ServiceWiseSliderSetting.Find(ServiceWiseSliderSettingID);
            }
        }

        internal ViewModel SelectSlidersByID(SelectByIDRequest mainRequest)
        {
            List<ViewModel> returnlist = new List<ViewModel>();
            List<ServiceWiseSliderSetting> list = SelectAll();
            List<Response.Service.ViewModel> serviceList = new ServiceMasterDal().Select();

            returnlist = (from item in list
                          join service in serviceList on item.ServiceID equals service.ServiceID
                          where item.IsDelete == false && item.ServiceWiseSliderSettingID == mainRequest.ServiceWiseSliderSettingID
                          orderby item.ServiceID, item.Priority ascending
                          select new ViewModel
                          {
                              ImagePath = String.Concat(Company.WebAPIURL, item.ImagePath),
                              ServiceWiseSliderSettingID = item.ServiceWiseSliderSettingID,
                              ServiceID = item.ServiceID,
                              ServiceName = service.ServiceName,
                              Description = item.Description,
                              Type = item.Type,
                              ReferenceLink = item.ReferenceLink,
                              Priority = item.Priority,
                              IsActive = item.IsActive,
                          }).ToList();

            return returnlist.FirstOrDefault();
        }

        private List<ServiceWiseSliderSetting> SelectWithoutCache()
        {
            List<ServiceWiseSliderSetting> list = null;
            try
            {
                using var context = new DatabaseContext();
                list = context.ServiceWiseSliderSetting.ToList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return list;
        }

        internal ServiceWiseSliderSetting SelectDuplicate(SliderSettingInsertRequest Slidersetting)
        {
            List<ServiceWiseSliderSetting> list = SelectAll();
            return (from slider in list
                    where slider.Priority == Slidersetting.Priority &&
                    slider.ReferenceLink == Slidersetting.ReferenceLink &&
                    slider.ServiceID == Slidersetting.ServiceID &&
                    slider.Description == Slidersetting.Description &&
                    slider.Type == Slidersetting.Type &&
                    slider.IsDelete == false
                    select slider).FirstOrDefault();
        }

        internal List<ViewModel> SelectByServiceID(SelectRequest mainRequest)
        {
            List<ViewModel> returnlist = new List<ViewModel>();

            List<ServiceWiseSliderSetting> list = SelectAll();
            List<Response.Service.ViewModel> serviceList = new ServiceMasterDal().Select();

            returnlist = (from item in list
                          join service in serviceList on item.ServiceID equals service.ServiceID
                          where item.IsDelete == false
                          orderby item.ServiceID, item.Priority ascending

                          select new ViewModel
                          {
                              ImagePath = String.Concat(Company.WebAPIURL, item.ImagePath),
                              ServiceWiseSliderSettingID = item.ServiceWiseSliderSettingID,
                              ServiceID = item.ServiceID,
                              ServiceName = service.ServiceName,
                              Description = item.Description,
                              Type = item.Type,
                              ReferenceLink = item.ReferenceLink,
                              Priority = item.Priority,
                              IsActive = item.IsActive,
                          }).ToList();
            if (mainRequest.ServiceID > 0)
            {
                returnlist = (from item in returnlist
                              where item.ServiceID == mainRequest.ServiceID
                              select item).ToList();
            }

            return returnlist;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> returnlist = new List<ViewModel>();

            List<ServiceWiseSliderSetting> list = SelectAll();
            List<Response.Service.ViewModel> serviceList = new ServiceMasterDal().Select();

            returnlist = (from item in list
                          join service in serviceList on item.ServiceID equals service.ServiceID
                          where item.IsDelete == false
                          orderby item.ServiceID, item.Priority ascending
                          select new ViewModel
                          {
                              ImagePath = String.Concat(Company.WebAPIURL, item.ImagePath),
                              ServiceWiseSliderSettingID = item.ServiceWiseSliderSettingID,
                              ServiceID = item.ServiceID,
                              ServiceName = service.ServiceName,
                              Description = item.Description,
                              Type = item.Type,
                              ReferenceLink = item.ReferenceLink,
                              Priority = item.Priority,
                              IsActive = item.IsActive,
                          }).ToList();

            return returnlist;
        }

        internal ServiceWiseSliderSetting SelectByImageURL(string path)
        {
            using var context = new DatabaseContext();
            return context.ServiceWiseSliderSetting.FirstOrDefault(U => U.ImagePath == path);
        }
    }
}