import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'EzyPayV1DMT/SenderRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'EzyPayV1DMT/SenderRegistrationSubmitOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'EzyPayV1DMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'EzyPayV1DMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'EzyPayV1DMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'EzyPayV1DMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'EzyPayV1DMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'EzyPayV1DMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'EzyPayV1DMT/SelectBank');
};
