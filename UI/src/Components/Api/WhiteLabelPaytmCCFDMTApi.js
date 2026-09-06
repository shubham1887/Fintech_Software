import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'WhiteLabelPaytmCCF/SenderRegistration');
};

export const SenderRegistrationGetOTP = async (request) => {
    return PostAPI(request, 'WhiteLabelPaytmCCF/SenderRegistrationGetOTP');
};

export const BeneficiaryRegistrationGetOTP = async (request) => {
    return PostAPI(request, 'WhiteLabelPaytmCCF/BeneficiaryRegistrationGetOTP');
};

export const DeleteBeneficiaryGetOTP = async (request) => {
    return PostAPI(request, 'WhiteLabelPaytmCCF/DeleteBeneficiaryGetOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'WhiteLabelPaytmCCF/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'WhiteLabelPaytmCCF/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'WhiteLabelPaytmCCF/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'WhiteLabelPaytmCCF/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'WhiteLabelPaytmCCF/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'WhiteLabelPaytmCCF/MoneyTransfer');
};
