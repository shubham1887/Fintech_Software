import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'PanCardSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'PanCardSetting/Select');
};

export const IsOnOff = async (request) => {
    return PostAPI(request, 'PanCardSetting/IsOnOff');
};

export const ActivePanCard = async (request) => {
    return PostAPI(request, 'PanCardSetting/SelectActivePanCard');
};
