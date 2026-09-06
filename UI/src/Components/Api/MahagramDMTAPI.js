import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'MahagramDMT/SenderRegistration');
};

export const SenderRegistrationGetOTP = async (request) => {
    return PostAPI(request, 'MahagramDMT/SenderRegistrationGetOTP');
};

export const SenderRegistrationResendOTP = async (request) => {
    return PostAPI(request, 'MahagramDMT/SenderRegistrationResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'MahagramDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'MahagramDMT/AddBeneficiary');
};

export const BeneficiaryVerifyOTP = async (request) => {
    return PostAPI(request, 'MahagramDMT/BeneficiaryVerifyOTP');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'MahagramDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'MahagramDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'MahagramDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'MahagramDMT/SelectBank');
};
