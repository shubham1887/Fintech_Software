import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'OfflineProductMaster/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'OfflineProductMaster/Select');
};

export const Update = async (request) => {
    return PostAPI(request, 'OfflineProductMaster/Update');
};
export const Activate = async (request) => {
    return PostAPI(request, 'OfflineProductMaster/ActiveDeactive');
};
export const Delete = async (request) => {
    return PostAPI(request, 'OfflineProductMaster/Delete');
};
export const SelectPackageName = async (request) => {
    return PostAPI(request, 'OfflineProductMaster/SelectPackageName');
};
export const SelectByOfflineProductMasterID = async (request) => {
    return PostAPI(
        request,
        'OfflineProductMaster/SelectByOfflineProductMasterID'
    );
};
