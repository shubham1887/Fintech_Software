import { PostAPI } from 'Components/Helper/Helpers';

export const GenerateOTP = async (request) => {
    return PostAPI(request, 'PaysprintCashDeposit/GenerateOTP');
};

export const Transaction = async (request) => {
    return PostAPI(request, 'PaysprintCashDeposit/Transaction');
};
