using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;
using Project.Response.PayoutVendorBeneficiaryMapping;

namespace Project.DAL
{
    public class PayoutVendorBeneficiaryMappingDal
    {
        private readonly string key = "_PayoutVendorBeneficiaryMapping";

        internal int Insert(PayoutVendorBeneficiaryMapping ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.PayoutVendorBeneficiaryMapping.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.PayoutVendorBeneficiaryMappingID;
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

        internal string SelectByVendorID(int BeneficiaryID)
        {
            List<ViewModel> list = Select();

            ViewModel obj = (from bene in list
                                  where bene.PayoutBeneficiaryID == BeneficiaryID                                       
                                  orderby bene.PayoutBeneficiaryID descending
                                  select bene).FirstOrDefault();

            return obj?.PayoutVendorBeneficiaryID ?? "";
        }

        internal ViewModel SelectByBeneID(int BeneID)
        {
            List<ViewModel> list = Select();
            ViewModel obj;

            obj = (from bene in list
                   where bene.PayoutBeneficiaryID == BeneID
                   orderby bene.PayoutVendorBeneficiaryID descending
                   select bene).FirstOrDefault();

            return obj;
        }    

        internal ViewModel CheckDuplicate(int PayoutBeneficaryID)
        {
            List<ViewModel> list = Select();

            ViewModel response = null;

            response = (from Detail in list
                        where Detail.PayoutBeneficiaryID == PayoutBeneficaryID
                        select Detail).FirstOrDefault();

            return response;
        }

        private List<PayoutVendorBeneficiaryMapping> SelectAll()
        {
            using var context = new DatabaseContext();
            return context.PayoutVendorBeneficiaryMapping.ToList();
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<PayoutVendorBeneficiaryMapping> list = new PayoutVendorBeneficiaryMappingDal().SelectAll();
            using (var context = new DatabaseContext())
            {
                return (from mapping in list

                        select new ViewModel
                        {
                            PayoutVendorBeneficiaryMappingID = mapping.PayoutVendorBeneficiaryMappingID,
                            ProviderID = mapping.ProviderID,
                            PayoutVendorBeneficiaryID = mapping.PayoutVendorBeneficiaryID,
                            PayoutBeneficiaryID = mapping.PayoutBeneficaryID,
                        }).ToList();
            }
        }
    }
}