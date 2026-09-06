import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'EpocketDMT/SenderRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'EpocketDMT/SenderRegistrationSubmitOTP');
};

export const SenderRegistrationResendOTP = async (request) => {
    return PostAPI(request, 'EpocketDMT/SenderRegistrationResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'EpocketDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'EpocketDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'EpocketDMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'EpocketDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'EpocketDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'EpocketDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'EpocketDMT/SelectBank');
};

export const VerifyOTPRefund = async (request) => {
    return PostAPI(request, 'EpocketDMT/VerifyOTPRefund');
};
