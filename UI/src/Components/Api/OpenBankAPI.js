import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'VendorBeneficiary/Select');
};

export const Add = async (request) => {
    return PostAPI(request, 'VendorBeneficiary/Add');
};
