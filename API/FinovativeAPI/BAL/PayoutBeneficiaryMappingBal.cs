using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class PayoutVendorBeneficiaryMappingBal
    {
        internal string Insert(string Name, string VendorBeneficiaryID, int BeneficiaryID)
        {
            string failureMessage;

            Response.PayoutVendorBeneficiaryMapping.ViewModel details = new Response.PayoutVendorBeneficiaryMapping.ViewModel();
            details = new PayoutVendorBeneficiaryMappingDal().CheckDuplicate(BeneficiaryID);
            if(details != null)
            {
                return BeneficiaryMessage.AlreadyExits;
            }

            int ProviderID = PayoutCode.getIDByName(Name);

            PayoutVendorBeneficiaryMapping ObjModel = new PayoutVendorBeneficiaryMapping
            {
                ProviderID = ProviderID,
                PayoutVendorBeneficiaryID = VendorBeneficiaryID,
                PayoutBeneficaryID = BeneficiaryID
            };
            try
            {
                int rowsAfftected = new PayoutVendorBeneficiaryMappingDal().Insert(ObjModel);

                if (rowsAfftected <= 0)
                {
                    return Messages.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            return StatusType.Ok;
        }
    }
}
