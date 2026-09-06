import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'ChannelSlabMaster/Insert');
};
export const Copy = async (request) => {
    return PostAPI(request, 'ChannelSlabMaster/Copy');
};

export const Select = async (request) => {
    return PostAPI(request, 'ChannelSlabMaster/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'ChannelSlabMaster/SelectByID');
};

export const Delete = async (request) => {
    return PostAPI(request, 'ChannelSlabMaster/Delete');
};
export const SelectByUserID = async (request) => {
    return PostAPI(request, 'ChannelSlabMaster/SelectByUserID');
};
