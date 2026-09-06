import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'InvoiceSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'InvoiceSetting/Select');
};
