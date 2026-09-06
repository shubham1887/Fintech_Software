import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'CyberPlateDMT/SenderRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'CyberPlateDMT/SenderRegistrationSubmitOTP');
};

export const ResendOTP = async (request) => {
    return PostAPI(request, 'CyberPlateDMT/ResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'CyberPlateDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'CyberPlateDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'CyberPlateDMT/DeleteBeneficiary');
};

export const DeleteBeneficiaryOTPValidation = async (request) => {
    return PostAPI(request, 'CyberPlateDMT/DeleteBeneficiaryOTPValidation');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'CyberPlateDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'CyberPlateDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'CyberPlateDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'CyberPlateDMT/SelectBank');
};
