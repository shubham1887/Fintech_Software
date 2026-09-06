import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'CallMeRequest/Select');
};

export const Insert = async (request) => {
    return PostAPI(request, 'CallMeRequest/Insert');
};
export const selectCategory = async (request) => {
    return PostAPI(request, 'CallMeRequest/SelectCategory');
};
export const SelectByUplineUserID = async (request) => {
    return PostAPI(request, 'CallMeRequest/SelectByUplineUserID');
};
export const Update = async (request) => {
    return PostAPI(request, 'CallMeRequest/Update');
};
