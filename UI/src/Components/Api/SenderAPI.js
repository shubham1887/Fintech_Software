import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'Sender/Add');
};

export const VerifyOTP = async (request) => {
    return PostAPI(request, 'Sender/VerifyOTP');
};

export const ResendOTP = async (request) => {
    return PostAPI(request, 'Sender/ResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'Sender/Select');
};

export const SelectByAccountNumber = async (request) => {
    return PostAPI(request, 'Sender/SelectByAccountNumber');
};

export const ICICISenderSelect = async (request) => {
    return PostAPI(request, 'ICICIComposite/SenderSelect');
};

export const PaytmSenderSelect = async (request) => {
    return PostAPI(request, 'Paytm/SenderSelect');
};

export const OpenBankSenderSelect = async (request) => {
    return PostAPI(request, 'OpenBank/SenderSelect');
};

export const PayUSenderSelect = async (request) => {
    return PostAPI(request, 'PayU/SenderSelect');
};

export const JaiJuiSenderSelect = async (request) => {
    return PostAPI(request, 'JaiJuiDMT/SenderSelect');
};
export const RazorpaySenderSelect = async (request) => {
    return PostAPI(request, 'RazorpayDMT/SenderSelect');
};
export const InstantPayPayoutDMTSenderSelect = async (request) => {
    return PostAPI(request, 'InstantPayPayoutDMT/SenderSelect');
};
