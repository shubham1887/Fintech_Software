import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'OperatorUserwise/Select');
};
export const Insert = async (request) => {
    return PostAPI(request, 'OperatorUserwise/Insert');
};

export const DeleteByUserID = async (request) => {
    return PostAPI(request, 'OperatorUserwise/DeleteByUserID');
};
export const DeleteAll = async (request) => {
    return PostAPI(request, 'OperatorUserwise/DeleteAll');
};
export const Activate = async (request) => {
    return PostAPI(request, 'OperatorUserwise/Activate');
};
