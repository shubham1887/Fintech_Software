import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'UISetting/Select');
};
export const SelectyID = async (request) => {
    return PostAPI(request, 'UISetting/SelectyID');
};

export const Update = async (request) => {
    return PostAPI(request, 'UISetting/Update');
};

export const UpdateAll = async (request) => {
    return PostAPI(request, 'UISetting/UpdateAll');
};
