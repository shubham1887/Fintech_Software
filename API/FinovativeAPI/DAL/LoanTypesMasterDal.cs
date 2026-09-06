using Project.Model;
using Project.Processing;
using Project.Requests.LeadGenerationMaster;
using System.Collections.Generic;
using System.Linq;


namespace Project.DAL
{
    public class LoanTypesMasterDal
    {
        private readonly string key = "_LoanTypesMasterDal";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<LoanTypes>>();
            cache.Remove(key);
        }

        internal List<LoanTypes> SelectLoanType()
        {
            var _settingList = new MemoryCacheProcessing<List<LoanTypes>>();
            List<LoanTypes> SettingList = _settingList.GetOrCreate(key, () => SelectWithoutCache());
            return SettingList;
        }
        internal string SelectOperatorNameByOperatorID(string OperatorID)
        {
            return SelectLoanType().FirstOrDefault(o => o.LoanTypeMasterID.ToString() == OperatorID).LoanType;
        }
        private List<LoanTypes> SelectWithoutCache()
        {
            using var context = new DatabaseContext();


            return (from setting in context.LoanTypeMaster
                    select new LoanTypes
                    {
                        LoanTypeMasterID = setting.LoanTypeMasterID,
                        LoanType = setting.LoanType,

                    }).ToList();
        }
    }
}
