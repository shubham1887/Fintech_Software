import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'UserCreationSetting/Insert');
};
export const Update = async (request) => {
    return PostAPI(request, 'UserCreationSetting/Update');
};
export const Delete = async (request) => {
    return PostAPI(request, 'UserCreationSetting/Delete');
};
export const Select = async (request) => {
    return PostAPI(request, 'UserCreationSetting/Select');
};
export const SelectByUserID = async (request) => {
    return PostAPI(request, 'UserCreationSetting/SelectByUserID');
};
