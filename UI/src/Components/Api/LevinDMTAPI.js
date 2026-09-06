import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'LevinDMT/Select');
};

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'LevinDMT/SenderRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'LevinDMT/SenderRegistrationSubmitOTP');
};

export const SelectBene = async (request) => {
    return PostAPI(request, 'LevinDMT/ViewBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'LevinDMT/DeleteBeneficiary');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'LevinDMT/MoneyTransfer');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'LevinDMT/AddBeneficiary');
};
