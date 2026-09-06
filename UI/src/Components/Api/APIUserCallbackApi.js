import { PostAPI } from 'Components/Helper/Helpers';

export const Search = async (request) => {
    return PostAPI(
        request,
        'APIUserCallbackRequestResponseLog/SelectByConsumerNumber'
    );
};
export const ResendCallback = async (request) => {
    return PostAPI(request, 'APIUserCallbackRequestResponseLog/ResendCallback');
};
export const SelectInitialLogByUserID = async (request) => {
    return PostAPI(
        request,
        'APIUserInitialRequestResponseLog/SelectInitialLogByUserIDAndConsumerNumber'
    );
};
export const SelectCallBackByUserID = async (request) => {
    return PostAPI(
        request,
        'APIUserCallbackRequestResponseLog/SelectCallBackByUserIDAndConsumerNumber'
    );
};
