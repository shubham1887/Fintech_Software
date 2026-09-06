using Project.Model;
using Project.Processing;
using Project.Requests.SliderMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class SliderMasterDal
    {
        private readonly string key = "_SliderMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<SliderMaster>>();
            cache.Remove(key);
        }

        internal SliderMaster SelectByID(int sliderMasterID)
        {
            using DatabaseContext context = new DatabaseContext();
            return context.SliderMaster.Find(sliderMasterID);
        }

        internal List<SliderMaster> Select()
        {
            var _sliderList = new MemoryCacheProcessing<List<SliderMaster>>();
            List<SliderMaster> sliderList = _sliderList.GetOrCreate(key, () => SelectWithoutCache());
            return sliderList;
        }

        internal List<SliderMaster> GetAppSlider()
        {
            List<SliderMaster> list = Select();

            return list.Where(s => s.Type == PlatformCode.App).Take(5).ToList();
        }

        private List<SliderMaster> SelectWithoutCache()
        {
            using DatabaseContext context = new DatabaseContext();
            return context.SliderMaster.Where(s => s.IsDelete == false).OrderByDescending(s => s.DOC).ToList();
        }

        internal int Delete(SliderMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.SliderMaster.Remove(objModel);
            return context.SaveChanges();
        }

        internal int Insert(SliderMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.SliderMaster.Add(objModel);
            context.SaveChanges();
            return objModel.SliderMasterID;
        }

        internal int Update(SliderMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.SliderMaster.Update(objModel);
            context.SaveChanges();
            return objModel.SliderMasterID;
        }

        internal List<SliderMaster> SelectByType(SelectRequest mainRequest)
        {
            List<SliderMaster> list = Select();
            return list.Where(x => string.Equals(x.Type, mainRequest.Type, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }
    }
}