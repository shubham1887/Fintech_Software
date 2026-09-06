import { PostAPI } from 'Components/Helper/Helpers';

export const SelectBank = async (request) => {
    return PostAPI(request, 'EkoAEPS/SelectBank');
};

export const CashWithdrawal = async (request) => {
    return PostAPI(request, 'EkoAEPS/CashWithdrawal');
};

export const AadharPay = async (request) => {
    return PostAPI(request, 'EkoAEPS/AadharPay');
};

export const BalanceEnquiry = async (request) => {
    return PostAPI(request, 'EkoAEPS/BalanceEnquiry');
};

export const MiniStatement = async (request) => {
    return PostAPI(request, 'EkoAEPS/MiniStatement');
};

export const UserOnbording = async (request) => {
    return PostAPI(request, 'EkoAEPS/UserOnbording');
};

export const CheckUserAgentID = async (request) => {
    return PostAPI(request, 'EkoAEPS/CheckUserAgentID');
};

export const AgentActivate = async (request) => {
    return PostAPI(request, 'EkoAEPS/AgentActivate');
};

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'EkoAEPS/SenderRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'EkoAEPS/SenderRegistrationSubmitOTP');
};

export const SenderRegistrationResendOTP = async (request) => {
    return PostAPI(request, 'EkoAEPS/SenderRegistrationResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'EkoAEPS/Select');
};
