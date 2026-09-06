import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'News/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'News/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'News/SelectByID');
};

export const Update = async (request) => {
    return PostAPI(request, 'News/Update');
};

export const SelectByUserID = async (request) => {
    return PostAPI(request, 'News/SelectByUserID');
};
export const UpdateIsView = async (request) => {
    return PostAPI(request, 'News/UpdateIsView');
};
export const Delete = async (request) => {
    return PostAPI(request, 'News/Delete');
};
export const DeleteAll = async (request) => {
    return PostAPI(request, 'News/DeleteAll');
};
