using Project.Message;
using Project.StaticModel;

namespace Project.Processing.PayoutProcessingV1
{
    public class GetPayoutProviders
    {
        public int getProviderID(Response.PayoutSetting.ViewModel viewModel, decimal Amount)
        {
            if (new Processing.PayoutProcessingV1.PayoutProcessingV1().IsOnlyVerification(Amount))
            {
                return new Processing.PayoutProcessingV1.PayoutProcessingV1().SelectProviderIDForVerification();
            }

            return PayoutCode.getIDByName(viewModel.Name);
        }

        public string isProviderCodeSetForPayout(Response.PayoutSetting.ViewModel viewModel)
        {
            int providerID;
            if (PayoutCode.providerMap.TryGetValue(viewModel.Name.ToLower(), out providerID))
            {
                if (providerID <= 0)
                {
                    return PayoutSettingMessage.InvalidPayoutCode;
                }
            }
            else
            {
                return PayoutSettingMessage.InvalidPayoutCode;
            }
            return StatusType.Ok;
        }
    }
}