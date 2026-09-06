import { PostAPI } from 'Components/Helper/Helpers';

export const AddContact = async (request) => {
    return PostAPI(request, 'EzyPayout/AddContact');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'EzyPayout/AddBeneficiary');
};

export const GetContact = async (request) => {
    return PostAPI(request, 'EzyPayout/GetContact');
};

export const GetBene = async (request) => {
    return PostAPI(request, 'EzyPayout/GetBeneficiary');
};
