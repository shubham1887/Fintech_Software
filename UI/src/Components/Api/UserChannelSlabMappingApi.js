import { PostAPI } from 'Components/Helper/Helpers';
export const SlabUserAdd = async (request) => {
    return PostAPI(request, 'UserChannelSlabMapping/Insert');
};
export const Select = async (request) => {
    return PostAPI(request, 'UserChannelSlabMapping/Select');
};
export const Delete = async (request) => {
    return PostAPI(request, 'UserChannelSlabMapping/Delete');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'UserChannelSlabMapping/SelectByUserSlabMappingID');
};

export const Update = async (request) => {
    return PostAPI(request, 'UserChannelSlabMapping/Update');
};
