import { PostAPI } from 'Components/Helper/Helpers';

export const SelectBank = async (request) => {
    return PostAPI(request, 'PaysprintAEPS/SelectBank');
};

export const CashWithdrawal = async (request) => {
    return PostAPI(request, 'PaysprintAEPS/CashWithdrawal');
};

export const AadharPay = async (request) => {
    return PostAPI(request, 'PaysprintAEPS/AadharPay');
};

export const BalanceEnquiry = async (request) => {
    return PostAPI(request, 'PaysprintAEPS/BalanceEnquiry');
};

export const MiniStatement = async (request) => {
    return PostAPI(request, 'PaysprintAEPS/MiniStatement');
};

export const SelectURL = async (request) => {
    return PostAPI(request, 'PaysprintAEPS/UserOnbording');
};

export const DailyAuthentication = async (request) => {
    return PostAPI(request, 'PaysprintAEPS/DailyAuthentication');
};
export const UserOnbordingStatus = async (request) => {
    return PostAPI(request, 'PaysprintAEPS/UserOnbordingStatus');
};
export const DailyAuthenticationManual = async (request) => {
    return PostAPI(request, 'PaysprintAEPS/DailyAuthenticationManual');
};
export const Select = async (request) => {
    return PostAPI(request, 'DailyAuthentication/Select');
};
export const TxnAuthentication = async (request) => {
    return PostAPI(request, 'PaysprintAEPS/TxnAuthentication');
};
export const Paysprint_CashWithdrawalWithAuthencity = async (request) => {
    return PostAPI(
        request,
        'PaysprintAEPS/Paysprint_CashWithdrawalWithAuthencity'
    );
};
