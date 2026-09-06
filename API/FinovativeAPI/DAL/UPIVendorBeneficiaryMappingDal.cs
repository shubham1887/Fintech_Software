using Project.Model;
using Project.Processing;
using Project.Response.UPIVendorBeneficiaryMapping;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UPIVendorBeneficiaryMappingDal
    {
        private readonly string key = "_UPIVendorBeneficiaryMapping";

        internal int Insert(UPIVendorBeneficiaryMapping ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UPIVendorBeneficiaryMapping.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.UPIVendorBeneficiaryMappingID;
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

        private List<UPIVendorBeneficiaryMapping> SelectAll()
        {
            using var context = new DatabaseContext();
            return context.UPIVendorBeneficiaryMapping.ToList();
        }

        internal ViewModel SelectByBeneID(string BeneID)
        {
            List<ViewModel> list = Select();
            ViewModel obj;

            obj = (from bene in list
                   where bene.UPIVendorBeneficiaryID == BeneID                  
                   orderby bene.UPIVendorBeneficiaryMappingID descending
                   select bene).FirstOrDefault();

            return obj;
        }

        internal ViewModel CheckDuplicate(int UPIBeneficaryID)
        {
            List<ViewModel> list = Select();

            ViewModel response = null;

            response = (from Detail in list
                        where Detail.UPIBeneficiaryID == UPIBeneficaryID
                        select Detail).FirstOrDefault();

            return response;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<UPIVendorBeneficiaryMapping> list = new UPIVendorBeneficiaryMappingDal().SelectAll();
            using (var context = new DatabaseContext())
            {
                return (from mapping in list

                        select new ViewModel
                        {
                            UPIVendorBeneficiaryMappingID = mapping.UPIVendorBeneficiaryMappingID,
                            ProviderID = mapping.ProviderID,
                            UPIVendorBeneficiaryID = mapping.UPIVendorBeneficiaryID,
                            UPIBeneficiaryID = mapping.UpiBeneficiaryID,
                        }).ToList();
            }
        }
    }
}
