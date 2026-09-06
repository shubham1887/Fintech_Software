import { PostAPI } from 'Components/Helper/Helpers';

export const GenerateOTP = async (request) => {
    return PostAPI(request, 'PaysprintWalletTopUp/GenerateOTP');
};

export const Transaction = async (request) => {
    return PostAPI(request, 'PaysprintWalletTopUp/Transaction');
};
