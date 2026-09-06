import { PostAPI } from 'Components/Helper/Helpers';

export const GenerateOTP = async (request) => {
    return PostAPI(request, 'RetailerToAllFundTransfer/GenerateOTP');
};
export const VerifyFoundTransferOTP = async (request) => {
    return PostAPI(request, 'RetailerToAllFundTransfer/VerifyFoundTransferOTP');
};
export const Transfer = async (request) => {
    return PostAPI(request, 'RetailerToAllFundTransfer/Transfer');
};
export const SelectLast10BySenderID = async (request) => {
    return PostAPI(request, 'RetailerToAllFundTransfer/SelectLast10BySenderID');
};
