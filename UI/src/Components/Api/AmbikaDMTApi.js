import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'AmbikaDMT/SenderRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'AmbikaDMT/SenderRegistrationSubmitOTP');
};

export const SenderRegistrationResendOTP = async (request) => {
    return PostAPI(request, 'AmbikaDMT/SenderRegistrationResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'AmbikaDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'AmbikaDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'AmbikaDMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'AmbikaDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'AmbikaDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'AmbikaDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'AmbikaDMT/SelectBank');
};
