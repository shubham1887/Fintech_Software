import { PostAPI } from 'Components/Helper/Helpers';

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'MyNetLinkDMT/SenderRegistration');
};

export const Select = async (request) => {
    return PostAPI(request, 'MyNetLinkDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'MyNetLinkDMT/AddBeneficiary');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'MyNetLinkDMT/DeleteBeneficiary');
};

export const ViewBeneficiary = async (request) => {
    return PostAPI(request, 'MyNetLinkDMT/ViewBeneficiary');
};

export const GetTransctionStatus = async (request) => {
    return PostAPI(request, 'MyNetLinkDMT/GetTransctionStatus');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'MyNetLinkDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'MyNetLinkDMT/SelectBank');
};
