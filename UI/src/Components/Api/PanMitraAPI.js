import { PostAPI } from 'Components/Helper/Helpers';

export const Registration = async (request) => {
    return PostAPI(request, 'PanMitra/Registration');
};
export const CouponBuy = async (request) => {
    return PostAPI(request, 'PanMitra/CouponBuy');
};
export const CouponStatusCheck = async (request) => {
    return PostAPI(request, 'PanMitra/CouponStatusCheck');
};
export const SelectByUserID = async (request) => {
    return PostAPI(request, 'PanMitraOnboardedUser/SelectByUserID');
};

export const Authorization = async (request) => {
    return PostAPI(request, 'PanMitra/Authorization');
};
