import { PostAPI } from 'Components/Helper/Helpers';

export const Initiate = async (request) => {
    return PostAPI(request, 'Demat/Initiate');
};
export const Select = async (request) => {
    return PostAPI(request, 'Demat/Select');
};
