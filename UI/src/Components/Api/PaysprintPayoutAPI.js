import { PostAPI } from 'Components/Helper/Helpers';

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'PaysprintPayout/AddBeneficiary');
};

export const Select = async (request) => {
    return PostAPI(request, 'PaysprintPayout/Select');
};

export const UploadDocument = async (request) => {
    return PostAPI(request, 'PaysprintPayout/UploadDocument');
};
