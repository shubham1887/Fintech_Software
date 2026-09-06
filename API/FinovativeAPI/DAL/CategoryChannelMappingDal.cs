using Project.Model;
using Project.Processing;
using Project.Response.CategoryChannelMapping;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CategoryChannelMappingDal
    {
        private readonly string key = "_CategoryChannelMapping";

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        public int Insert(CategoryChannelMapping ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.CategoryChannelMapping.Add(ObjModel);
            return context.SaveChanges();
        }

        internal int Update(CategoryChannelMapping objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.CategoryChannelMapping.Update(objModel);
            return context.SaveChanges();
        }

        internal CategoryChannelMapping SelectByID(int CategoryChannelMappingID)
        {
            using var context = new DatabaseContext();
            return context.CategoryChannelMapping.Find(CategoryChannelMappingID);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return (from channel in context.CategoryChannelMapping
                    join category in context.ChannelCategory on channel.ChannelCategoryID equals category.ChannelCategoryID
                    where channel.IsDelete == false
                    select new ViewModel
                    {
                        CategoryChannelMappingID = channel.CategoryChannelMappingID,
                        ChannelCategoryID = channel.ChannelCategoryID,
                        Category = category.Category,
                        ChannelName = channel.ChannelName,
                        ChannelImagePath = channel.ChannelImagePath,
                        IsDelete = channel.IsDelete,
                        IsCheckbox = true
                    }).ToList();
        }

        internal ViewModel SelectByCategoryChannelMappingID(int CategoryChannelMappingID)
        {
            List<ViewModel> list = Select();

            ViewModel obj = (from item in list
                             where item.CategoryChannelMappingID == CategoryChannelMappingID
                             select item).FirstOrDefault();

            return obj;
        }

        internal List<ViewModel> SelectByChannelCategoryID(int ChannelCategoryID)
        {
            List<ViewModel> list = Select();

            List<ViewModel> responselist = (from item in list
                                            where item.ChannelCategoryID == ChannelCategoryID
                                            select item).ToList();

            return responselist;
        }
    }
}