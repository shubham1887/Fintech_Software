import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'UPITransferSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'UPITransferSetting/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'UPITransferSetting/SelectByID');
};

export const SelectActiveUPITransfer = async (request) => {
    return PostAPI(request, 'UPITransferSetting/SelectActiveUPITransfer');
};
