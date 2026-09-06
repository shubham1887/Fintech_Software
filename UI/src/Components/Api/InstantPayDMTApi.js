import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'InstantPayDMT/SenderRegistration');
};

export const SubmitOTP = async (request) => {
    return PostAPI(request, 'InstantPayDMT/SubmitOTP');
};

export const ResendOTP = async (request) => {
    return PostAPI(request, 'InstantPayDMT/SenderRegistrationResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'InstantPayDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'InstantPayDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'InstantPayDMT/DeleteBeneficiary');
};

export const DeleteBeneficiaryOTPValidation = async (request) => {
    return PostAPI(request, 'InstantPayDMT/DeleteBeneficiaryOTPValidation');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'InstantPayDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'InstantPayDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'InstantPayDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'InstantPayDMT/SelectBank');
};
