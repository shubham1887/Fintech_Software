import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'VastWebDMT/Select');
};

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'VastWebDMT/SenderRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'VastWebDMT/SenderRegistrationSubmitOTP');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'VastWebDMT/AddBeneficiary');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'VastWebDMT/MoneyTransfer');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'VastWebDMT/DeleteBeneficiary');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'VastWebDMT/SelectBank');
};
