import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'EnquiryDetails/Update');
};
export const Select = async (request) => {
    return PostAPI(request, 'EnquiryDetails/Select');
};
export const Insert = async (request) => {
    return PostAPI(request, 'EnquiryDetails/Insert');
};
