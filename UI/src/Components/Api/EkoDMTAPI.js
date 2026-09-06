import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'EkoDMT/SenderRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'EkoDMT/SenderRegistrationSubmitOTP');
};

export const SenderRegistrationResendOTP = async (request) => {
    return PostAPI(request, 'EkoDMT/SenderRegistrationResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'EkoDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'EkoDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'EkoDMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'EkoDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'EkoDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'EkoDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'EkoDMT/SelectBank');
};

export const UserOnbording = async (request) => {
    return PostAPI(request, 'EkoDMT/UserOnbording');
};

export const VerifyOTPRefund = async (request) => {
    return PostAPI(request, 'EkoDMT/VerifyOTPRefund');
};
