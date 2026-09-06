using Project.Model;
using System.Linq;

namespace Project.DAL
{
    public class PayoutBeneficiaryOTPDal
    {
        public int Insert(PayoutBeneficiaryOTP ObjModel)
        {
            using (var context = new DatabaseContext())
            {
                context.PayoutBeneficiaryOTP.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.PayoutBeneficiaryOTPID;
            }
        }

        internal int Update(PayoutBeneficiaryOTP ObjModel)
        {
            using (var context = new DatabaseContext())
            {
                context.PayoutBeneficiaryOTP.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.PayoutBeneficiaryOTPID;
            }
        }

        internal PayoutBeneficiaryOTP SelectByID(int PayoutBeneficaryID)
        {
            PayoutBeneficiaryOTP obj;
            using (var context = new DatabaseContext())
            {
                obj = (from beneficiaryOTP in context.PayoutBeneficiaryOTP
                       where beneficiaryOTP.PayoutBeneficaryID == PayoutBeneficaryID
                       orderby beneficiaryOTP.PayoutBeneficaryID descending
                       select beneficiaryOTP).FirstOrDefault();
            }
            return obj;
        }
    }
}