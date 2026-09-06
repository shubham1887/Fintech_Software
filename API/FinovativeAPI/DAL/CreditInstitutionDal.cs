using Project.Model;
using Project.Processing;
using Project.Response.CreditInstitution;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CreditInstitutionDal
    {
        public static string Key = "_CreditInstitutionDal";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal CreditInstitutionMaster SelectByID(int CreditInstitutionMasterID)
        {
            using var context = new DatabaseContext();
            return context.CreditInstitutionMaster.Find(CreditInstitutionMasterID);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.CreditInstitutionMaster
                        orderby setting.CreditInstitutionMasterID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            CreditInstitutionMasterID = setting.CreditInstitutionMasterID,
                            Name = setting.Name,
                            Token = setting.Token,
                            EncryptionKey = setting.EncryptionKey,
                            BranchID = setting.BranchID,
                            AccountNumber = setting.AccountNumber,
                            MerchantID = setting.MerchantID,
                            IsActive = setting.IsActive,

                        }).ToList();
            }
        }


        internal int DeactiveAll()
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.CreditInstitutionMaster.ToList().ForEach(a => a.IsActive = false);
                return context.SaveChanges();
            }
        }

        internal int Update(CreditInstitutionMaster model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.CreditInstitutionMaster.Update(model);
                return context.SaveChanges();
            }
        }

        internal ViewModel SelectActiveBank()
        {
            List<ViewModel> List = Select();
            return List.FirstOrDefault(d => d.IsActive ==true);
        }
    }
}
