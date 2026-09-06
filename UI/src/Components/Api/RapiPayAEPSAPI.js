import { PostAPI } from 'Components/Helper/Helpers';

export const SelectBank = async (request) => {
    return PostAPI(request, 'RapiPayAEPS/SelectBank');
};

export const CashWithdrawal = async (request) => {
    return PostAPI(request, 'RapiPayAEPS/CashWithdrawal');
};

export const AadharPay = async (request) => {
    return PostAPI(request, 'RapiPayAEPS/AadharPay');
};

export const BalanceEnquiry = async (request) => {
    return PostAPI(request, 'RapiPayAEPS/BalanceEnquiry');
};

export const MiniStatement = async (request) => {
    return PostAPI(request, 'RapiPayAEPS/MiniStatement');
};
