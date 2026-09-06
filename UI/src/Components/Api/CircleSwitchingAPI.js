import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'CircleSwitching/Select');
};

export const Update = async (request) => {
    return PostAPI(request, 'CircleSwitching/Update');
};
export const SelectCircleSwitching = async (request) => {
    return PostAPI(request, 'CircleSwitching/SelectCircleSwitching');
};
export const Delete = async (request) => {
    return PostAPI(request, 'CircleSwitching/Delete');
};
