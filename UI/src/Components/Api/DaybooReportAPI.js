import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'DayBookReport/Select');
};

export const SelectProviderDaybook = async (request) => {
    return PostAPI(request, 'DayBookReport/ProviderDaybook');
};
export const SelectByProviderID = async (request) => {
    return PostAPI(request, 'DayBookReport/SelectByProviderID');
};
