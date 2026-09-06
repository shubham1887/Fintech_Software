import { PostAPI } from 'Components/Helper/Helpers';

export const SelectByKey = async (request) => {
    return PostAPI(request, 'PublicTemplate/SelectByKey');
};

export const Select = async (request) => {
    return PostAPI(request, 'PublicTemplate/Select');
};

export const Update = async (request) => {
    return PostAPI(request, 'PublicTemplate/Update');
};
