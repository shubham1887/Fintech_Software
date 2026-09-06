import { PostAPI } from 'Components/Helper/Helpers';

export const BulkUpdate = async (request) => {
    return PostAPI(request, 'CircleWiseUserWiseOperatorOff/BulkUpdate');
};
export const SelectAllUserwiseCircleOffList = async (request) => {
    return PostAPI(
        request,
        'CircleWiseUserWiseOperatorOff/SelectAllUserwiseCircleOffList'
    );
};
export const DeleteByUserID = async (request) => {
    return PostAPI(request, 'CircleWiseUserWiseOperatorOff/DeleteByUserID');
};
export const DeleteAll = async (request) => {
    return PostAPI(request, 'CircleWiseUserWiseOperatorOff/DeleteAll');
};
