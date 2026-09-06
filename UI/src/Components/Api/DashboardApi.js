import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'Dashboard/Select');
};
export const SelectForRetailer = async (request) => {
    return PostAPI(request, 'Dashboard/SelectForRetailer');
};
export const SelectForDistributor = async (request) => {
    return PostAPI(request, 'Dashboard/SelectForDistributor');
};
