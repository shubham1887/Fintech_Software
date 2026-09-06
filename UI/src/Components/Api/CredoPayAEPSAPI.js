import { PostAPI } from 'Components/Helper/Helpers';

export const CashWithdrawal = async (request) => {
    return PostAPI(request, 'CredoPayAEPS/CashWithdrawal');
};

export const BalanceEnquiry = async (request) => {
    return PostAPI(request, 'CredoPayAEPS/BalanceEnquiry');
};

export const MiniStatement = async (request) => {
    return PostAPI(request, 'CredoPayAEPS/MiniStatement');
};

export const Authentication = async (request) => {
    return PostAPI(request, 'CredoPayAEPS/Authentication');
};
