import { PostAPI } from 'Components/Helper/Helpers';

export const UserOnbording = async (request) => {
    return PostAPI(request, 'MorePayOnboarding/UserOnbording');
};
export const SendEKYCOTP = async (request) => {
    return PostAPI(request, 'MorePayOnboarding/SendEKYCOTP');
};
export const ValidateEKYCOTP = async (request) => {
    return PostAPI(request, 'MorePayOnboarding/ValidateEKYCOTP');
};
