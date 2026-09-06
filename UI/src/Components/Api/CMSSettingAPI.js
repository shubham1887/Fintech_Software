import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'CMSSetting/Update');
};
export const Select = async (request) => {
    return PostAPI(request, 'CMSSetting/Select');
};
export const IsOnOff = async (request) => {
    return PostAPI(request, 'CMSSetting/IsOnOff');
};
