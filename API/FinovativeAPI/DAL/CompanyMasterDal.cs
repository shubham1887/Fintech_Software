using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CompanyMasterDal
    {
        private readonly string key = "_CompanyMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<CompanyMaster>>();
            cache.Remove(key);
        }

        internal List<CompanyMaster> Select()
        {
            var _CompanyDataList = new MemoryCacheProcessing<List<CompanyMaster>>();
            List<CompanyMaster> CompanyDataList = _CompanyDataList.GetOrCreate(key, () => SelectWithoutCache());
            return CompanyDataList;
        }

        private List<CompanyMaster> SelectWithoutCache()
        {
            List<CompanyMaster> list = new List<CompanyMaster>();
            using (var context = new DatabaseContext())
            {
                list = context.CompanyMaster.ToList();
            }
            return list;
        }

        internal CompanyMaster SelectByName(string name)
        {
            List<CompanyMaster> mainList = Select();
            return mainList.FirstOrDefault(c => c.Name.Trim() == name.Trim());
        }

        internal CompanyMaster SelectByID(int companyMasterID)
        {
            return Select().FirstOrDefault(c => c.CompanyMasterID == companyMasterID);
        }

        internal int Update(CompanyMaster dbResponse)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.CompanyMaster.Update(dbResponse);
            context.SaveChanges();
            return dbResponse.CompanyMasterID;
        }
    }
}