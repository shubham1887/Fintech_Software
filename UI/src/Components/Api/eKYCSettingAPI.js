import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'eKYCSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'eKYCSetting/Select');
};

export const IsOnOff = async (request) => {
    return PostAPI(request, 'eKYCSetting/IsOnOff');
};

export const SelectActiveeKYC = async (request) => {
    return PostAPI(request, 'eKYCSetting/SelectActiveeKYC');
};
