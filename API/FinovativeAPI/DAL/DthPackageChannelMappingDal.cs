using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class DthPackageChannelMappingDal
    {
        private readonly string key = "_DthPackageChannelMapping";

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<DthPackageChannelMapping>>();
            cache.Remove(key);
        }

        internal void Delete(int OfflineProductMasterID, int ChannelCategoryID)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            var Channel = context.DthPackageChannelMapping.Where(U => U.OfflineProductMasterID == OfflineProductMasterID && U.ChannelCategoryID == ChannelCategoryID);
            context.DthPackageChannelMapping.RemoveRange(Channel);
            context.SaveChanges();
        }

        internal async Task<int> Insert(DthPackageChannelMapping dthPackageChannelMapping)
        {
            using var context = new DatabaseContext();
            context.DthPackageChannelMapping.Add(dthPackageChannelMapping);
            return await context.SaveChangesAsync();
        }

        internal List<DthPackageChannelMapping> Select()
        {
            var _list = new MemoryCacheProcessing<List<DthPackageChannelMapping>>();

            List<DthPackageChannelMapping> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<DthPackageChannelMapping> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return (from PackageChannel in context.DthPackageChannelMapping
                    select PackageChannel).ToList();
        }

        internal List<Response.CategoryChannelMapping.ViewModel> SelectByOfflineProductMasterIDandChannelCategoryID(int OfflineProductMasterID, int ChannelCategoryID)
        {
            List<DthPackageChannelMapping> list = Select();
            List<Response.CategoryChannelMapping.ViewModel> categoryChannelMapping = new CategoryChannelMappingDal().Select();

            return (from item in list
                    join categoryChannel in categoryChannelMapping on item.CategoryChannelMappingID equals categoryChannel.CategoryChannelMappingID
                    where item.OfflineProductMasterID == OfflineProductMasterID
                    && item.ChannelCategoryID == ChannelCategoryID
                    select new Response.CategoryChannelMapping.ViewModel
                    {
                        ChannelCategoryID = item.ChannelCategoryID,
                        ChannelName = categoryChannel.ChannelName,
                        ChannelImagePath = categoryChannel.ChannelImagePath.ConcatWebAPIURL(),
                        IsCheckbox = false
                    }).ToList();
        }
    }
}