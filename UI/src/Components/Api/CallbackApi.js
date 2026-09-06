import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'CallbackMaster/Insert');
};

export const Delete = async (request) => {
    return PostAPI(request, 'CallbackMaster/Delete');
};

export const Select = async (request) => {
    return PostAPI(request, 'CallbackMaster/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'CallbackMaster/SelectByID');
};
