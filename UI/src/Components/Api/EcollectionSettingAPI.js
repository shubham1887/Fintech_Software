import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'EcollectionSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'EcollectionSetting/Select');
};
export const IsOnOff = async (request) => {
    return PostAPI(request, 'EcollectionSetting/IsOnOff');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'EcollectionSetting/SelectByID');
};
