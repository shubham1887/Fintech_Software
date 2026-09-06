import { PostAPI } from 'Components/Helper/Helpers';
export const Renew = async (request) => {
    return PostAPI(request, 'SubscriptionMaster/RenewSubscription');
};
export const SelectResponseMessage = async (request) => {
    return PostAPI(request, 'SubscriptionMaster/SelectResponseMessage');
};

export const SubscriptionURL = async (request) => {
    return PostAPI(request, 'Credential/RenewSubscription');
};
