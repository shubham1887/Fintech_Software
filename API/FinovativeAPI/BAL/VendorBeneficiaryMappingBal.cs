using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;
using Project.Response.VendorBeneficiaryMapping;
namespace Project.BAL
{
    public class VendorBeneficiaryMappingBal
    {
        internal string Insert(string DMTName, string VendorBeneficiaryID, int BeneficiaryID)
        {
            string failureMessage;
            ViewModel details = new ViewModel();
            details = new VendorBeneficiaryMappingDal().CheckDuplicate(BeneficiaryID);
            if (details != null)
            {
                return BeneficiaryMessage.AlreadyExits;
            }

            int ProviderID = DMTCode.getIDByName(DMTName);
            VendorBeneficiaryMapping ObjModel = new VendorBeneficiaryMapping
            {
                ProviderID = ProviderID,
                VendorBeneficiaryID = VendorBeneficiaryID,                  
                BeneficiaryID = BeneficiaryID
            };
            try
            {
                int rowsAfftected = new VendorBeneficiaryMappingDal().Insert(ObjModel);

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
