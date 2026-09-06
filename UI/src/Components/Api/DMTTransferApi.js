import { PostAPI } from 'Components/Helper/Helpers';

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'CommonDMT/MoneyTransfer');
};

export const ICICIDMTMoneyTransfer = async (request) => {
    return PostAPI(request, 'ICICIComposite/DMTMoneyTransfer');
};

export const PaytmDMTMoneyTransfer = async (request) => {
    return PostAPI(request, 'Paytm/DMTMoneyTransfer');
};

export const OpenBankDMTMoneyTransfer = async (request) => {
    return PostAPI(request, 'OpenBank/DMTMoneyTransfer');
};

export const PayUDMTMoneyTransfer = async (request) => {
    return PostAPI(request, 'PayU/DMTMoneyTransfer');
};

export const JaiJuiDMTMoneyTransfer = async (request) => {
    return PostAPI(request, 'JaiJuiDMT/DMTMoneyTransfer');
};

export const PrimePayDMTMoneyTransfer = async (request) => {
    return PostAPI(request, 'PrimePayDMT/DMTMoneyTransfer');
};

export const ZplusPayoutDMTMoneyTransfer = async (request) => {
    return PostAPI(request, 'ZPlusPayout/DMTMoneyTransfer');
};
export const HyptoDMTMoneyTransfer = async (request) => {
    return PostAPI(request, 'HyptoDMT/DMTMoneyTransfer');
};
export const RazorpayDMTMoneyTransfer = async (request) => {
    return PostAPI(request, 'RazorpayDMT/DMTMoneyTransfer');
};
export const InstantPayPayoutDMTMoneyTransfer = async (request) => {
    return PostAPI(request, 'InstantPayPayoutDMT/DMTMoneyTransfer');
};
