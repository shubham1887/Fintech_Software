import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'MasterMoneyDMT/SenderRegistration');
};

export const SenderRegistrationGetOTP = async (request) => {
    return PostAPI(request, 'MasterMoneyDMT/SenderRegistrationGetOTP');
};

export const ResendOTP = async (request) => {
    return PostAPI(request, 'MasterMoneyDMT/ResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'MasterMoneyDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'MasterMoneyDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'MasterMoneyDMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'MasterMoneyDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'MasterMoneyDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'MasterMoneyDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'MasterMoneyDMT/SelectBank');
};
