import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'Beneficiary/Add');
};

export const CompositeBeneAdd = async (request) => {
    return PostAPI(request, 'Beneficiary/CompositeBeneAdd');
};

export const VerifyOTP = async (request) => {
    return PostAPI(request, 'Beneficiary/VerifyOTP');
};

export const ResendOTP = async (request) => {
    return PostAPI(request, 'Beneficiary/ResendOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'Beneficiary/Select');
};

export const SelectByGUID = async (request) => {
    return PostAPI(request, 'Beneficiary/SelectByGUID');
};

export const Delete = async (request) => {
    return PostAPI(request, 'Beneficiary/Delete');
};

export const CheckBranch = async (request) => {
    return PostAPI(request, 'Beneficiary/CheckBranch');
};
