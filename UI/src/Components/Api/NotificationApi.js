import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'Notification/Insert');
};
export const Select = async (request) => {
    return PostAPI(request, 'Notification/Select');
};
