using Project.Model;
using Project.Processing;
using Project.Response.KYCApprove;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class KYCApprovedDal
    {

        private readonly string Key = "_KYCApprovedDetails";

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from KYC in context.KYCApprovedDetails
                        orderby KYC.KYCApprovedDetailsID descending                       
                        select new ViewModel
                        {
                            Remark = KYC.Remark,
                            ApproveUserID = KYC.ApporvedUserID,
                            UserID = KYC.UserID,

                        }).ToList();
            }
        }

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(KYCApprovedDetails KYCApprovedDetails)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.KYCApprovedDetails.Add(KYCApprovedDetails);
                context.SaveChanges();
                return KYCApprovedDetails.KYCApprovedDetailsID;
            }
        }

        internal List<ViewModel> Select()
        {
            var _KYC = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> details = _KYC.GetOrCreate(Key, () => SelectWithoutCache());
            return details;
        }
    }
}