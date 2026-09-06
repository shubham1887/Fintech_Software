import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'OfflineSlabDetail/Insert');
};
export const Update = async (request) => {
    return PostAPI(request, 'OfflineSlabDetail/Update');
};
export const Delete = async (request) => {
    return PostAPI(request, 'OfflineSlabDetail/Delete');
};
export const Activate = async (request) => {
    return PostAPI(request, 'OfflineSlabDetail/Activate');
};
export const SelectByServiceID = async (request) => {
    return PostAPI(request, 'OfflineSlabDetail/SelectByServiceID');
};
