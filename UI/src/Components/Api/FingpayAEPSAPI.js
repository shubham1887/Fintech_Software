import { PostAPI } from 'Components/Helper/Helpers';

export const SelectBank = async (request) => {
    return PostAPI(request, 'FingpayAEPS/SelectBank');
};

export const CashWithdrawal = async (request) => {
    return PostAPI(request, 'FingpayAEPS/CashWithdrawal');
};

export const AadharPay = async (request) => {
    return PostAPI(request, 'FingpayAEPS/AadharPay');
};

export const BalanceEnquiry = async (request) => {
    return PostAPI(request, 'FingpayAEPS/BalanceEnquiry');
};

export const MiniStatement = async (request) => {
    return PostAPI(request, 'FingpayAEPS/MiniStatement');
};
export const CheckUserAgentID = async (request) => {
    return PostAPI(request, 'FingpayAEPS/CheckUserAgentID');
};
export const SelectState = async (request) => {
    return PostAPI(request, 'FingpayAEPS/SelectState');
};

export const UserOnbording = async (request) => {
    return PostAPI(request, 'FingpayAEPS/UserOnbording');
};

export const EKYCSendOTP = async (request) => {
    return PostAPI(request, 'FingpayAEPS/EKYCSendOTP');
};

export const EKYCValidateOTP = async (request) => {
    return PostAPI(request, 'FingpayAEPS/EKYCValidateOTP');
};

export const EKYCBiomateric = async (request) => {
    return PostAPI(request, 'FingpayAEPS/EKYCBiomatric');
};

export const TwoFactorAuthentication = async (request) => {
    return PostAPI(request, 'FingpayAEPS/TwoFactorAuthentication');
};
export const DailyMasterAuthentication = async (request) => {
    return PostAPI(request, 'FingpayAEPS/DailyMasterAuthentication');
};

