import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'DMTSettings/Insert');
};

export const Update = async (request) => {
    return PostAPI(request, 'DMTSettings/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'DMTSettings/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'DMTSettings/SelectByID');
};

export const SelectTransferMode = async (request) => {
    return PostAPI(request, 'DMTSettings/SelectTransferMode');
};

export const SelectAllBank = async (request) => {
    return PostAPI(request, 'DMTBank/SelectAll');
};

export const ActiveDeactive = async (request) => {
    return PostAPI(request, 'DMTBank/ActiveDeactive');
};
export const IMPSActive = async (request) => {
    return PostAPI(request, 'DMTBank/IMPSActiveDeactive');
};
export const UpdateBank = async (request) => {
    return PostAPI(request, 'DMTBank/UpdateBank');
};

export const UpdateAll = async (request) => {
    return PostAPI(request, 'DMTBank/UpdateAll');
};

export const MultiActiveDMT = async (request) => {
    return PostAPI(request, 'DMTSettings/SelectMultiActiveDMT');
};
