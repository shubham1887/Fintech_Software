import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'PaymentGatewaySetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'PaymentGatewaySetting/Select');
};

export const IsOnOff = async (request) => {
    return PostAPI(request, 'PaymentGatewaySetting/IsOnOff');
};
