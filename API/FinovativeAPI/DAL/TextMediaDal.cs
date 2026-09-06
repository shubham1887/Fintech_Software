using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class TextMediaDal
    {
        private readonly string Key = "_TextMedia";

        internal List<RichTextMedia> Select()
        {
            var _list = new MemoryCacheProcessing<List<RichTextMedia>>();
            List<RichTextMedia> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<RichTextMedia> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.RichTextMedia
                        orderby setting.ID descending
                        select new RichTextMedia
                        {
                            ID = setting.ID,
                            Key = setting.Key,
                            Content = setting.Content,
                        }).ToList();
            }
        }

        internal RichTextMedia SelectByID(int ID)
        {
            using var context = new DatabaseContext();
            return context.RichTextMedia.Find(ID);
        }

        internal int Update(RichTextMedia model)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.RichTextMedia.Update(model);
                return context.SaveChanges();
            }
        }

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<RichTextMedia>>();
            cache.Remove(Key);
        }

        internal RichTextMedia SelectByKey(string Key)
        {
            List<RichTextMedia> list = Select();
            RichTextMedia item = list.FirstOrDefault(u => u.Key == Key);
            return item;
        }
    }
}