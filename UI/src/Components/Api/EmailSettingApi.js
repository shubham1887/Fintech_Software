import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'EmailSetting/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'EmailSetting/Select');
};

export const ActivateEmail = async (request) => {
    return PostAPI(request, 'EmailSetting/ActiveDeactive');
};

export const ActivateSSL = async (request) => {
    return PostAPI(request, 'EmailSetting/ActiveDeactiveSSL');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'EmailSetting/SelectByID');
};

export const Update = async (request) => {
    return PostAPI(request, 'EmailSetting/Update');
};
