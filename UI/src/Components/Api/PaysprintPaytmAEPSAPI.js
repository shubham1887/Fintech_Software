import { PostAPI } from 'Components/Helper/Helpers';

export const SelectBank = async (request) => {
    return PostAPI(request, 'PaysprintPaytmAEPS/SelectBank');
};

export const SelectState = async (request) => {
    return PostAPI(request, 'PaysprintPaytmAEPS/SelectState');
};

export const CashWithdrawal = async (request) => {
    return PostAPI(request, 'PaysprintPaytmAEPS/CashWithdrawal');
};

export const BalanceEnquiry = async (request) => {
    return PostAPI(request, 'PaysprintPaytmAEPS/BalanceEnquiry');
};

export const MiniStatement = async (request) => {
    return PostAPI(request, 'PaysprintPaytmAEPS/MiniStatement');
};
export const MerchantOnboarding = async (request) => {
    return PostAPI(request, 'PaysprintPaytmAEPS/MerchantOnboarding');
};

export const MerchantOnboardingStatus = async (request) => {
    return PostAPI(request, 'PaysprintPaytmAEPS/MerchantOnboardingStatus');
};

export const DailyAuthentication = async (request) => {
    return PostAPI(request, 'PaysprintPaytmAEPS/TwoFactorAuthentication');
};
