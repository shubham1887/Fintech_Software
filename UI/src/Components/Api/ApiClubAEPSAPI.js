import { PostAPI } from 'Components/Helper/Helpers';

export const SelectBank = async (request) => {
    return PostAPI(request, 'ApiClubAEPS/SelectBank');
};

export const CashWithdrawal = async (request) => {
    return PostAPI(request, 'ApiClubAEPS/CashWithdrawal');
};

export const AadharPay = async (request) => {
    return PostAPI(request, 'ApiClubAEPS/AadharPay');
};

export const BalanceEnquiry = async (request) => {
    return PostAPI(request, 'ApiClubAEPS/BalanceEnquiry');
};

export const MiniStatement = async (request) => {
    return PostAPI(request, 'ApiClubAEPS/MiniStatement');
};
export const CheckUserAgentID = async (request) => {
    return PostAPI(request, 'ApiClubAEPS/CheckUserAgentID');
};
export const SelectState = async (request) => {
    return PostAPI(request, 'ApiClubAEPS/SelectState');
};

export const UserOnbording = async (request) => {
    return PostAPI(request, 'ApiClubAEPS/UserOnbording');
};

export const EKYCSendOTP = async (request) => {
    return PostAPI(request, 'ApiClubAEPS/EKYCSendOTP');
};

export const EKYCValidateOTP = async (request) => {
    return PostAPI(request, 'ApiClubAEPS/EKYCValidateOTP');
};

export const EKYCReSendOTP = async (request) => {
    return PostAPI(request, 'ApiClubAEPS/EKYCReSendOTP');
};

export const EKYCBiomateric = async (request) => {
    return PostAPI(request, 'ApiClubAEPS/EKYCBiomatric');
};
