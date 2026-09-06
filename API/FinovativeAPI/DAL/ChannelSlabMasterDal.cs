using Project.Model;
using Project.Processing;
using Project.Response.Channel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ChannelSlabMasterDal
    {
        public static string Key
        { get { return "_ ChannelSlabMaster"; } }

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<MasterViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(ChannelSlabMaster SlabRequest)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.ChannelSlabMaster.Add(SlabRequest);
                context.SaveChanges();
                return SlabRequest.ChannelSlabMasterID;
            }
        }

        internal int Update(ChannelSlabMaster SlabRequest)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.ChannelSlabMaster.Update(SlabRequest);
                return context.SaveChanges();
            }
        }

        internal List<MasterViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<MasterViewModel>>();

            List<MasterViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<MasterViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from Slab in context.ChannelSlabMaster
                        where Slab.IsDelete == false
                        select new MasterViewModel
                        {
                            Name = Slab.SlabName,
                            ChannelSlabMasterID = Slab.ChannelSlabMasterID,
                            IsActive = Slab.IsActive,
                            UplineUserID = Slab.UplineUserID
                        }).ToList();
            }
        }

        internal bool CheckDuplicateName(string slabName, int UplineUserID)
        {
            List<MasterViewModel> list = Select();
            MasterViewModel obj = (from item in list
                                   where item.UplineUserID == UplineUserID
                                   && item.Name.EqualsIgnoreCase(slabName)
                                   select item).FirstOrDefault();

            if (obj == null)
                return true;
            return false;
        }

        internal List<MasterViewModel> SelectByUserID(int UserID)
        {
            List<MasterViewModel> list = Select();
            return (from item in list
                    where item.UplineUserID == UserID
                    select item).ToList();
        }

        internal ChannelSlabMaster SelectByID(int SlabID)
        {
            using (var context = new DatabaseContext())
            {
                return context.ChannelSlabMaster.Find(SlabID);
            }
        }
    }
}