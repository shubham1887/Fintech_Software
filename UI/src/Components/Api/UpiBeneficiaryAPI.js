import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'UpiBeneficiary/Add');
};

export const VerifyOTP = async (request) => {
    return PostAPI(request, 'UpiBeneficiary/VerifyOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'UpiBeneficiary/Select');
};

export const SelectByGUID = async (request) => {
    return PostAPI(request, 'UpiBeneficiary/SelectByGUID');
};

export const Delete = async (request) => {
    return PostAPI(request, 'UpiBeneficiary/Delete');
};
