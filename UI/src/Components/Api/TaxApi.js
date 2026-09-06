import { PostAPI } from 'Components/Helper/Helpers';

export const SelectTDS = async (request) => {
    return PostAPI(request, 'Tax/SelectTDS');
};

export const SelectGSTDetailReportForAdmin = async (request) => {
    return PostAPI(request, 'Tax/SelectGSTDetailReportForAdmin');
};

export const SelectGSTDetail = async (request) => {
    return PostAPI(request, 'Tax/SelectGSTDetail');
};

export const SelectGSTCategory = async (request) => {
    return PostAPI(request, 'Tax/SelectGSTCategoryWise');
};
export const SelectGSTDetailByTransaction = async (request) => {
    return PostAPI(request, 'Tax/SelectGSTDetailByTransaction');
};
export const SelectGSTCategoryV2 = async (request) => {
    return PostAPI(request, 'Tax/SelectGSTCategoryWiseV2');
};
