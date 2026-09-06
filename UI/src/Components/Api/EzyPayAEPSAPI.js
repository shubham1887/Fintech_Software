import { PostAPI } from 'Components/Helper/Helpers';

export const MerchantEnquiry = async (request) => {
    return PostAPI(request, 'EzyPayAEPS/MerchantEnquiry');
};

export const MerchantActivation = async (request) => {
    return PostAPI(request, 'EzyPayAEPS/MerchantActivation');
};

export const GenerateSessionID = async (request) => {
    return PostAPI(request, 'EzyPayAEPS/GenerateSession');
};
