import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'Wallet/Select');
};

export const SelectForPaymentGateway = async (request) => {
    return PostAPI(request, 'Wallet/SelectForPaymentGateway');
};

export const SelectForFundTransfer = async (request) => {
    return PostAPI(request, 'Wallet/SelectForFundTransfer');
};

export const SelectByUserID = async (request) => {
    return PostAPI(request, 'Wallet/SelectByUserID');
};

export const SelectByWalletIDAndReceiverID = async (request) => {
    return PostAPI(request, 'Wallet/SelectByWalletIDAndReceiverID');
};

export const AEPSToOtherWalletTransfer = async (request) => {
    return PostAPI(request, 'WalletToWalletTransfer/AEPSToOther');
};
export const SelectRechargeAndDMT = async (request) => {
    return PostAPI(request, 'Wallet/SelectRechargeAndDMT');
};
export const SelectWalletSummary = async (request) => {
    return PostAPI(request, 'Wallet/SelectForDashboardByUserID');
};
export const SelectServiceWiseWallet = async (request) => {
    return PostAPI(request, 'Wallet/SelectServiceWiseWallet');
};
export const SelectForeCollection = async (request) => {
    return PostAPI(request, 'Wallet/SelectForeCollection');
};
export const SelectForStaticQR = async (request) => {
    return PostAPI(request, 'Wallet/SelectForStaticQR');
};
export const SelectForUserCreation = async (request) => {
    return PostAPI(request, 'Wallet/SelectForUserCreation');
};
