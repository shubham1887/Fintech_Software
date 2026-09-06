using Project.Model;
using Project.Processing;
using Project.Response.VendorBeneficiaryMapping;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class VendorBeneficiaryMappingDal
    {

        private readonly string key = "_VendorBeneficiaryMapping";

        internal int Insert(VendorBeneficiaryMapping ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.VendorBeneficiaryMapping.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.VendorBeneficiaryMappingID;
        }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        internal List<ViewModel> Select()
        {
            var _List = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        private List<VendorBeneficiaryMapping> SelectAll()
        {
            using var context = new DatabaseContext();
            return context.VendorBeneficiaryMapping.ToList();
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<VendorBeneficiaryMapping> list = new VendorBeneficiaryMappingDal().SelectAll();
            using (var context = new DatabaseContext())
            {
                return (from mapping in list
                         
                        select new ViewModel
                        {
                            VendorBeneficiaryMappingID = mapping.VendorBeneficiaryMappingID,
                            ProviderID = mapping.ProviderID,
                            VendorBeneficiaryID = mapping.VendorBeneficiaryID,
                            BeneficiaryID = mapping.BeneficiaryID,
                        }).ToList();
            }
        }

        internal ViewModel CheckDuplicate(int BeneficiaryID)
        {
            List<ViewModel> list = Select();

            ViewModel response = null;
            response = (from Detail in list
                        where Detail.BeneficiaryID == BeneficiaryID
                        select Detail).FirstOrDefault();

            return response;
        }

        internal string SelectByVendorID(int BeneficiaryID)
        {
            List<ViewModel> list = Select();

            ViewModel obj = (from bene in list
                             where bene.BeneficiaryID == BeneficiaryID
                             orderby bene.BeneficiaryID descending
                             select bene).FirstOrDefault();

            return obj?.VendorBeneficiaryID ?? "";
        }

        internal ViewModel SelectByBeneID(int BeneID)
        {
            List<ViewModel> list = Select();
            ViewModel obj;

            obj = (from bene in list
                   where bene.BeneficiaryID == BeneID
                   orderby bene.VendorBeneficiaryID descending
                   select bene).FirstOrDefault();

            return obj;
        }

    }
}