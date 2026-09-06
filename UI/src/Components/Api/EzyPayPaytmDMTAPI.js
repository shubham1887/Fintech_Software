import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'EzyPayPaytmDMT/SenderRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'EzyPayPaytmDMT/SenderRegistrationSubmitOTP');
};

export const SenderRegistrationResendOTP = async (request) => {
    return PostAPI(request, 'EzyPayPaytmDMT/SenderRegistrationResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'EzyPayPaytmDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'EzyPayPaytmDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'EzyPayPaytmDMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'EzyPayPaytmDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'EzyPayPaytmDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'EzyPayPaytmDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'DMTBank/Select');
};
