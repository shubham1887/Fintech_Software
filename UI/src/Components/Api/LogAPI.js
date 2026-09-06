import { PostAPI } from 'Components/Helper/Helpers';

export const SelectInitialLog = async (request) => {
    return PostAPI(
        request,
        'APIUserInitialRequestResponseLog/SelectByFromDateToDate'
    );
};
export const SelectCallbackLog = async (request) => {
    return PostAPI(
        request,
        'APIUserCallbackRequestResponseLog/SelectByFromDateToDate'
    );
};
