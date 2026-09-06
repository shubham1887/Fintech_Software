using Project.Model;
using Project.Processing;
using Project.Response.KycDocumentMaster;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class KycDocumentMasterDal
    {
        public string key
        { get { return "_KycDocumentMaster"; } }

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        internal int Update(KycDocumentMaster objModel)
        {
            using (var context = new DatabaseContext())
            {
                removeCache();
                context.KycDocumentMaster.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal KycDocumentMaster SelectByID(int KycDocumentMasterID)
        {
            using (var context = new DatabaseContext())
            {
                return context.KycDocumentMaster.Find(KycDocumentMasterID);
            }
        }

        internal List<ViewModel> Select()
        {
            removeCache();
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        internal List<KycDocumentMaster> SelectType()
        {
            List<KycDocumentMaster> list = new List<KycDocumentMaster>();
            using (var context = new DatabaseContext())
            {
                list = context.KycDocumentMaster.Where(c => c.IsRequired == true).ToList();
            }
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from obj in context.KycDocumentMaster
                        select new ViewModel
                        {
                            KycDocumentMasterID = obj.KycDocumentMasterID,
                            Label = obj.Label,
                            DocumentType = obj.DocumentType,
                            IsRequired = obj.IsRequired,
                            Remark = obj.Remark,
                            SampleImageURL = obj.SampleImageURL.ConcatWebAPIURL(),
                            RegularExpression = obj.RegularRxpression,
                        }).ToList();
            }
        }

        internal List<ViewModel> SelectRequired()
        {
            List<ViewModel> list = Select();
            return (from item in list
                    where item.IsRequired == true
                    select item).ToList();
        }
    }
}