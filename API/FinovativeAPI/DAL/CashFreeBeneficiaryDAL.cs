using Project.Model;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CashFreeBeneficiaryDAL
    {
        internal CashFreeBeneficiary SelectByGUID(string guid)
        {
            CashFreeBeneficiary obj;
            using (var context = new DatabaseContext())
            {
                obj = (from bene in context.CashFreeBeneficiary
                       where bene.GUID == guid
                       && bene.IsActive == true
                       orderby bene.CashFreeBeneficiaryID descending
                       select bene).FirstOrDefault();
            }
            return obj;
        }

        internal List<CashFreeBeneficiary> SelectBySenderID(int SenderID)
        {
            List<CashFreeBeneficiary> obj;
            using (var context = new DatabaseContext())
            {
                obj = (from bene in context.CashFreeBeneficiary
                       where bene.SenderID == SenderID
                       && bene.IsActive == true && bene.IsOTPVerified == true
                       orderby bene.CashFreeBeneficiaryID descending
                       select bene).ToList();
            }
            return obj;
        }

        internal int Delete(CashFreeBeneficiary ObjModel)
        {
            using var context = new DatabaseContext();
            context.CashFreeBeneficiary.Remove(ObjModel);
            context.SaveChanges();
            return ObjModel.CashFreeBeneficiaryID;
        }
    }
}