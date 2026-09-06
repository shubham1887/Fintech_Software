import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'RoundPayDMT/SenderRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'RoundPayDMT/SenderRegistrationSubmitOTP');
};

export const SenderRegistrationResendOTP = async (request) => {
    return PostAPI(request, 'RoundPayDMT/SenderRegistrationResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'RoundPayDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'RoundPayDMT/AddBeneficiary');
};

export const BeneVerifyOTP = async (request) => {
    return PostAPI(request, 'RoundPayDMT/AddBeneficiary');
};
export const BeneResendOTP = async (request) => {
    return PostAPI(request, 'RoundPayDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'RoundPayDMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'RoundPayDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'RoundPayDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'RoundPayDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'RoundPayDMT/SelectBank');
};
