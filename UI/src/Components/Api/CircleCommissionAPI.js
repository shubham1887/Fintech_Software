import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'CircleWiseCommission/Insert');
};
export const SelectByStateIDAndSlabID = async (request) => {
    return PostAPI(request, 'CircleWiseCommission/SelectByStateIDAndSlabID');
};
export const Update = async (request) => {
    return PostAPI(request, 'CircleWiseCommission/Update');
};
export const Delete = async (request) => {
    return PostAPI(request, 'CircleWiseCommission/Delete');
};
export const BulkUpdate = async (request) => {
    return PostAPI(request, 'CircleWiseCommission/BulkUpdate');
};
