import { PostAPI } from 'Components/Helper/Helpers';
export const SlabUserAdd = async (request) => {
    return PostAPI(request, 'UserSlabMapping/Insert');
};
export const Select = async (request) => {
    return PostAPI(request, 'UserSlabMapping/Select');
};
export const Delete = async (request) => {
    return PostAPI(request, 'UserSlabMapping/Delete');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'UserSlabMapping/SelectByUserSlabMappingID');
};

export const Update = async (request) => {
    return PostAPI(request, 'UserSlabMapping/Update');
};

export const SelectByUserID = async (request) => {
    return PostAPI(request, 'UserSlabMapping/SelectByUserID');
};
