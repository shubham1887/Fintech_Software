using Poject.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.PayoutBeneficiary;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class PayoutVendorBeneficiaryDal
    {
        internal List<Response.PayoutBeneficiary.BeneficiaryViewModel> SelectByVendorID(PayoutBeneficiaryReportRequest mainRequest, int UserID)
        {
            List<PayoutBeneficiary> list = new PayoutBeneficaryDal().Select();

            List<Response.PayoutVendorBeneficiaryMapping.ViewModel> mappinglist = new PayoutVendorBeneficiaryMappingDal().Select();

            List<Response.PayoutBeneficiary.BeneficiaryViewModel> obj;

            obj = (from bene in list
                   join vendor in mappinglist on bene.PayoutBeneficaryID equals vendor.PayoutBeneficiaryID
                   where 
                    bene.IsOTPVerified == true
                   orderby bene.PayoutBeneficaryID descending

                   select new Response.PayoutBeneficiary.BeneficiaryViewModel
                   {                     
                       Name = bene.AccountHolderName,
                       BankName = bene.BankName,
                       AccountNo = bene.AccountNo,
                       UserID = bene.UserID,
                       Status = bene.Status,
                       IFSC = bene.IFSC,
                       BeneficiaryID = bene.PayoutBeneficaryID,
                       IsOTPVerified = bene.IsOTPVerified,
                       IsNameVerified = bene.IsNameVerified,

                   }).ToList();

            if (mainRequest.UserID > Company.AdminUserID)
            {
                obj = (from item in obj
                       where item.UserID == mainRequest.UserID
                       select item).ToList();
            }

            if (!mainRequest.Status.EqualsIgnoreCase(StatusType.ALL))
            {
                obj = (from item in obj
                       where item.Status == mainRequest.Status
                       select item).ToList();
            }

            return obj;
        }
    }
}