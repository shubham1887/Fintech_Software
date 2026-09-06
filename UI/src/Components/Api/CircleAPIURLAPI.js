import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'CircleAPI/Select');
};
export const Add = async (request) => {
    return PostAPI(request, 'CircleAPI/Insert');
};
export const Activate = async (request) => {
    return PostAPI(request, 'CircleAPI/ActiveDeactive');
};
