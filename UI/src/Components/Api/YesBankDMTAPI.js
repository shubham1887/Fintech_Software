import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'YesBankDMT/SenderRegistration');
};

export const Select = async (request) => {
    return PostAPI(request, 'YesBankDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'YesBankDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'YesBankDMT/DeleteBeneficiary');
};

export const BeneficiaryOTPValidate = async (request) => {
    return PostAPI(request, 'YesBankDMT/BeneficiaryOTPValidate');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'YesBankDMT/ViewBeneficiary');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'YesBankDMT/MoneyTransfer');
};

export const Refund = async (request) => {
    return PostAPI(request, 'YesBankDMT/RefundSendOTP');
};