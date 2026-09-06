import { PostAPI } from 'Components/Helper/Helpers';

export const SelectState = async (request) => {
    return PostAPI(request, 'MahagramAEPS/SelectState');
};

export const SelectDistrictByState = async (request) => {
    return PostAPI(request, 'MahagramAEPS/SelectDistrictByState');
};

export const AEPSRegistration = async (request) => {
    return PostAPI(request, 'MahagramAEPS/AEPSRegistration');
};

export const SelectURL = async (request) => {
    return PostAPI(request, 'MahagramAEPS/SelectURL');
};
