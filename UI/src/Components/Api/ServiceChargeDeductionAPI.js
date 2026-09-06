import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'UserServiceCharge/Report');
};
export const GetServiceCharge = async (request) => {
    return PostAPI(request, 'UserServiceCharge/SelectServiceCharge');
};
export const Deduct = async (request) => {
    return PostAPI(request, 'UserServiceCharge/Deduct');
};
export const SelectCommissionCharge = async (request) => {
    return PostAPI(request, 'UserServiceCharge/SelectCommissionCharge');
};
