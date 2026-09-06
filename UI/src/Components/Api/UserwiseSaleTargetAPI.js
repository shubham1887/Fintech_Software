import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'UserwiseSaleTarget/Select');
};
export const Insert = async (request) => {
    return PostAPI(request, 'UserwiseSaleTarget/Insert');
};
export const Delete = async (request) => {
    return PostAPI(request, 'UserwiseSaleTarget/Delete');
};
export const SaleTargetSelect = async (request) => {
    return PostAPI(request, 'UserwiseSaleTarget/SaleTargetSelect');
};

export const TransferCommission = async (request) => {
    return PostAPI(request, 'UserwiseSaleTarget/TransferCommission');
};
export const SaleTargetSelectUserwise = async (request) => {
    return PostAPI(request, 'UserwiseSaleTarget/SaleTargetSelectUserwise');
};
