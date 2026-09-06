import { PostAPI } from 'Components/Helper/Helpers';

export const CashFreeUPITransfer = async (request) => {
    return PostAPI(request, 'CashFreeUPITransfer/TransferV2');
};

export const UPITransferMyNetLink = async (request) => {
    return PostAPI(request, 'MyNetLinkUPITransfer/Transfer');
};

export const UPITransferClickPay = async (request) => {
    return PostAPI(request, 'ClickPayUPITransfer/Transfer');
};

export const RazorPayUPITransfer = async (request) => {
    return PostAPI(request, 'RazorPayUPITransfer/Transfer');
};

export const InstantPayUPITransfer = async (request) => {
    return PostAPI(request, 'InstantPayUPITransfer/Transfer');
};

export const ICICI_CompisiteUPITransfer = async (request) => {
    return PostAPI(request, 'ICICICompisiteUPITransfer/Transfer');
};

export const Transfer = async (request) => {
    return PostAPI(request, 'CommonUPITransfer/Transfer');
};

export const UpiTransferWithSender = async (request) => {
    return PostAPI(request, 'CommonUPITransfer/UpiTransferWithSender');
};
