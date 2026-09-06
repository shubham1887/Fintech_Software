import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'ReconciliationSettings/Insert');
};

export const Update = async (request) => {
    return PostAPI(request, 'ReconciliationSettings/Update');
};
export const Select = async (request) => {
    return PostAPI(request, 'ReconciliationSettings/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'ReconciliationSettings/SelectByID');
};

export const Delete = async (request) => {
    return PostAPI(request, 'ReconciliationSettings/Delete');
};
