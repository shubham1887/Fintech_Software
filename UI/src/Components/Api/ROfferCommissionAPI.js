import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'ROfferCommission/Insert');
};

export const Copy = async (request) => {
    return PostAPI(request, 'ROfferCommission/Copy');
};
export const Select = async (request) => {
    return PostAPI(request, 'ROfferCommission/Select');
};

export const Activate = async (request) => {
    return PostAPI(request, 'ROfferCommission/Activate');
};
export const SelectByROfferCommissionID = async (request) => {
    return PostAPI(request, 'ROfferCommission/SelectByID');
};
export const Update = async (request) => {
    return PostAPI(request, 'ROfferCommission/Update');
};
export const Delete = async (request) => {
    return PostAPI(request, 'ROfferCommission/Delete');
};

export const SelectFRCAmount = async (request) => {
    return PostAPI(request, 'ROfferCommission/SelectFRCAmount');
};
