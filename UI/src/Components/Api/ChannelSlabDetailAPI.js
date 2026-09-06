import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'ChannelSlabDetail/Insert');
};

export const SelectBySlabMasterID = async (request) => {
    return PostAPI(request, 'ChannelSlabDetail/SelectBySlabMasterID');
};
export const Update = async (request) => {
    return PostAPI(request, 'ChannelSlabDetail/Update');
};
export const Delete = async (request) => {
    return PostAPI(request, 'ChannelSlabDetail/Delete');
};

export const BulkUpdate = async (request) => {
    return PostAPI(request, 'ChannelSlabDetail/BulkUpdate');
};
export const BulkAdd = async (request) => {
    return PostAPI(request, 'ChannelSlabDetail/BulkAdd');
};

export const SelectByServiceID = async (request) => {
    return PostAPI(request, 'ChannelSlabDetail/SelectByServiceID');
};
export const ServicewiseBulkUpdate = async (request) => {
    return PostAPI(request, 'ChannelSlabDetail/ServicewiseBulkUpdate');
};
