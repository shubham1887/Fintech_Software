import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'AEPSSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'AEPSSetting/Select');
};

export const IsOnOff = async (request) => {
    return PostAPI(request, 'AEPSSetting/IsOnOff');
};
