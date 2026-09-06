using Project.Model;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class VendorBeneficiaryDal
    {
        internal List<Response.Beneficiary.ViewModel> SelectBySenderID(int SenderID)
        {
            List<Beneficiary> list = new BeneficiaryDAL().Select();

            List<Response.VendorBeneficiaryMapping.ViewModel> mappinglist = new VendorBeneficiaryMappingDal().Select();

            List<Response.Beneficiary.ViewModel> obj;

            obj = (from bene in list
                   join vendor in mappinglist on bene.BeneficiaryID equals vendor.BeneficiaryID
                   where bene.SenderID == SenderID
                   && bene.IsActive == true && bene.IsOTPVerified == true
                   orderby bene.BeneficiaryID descending

                   select new Response.Beneficiary.ViewModel
                   {
                       Mobile = bene.SenderMobile,
                       Name = bene.Name,
                       BankName = bene.BankName,
                       AccountNo = bene.AccountNo,
                       IFSC = bene.IFSC,
                       BeneficiaryID = vendor.VendorBeneficiaryID,
                       DMTBeneficiaryID = bene.GUID,
                       IsVerified = bene.IsNameVerified

                   }).ToList();

            return obj;        
        }
    }
}