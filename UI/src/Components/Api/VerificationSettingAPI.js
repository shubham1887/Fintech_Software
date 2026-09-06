import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'VerificationSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'VerificationSetting/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'VerificationSetting/SelectByID');
};

export const MultiActive = async (request) => {
    return PostAPI(request, 'VerificationSetting/SelectMultiActive');
};
