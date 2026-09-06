import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'CategoryChannelMapping/Insert');
};

export const Update = async (request) => {
    return PostAPI(request, 'CategoryChannelMapping/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'CategoryChannelMapping/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'CategoryChannelMapping/SelectByID');
};

export const Delete = async (request) => {
    return PostAPI(request, 'CategoryChannelMapping/Delete');
};
export const selectCategory = async (request) => {
    return PostAPI(request, 'ChannelCategory/Select');
};
export const SelectByChannelCategoryID = async (request) => {
    return PostAPI(request, 'CategoryChannelMapping/SelectByChannelCategoryID');
};

export const InsertPackageChannel = async (request) => {
    return PostAPI(request, 'DthPackageChannelMapping/Insert');
};
export const SelectByOfflineProductMasterID = async (request) => {
    return PostAPI(
        request,
        'DthPackageChannelMapping/SelectByOfflineProductMasterID'
    );
};
