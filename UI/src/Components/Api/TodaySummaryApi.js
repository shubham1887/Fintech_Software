import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'TodaySummary/Select');
};

export const SelectForRetailer = async (request) => {
    return PostAPI(request, 'TodaySummary/SelectForRetailer');
};
export const SelectDistributorsummaray = async (request) => {
    return PostAPI(request, 'TodaySummary/SelectForDistributor');
};
