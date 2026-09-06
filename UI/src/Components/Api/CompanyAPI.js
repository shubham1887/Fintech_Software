import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'Company/Select');
};

export const Update = async (request) => {
    return PostAPI(request, 'Company/Update');
};
