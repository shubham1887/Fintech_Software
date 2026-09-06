import { PostAPI } from 'Components/Helper/Helpers';

export const DailyAuthentication = async (request) => {
    return PostAPI(request, 'HitachiYesBankAEPS/Authentication');
};

export const CashWithdrawal = async (request) => {
    return PostAPI(request, 'HitachiYesBankAEPS/CashWithdrawal');
};

export const BalanceEnquiry = async (request) => {
    return PostAPI(request, 'HitachiYesBankAEPS/BalanceEnquiry');
};

export const MiniStatement = async (request) => {
    return PostAPI(request, 'HitachiYesBankAEPS/MiniStatement');
};