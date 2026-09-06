import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'AlleDMT/SenderRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'AlleDMT/SenderRegistrationSubmitOTP');
};

export const SenderRegistrationResendOTP = async (request) => {
    return PostAPI(request, 'AlleDMT/SenderRegistrationResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'AlleDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'AlleDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'AlleDMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'AlleDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'AlleDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'AlleDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'AlleDMT/SelectBank');
};
