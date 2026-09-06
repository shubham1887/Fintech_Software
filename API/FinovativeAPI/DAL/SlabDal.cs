using Project.Model;
using Project.Processing;
using Project.Response.Slab;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class SlabDAL
    {
        private static string key = "_SlabMaster";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<MasterViewModel>>();
            cache.Remove(key);
        }

        internal int Insert(SlabMaster SlabRequest)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.SlabMaster.Add(SlabRequest);
                context.SaveChanges();
                return SlabRequest.SlabMasterID;
            }
        }

        internal int Update(SlabMaster SlabRequest)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.SlabMaster.Update(SlabRequest);
                context.SaveChanges();
                return SlabRequest.SlabMasterID;
            }
        }

        internal List<MasterViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<MasterViewModel>>();

            List<MasterViewModel> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<MasterViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from Slab in context.SlabMaster
                        where Slab.IsDelete == false
                        select new MasterViewModel
                        {
                            Name = Slab.SlabName,
                            SlabID = Slab.SlabMasterID,
                            IsActive = Slab.IsActive,
                        }).ToList();
            }
        }

        internal SlabMaster SelectByID(int SlabID)
        {
            using (var context = new DatabaseContext())
            {
                return context.SlabMaster.Find(SlabID);
            }
        }

        internal bool IsDuplicateName(string slabName)
        {
            List<MasterViewModel> list = Select();
            MasterViewModel obj = (from item in list
                                   where item.Name.EqualsIgnoreCase(slabName)
                                   select item).FirstOrDefault();

            if (obj == null)
                return true;
            return false;
        }
    }
}