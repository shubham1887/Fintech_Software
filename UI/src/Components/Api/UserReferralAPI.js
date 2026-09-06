import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'ReferralRegistration/Select');
};

export const SelectURL = async (request) => {
    return PostAPI(request, 'ReferralRegistration/SelectURL');
};

export const SelectReferredUser = async (request) => {
    return PostAPI(request, 'ReferralRegistration/SelectReferredUser');
};
