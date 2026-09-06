import { PostAPI, useCustomAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'State/Select');
};

export const ZoneMasterSelect = async (request) => {
    return PostAPI(request, 'ZoneMaster/Select');
};
