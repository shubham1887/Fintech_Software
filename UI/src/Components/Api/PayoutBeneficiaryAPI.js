import { PostAPI } from 'Components/Helper/Helpers';

export const BeneAdd = async (request) => {
    return PostAPI(request, 'PayoutBeneficiary/Add');
};

export const VerifyOTP = async (request) => {
    return PostAPI(request, 'PayoutBeneficiary/VerifyOTP');
};

export const ResendOTP = async (request) => {
    return PostAPI(request, 'PayoutBeneficiary/ResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'PayoutBeneficiary/Select');
};

export const SelectBeneficiary = async (request) => {
    return PostAPI(request, 'PayoutBeneficiary/SelectBeneficiary');
};

export const UpdateStatus = async (request) => {
    return PostAPI(request, 'PayoutBeneficiary/UpdateStatus');
};

export const DeleteBene = async (request) => {
    return PostAPI(request, 'PayoutBeneficiary/Delete');
};

export const PayoutReceipt = async (request) => {
    return PostAPI(request, 'PayoutBeneficiary/PayoutReceipt');
};