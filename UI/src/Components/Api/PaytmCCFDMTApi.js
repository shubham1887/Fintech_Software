import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'PaytmCCFDMT/SenderRegistration');
};

export const SenderRegistrationGetOTP = async (request) => {
    return PostAPI(request, 'PaytmCCFDMT/SenderRegistrationGetOTP');
};

export const BeneficiaryRegistrationGetOTP = async (request) => {
    return PostAPI(request, 'PaytmCCFDMT/BeneficiaryRegistrationGetOTP');
};

export const DeleteBeneficiaryGetOTP = async (request) => {
    return PostAPI(request, 'PaytmCCFDMT/DeleteBeneficiaryGetOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'PaytmCCFDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'PaytmCCFDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'PaytmCCFDMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'PaytmCCFDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'PaytmCCFDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'PaytmCCFDMT/MoneyTransfer');
};
