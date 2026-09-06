import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'MasterMoneyPureDMT/SenderRegistration');
};

export const SenderRegistrationGetOTP = async (request) => {
    return PostAPI(request, 'MasterMoneyPureDMT/SenderRegistrationGetOTP');
};

export const ResendOTP = async (request) => {
    return PostAPI(request, 'MasterMoneyPureDMT/ResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'MasterMoneyPureDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'MasterMoneyPureDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'MasterMoneyPureDMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'MasterMoneyPureDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'MasterMoneyPureDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'MasterMoneyPureDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'MasterMoneyPureDMT/SelectBank');
};
