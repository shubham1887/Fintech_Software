import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'PaysprintDMT/SenderRegistration');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'PaysprintDMT/DeleteBeneficiary');
};
export const Select = async (request) => {
    return PostAPI(request, 'PaysprintDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'PaysprintDMT/AddBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'PaysprintDMT/ViewBeneficiary');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'PaysprintDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'PaysprintDMT/SelectBank');
};
export const SelectBankList = async (request) => {
    return PostAPI(request, 'PaySprintBank/Select');
};
