import { PostAPI, CustomAPI } from 'Components/Helper/Helpers';

export const SelectHistory = async (request) => {
    return PostAPI(request, 'LoginHistory/SelectHistory');
};
