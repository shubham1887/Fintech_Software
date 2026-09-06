using Project.Model;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UPIVendorBeneficiaryDal
    {
        internal List<Response.UpiBeneficiary.ViewModel> SelectSenderWithVendorBeneID(int SenderID)
        {
            List<UpiBeneficiary> list = new UpiBeneficiaryDal().Select();

            List<Response.UPIVendorBeneficiaryMapping.ViewModel> mappinglist = new UPIVendorBeneficiaryMappingDal().Select();

            List<Response.UpiBeneficiary.ViewModel> obj;

            obj = (from bene in list
                   join vendor in mappinglist on bene.UpiBeneficiaryID equals vendor.UPIBeneficiaryID
                   where bene.UpiSenderID == SenderID
                   && bene.IsActive == true && bene.IsOTPVerified == true
                   orderby bene.UpiBeneficiaryID descending

                   select new Response.UpiBeneficiary.ViewModel
                   {
                       Mobile = bene.SenderMobile,
                       Name = bene.Name,
                       UpiID = bene.UpiID,
                       UpiBeneficiaryID = vendor.UPIVendorBeneficiaryID,
                       BeneficiaryID = bene.GUID.ToString(),
                       IsVerified = bene.IsNameVerified

                   }).ToList();

            return obj;
        }
    }
}