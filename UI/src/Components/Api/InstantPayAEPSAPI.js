import { PostAPI } from 'Components/Helper/Helpers';

export const SelectBank = async (request) => {
    return PostAPI(request, 'InstantPayAEPS/SelectBank');
};

export const CashWithdrawal = async (request) => {
    return PostAPI(request, 'InstantPayAEPS/CashWithdrawal');
};

export const BalanceEnquiry = async (request) => {
    return PostAPI(request, 'InstantPayAEPS/BalanceEnquiry');
};

export const MiniStatement = async (request) => {
    return PostAPI(request, 'InstantPayAEPS/MiniStatement');
};

export const MerchantOnboarding = async (request) => {
    return PostAPI(request, 'InstantPayAEPS/MerchantOnboarding');
};

export const DailyAuthentication = async (request) => {
    return PostAPI(request, 'InstantPayAEPS/TwoFactorAuthentication');
};

export const MerchantOnboardingStatus = async (request) => {
    return PostAPI(request, 'InstantPayAEPS/MerchantOnboardingStatus');
};

export const MerchantOnboardingGenerateOTP = async (request) => {
    return PostAPI(request, 'InstantPayAEPS/MerchantOnboardingGenerateOTP');
};

export const MerchantOnboardingVerifyOTP = async (request) => {
    return PostAPI(request, 'InstantPayAEPS/MerchantOnboardingVerifyOTP');
};
