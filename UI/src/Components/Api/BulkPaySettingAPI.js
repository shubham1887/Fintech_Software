import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'BulkPaySetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'BulkPaySetting/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'BulkPaySetting/SelectByID');
};
