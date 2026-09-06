import { PostAPI } from 'Components/Helper/Helpers';

export const BulkUpdate = async (request) => {
    return PostAPI(request, 'CircleWiseOperatorLimit/BulkUpdate');
};
export const Select = async (request) => {
    return PostAPI(request, 'CircleWiseOperatorLimit/Select');
};
export const DeleteByUserID = async (request) => {
    return PostAPI(request, 'CircleWiseOperatorLimit/DeleteByUserID');
};
export const DeleteAll = async (request) => {
    return PostAPI(request, 'CircleWiseOperatorLimit/DeleteAll');
};
