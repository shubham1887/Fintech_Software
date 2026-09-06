using Project.Message;
using Project.StaticModel;

namespace Project.Processing.VendorPayProcessing
{
    public class GetVendorPayProviders
    {
        public int getProviderID(Response.BulkPaySetting.ViewModel viewModel)
        {
            return VendorPayCode.getIDByName(viewModel.Name);
        }
        public string getProviderName(int ProviderID)
        {
            return VendorPayCode.getNameByID(ProviderID);
        }

        public string isProviderCodeSetForPayout(Response.BulkPaySetting.ViewModel viewModel)
        {
            int providerID;
            if (VendorPayCode.providerMap.TryGetValue(viewModel.Name.ToLower(), out providerID))
            {
                if (providerID <= 0)
                {
                    return VendorPaySettingMessage.InvalidVendorPayCode;
                }
            }
            else
            {
                return VendorPaySettingMessage.InvalidVendorPayCode;
            }
            return StatusType.Ok;
        }
    }
}