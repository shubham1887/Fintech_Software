import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'NovityDMT/SenderRegistration');
};
export const AgentRegistration = async (request) => {
    return PostAPI(request, 'NovityDMT/AgentRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'NovityDMT/SenderRegistrationSubmitOTP');
};

export const SenderRegistrationResendOTP = async (request) => {
    return PostAPI(request, 'NovityDMT/SenderRegistrationResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'NovityDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'NovityDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'NovityDMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'NovityDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'NovityDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'NovityDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'NovityDMT/SelectBank');
};

export const CheckAgentRegistration = async (request) => {
    return PostAPI(request, 'NovityDMT/CheckAgentRegistration');
};

export const VerifyOTPRefund = async (request) => {
    return PostAPI(request, 'NovityDMT/VerifyOTPRefund');
};

export const BeneficiaryOTPValidate = async (request) => {
    return PostAPI(request, 'NovityDMT/BeneficiaryOTPValidate');
};

export const BeneficiaryAddResendOTP = async (request) => {
    return PostAPI(request, 'NovityDMT/BeneficiaryAddResendOTP');
};

export const ValidateDeleteBeneficiaryOTP = async (request) => {
    return PostAPI(request, 'NovityDMT/ValidateDeleteBeneficiaryOTP');
};

export const BeneficiaryDeleteResendOTP = async (request) => {
    return PostAPI(request, 'NovityDMT/BeneficiaryDeleteResendOTP');
};
