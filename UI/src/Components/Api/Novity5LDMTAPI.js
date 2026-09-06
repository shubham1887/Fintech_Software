import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'Novity5LDMT/SenderRegistration');
};
export const AgentRegistration = async (request) => {
    return PostAPI(request, 'Novity5LDMT/AgentRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'Novity5LDMT/ValidateSenderRegistrationOTP');
};

export const SenderRegistrationResendOTP = async (request) => {
    return PostAPI(request, 'Novity5LDMT/SenderRegistrationResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'Novity5LDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'Novity5LDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'Novity5LDMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'Novity5LDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'Novity5LDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'Novity5LDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'Novity5LDMT/SelectBank');
};

export const CheckAgentRegistration = async (request) => {
    return PostAPI(request, 'Novity5LDMT/CheckAgentRegistration');
};

export const VerifyOTPRefund = async (request) => {
    return PostAPI(request, 'Novity5LDMT/VerifyOTPRefund');
};

export const BeneficiaryOTPValidate = async (request) => {
    return PostAPI(request, 'Novity5LDMT/ValidateBeneficiaryOTP');
};

export const BeneficiaryAddResendOTP = async (request) => {
    return PostAPI(request, 'Novity5LDMT/BeneficiaryAddResendOTP');
};

export const ValidateDeleteBeneficiaryOTP = async (request) => {
    return PostAPI(request, 'Novity5LDMT/ValidateDeleteBeneficiaryOTP');
};

export const BeneficiaryDeleteResendOTP = async (request) => {
    return PostAPI(request, 'Novity5LDMT/BeneficiaryDeleteResendOTP');
};
