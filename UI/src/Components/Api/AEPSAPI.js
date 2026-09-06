import { PostAPI } from 'Components/Helper/Helpers';

export const ActiveAEPS = async (request) => {
    return PostAPI(request, 'AEPS/SelectActiveAEPS');
};
export const MultiActiveAEPS = async (request) => {
    return PostAPI(request, 'AEPS/SelectMultiActiveAEPS');
};
export const SelectBank = async (request) => {
    return PostAPI(request, 'PaytmAEPS/SelectBank');
};

export const BalanceEnquiry = async (request) => {
    return PostAPI(request, 'PaytmAEPS/BalanceEnquiry');
};

export const MiniStatement = async (request) => {
    return PostAPI(request, 'PaytmAEPS/MiniStatement');
};

export const CashWithdrawal = async (request) => {
    return PostAPI(request, 'PaytmAEPS/CashWithdrawal');
};

export const SelectBankAlle = async (request) => {
    return PostAPI(request, 'AlleAEPS/SelectBank');
};

export const BalanceEnquiryAlle = async (request) => {
    return PostAPI(request, 'AlleAEPS/BalanceEnquiry');
};

export const CashWithdrawalAlle = async (request) => {
    return PostAPI(request, 'AlleAEPS/CashWithdrawal');
};

export const FunctionalValidationAlle = async (request) => {
    return PostAPI(request, 'AlleAEPS/FunctionalValidation');
};
export const StatusCheckAlle = async (request) => {
    return PostAPI(request, 'AlleAEPS/StatusCheck');
};
export const SelectPIDOption = async (request) => {
    return PostAPI(request, 'AEPS/SelectPIDOption');
};
