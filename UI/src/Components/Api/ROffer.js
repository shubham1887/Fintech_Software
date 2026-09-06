import { PostAPI } from 'Components/Helper/Helpers';

export const SelectPlan = async (request) => {
    return PostAPI(request, 'ROffer/SelectPlan');
};

export const SelectDTHPlan = async (request) => {
    return PostAPI(request, 'ROffer/SelectDTHPlan');
};
export const SelectDTHCustInfo = async (request) => {
    return PostAPI(request, 'ROffer/SelectDTHCustInfo');
};

export const SelectROffer = async (request) => {
    return PostAPI(request, 'ROffer/SelectOffer');
};

export const HeavyRefresh = async (request) => {
    return PostAPI(request, 'ROffer/HeavyRefresh');
};
