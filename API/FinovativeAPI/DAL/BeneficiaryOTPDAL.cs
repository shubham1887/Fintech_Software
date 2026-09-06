using Project.Model;
using System.Linq;

namespace Project.DAL
{
    public class BeneficiaryOTPDAL
    {
        public int Insert(BeneficiaryOTP ObjModel)
        {
            using (var context = new DatabaseContext())
            {
                context.BeneficiaryOTP.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.BeneficiaryOTPID;
            }
        }

        internal int Update(BeneficiaryOTP ObjModel)
        {
            using (var context = new DatabaseContext())
            {
                context.BeneficiaryOTP.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.BeneficiaryOTPID;
            }
        }

        internal BeneficiaryOTP SelectByID(int BeneficiaryID)
        {
            BeneficiaryOTP obj;
            using (var context = new DatabaseContext())
            {
                obj = (from beneficiaryOTP in context.BeneficiaryOTP
                       where beneficiaryOTP.BeneficiaryID == BeneficiaryID
                       //&& beneficiaryOTP.UserID == UserID // need discussion here about, is sender will consume dmt on one retailer acount or many
                       orderby beneficiaryOTP.BeneficiaryOTPID descending
                       select beneficiaryOTP).FirstOrDefault();
            }
            return obj;
        }
    }
}