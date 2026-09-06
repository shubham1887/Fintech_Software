import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'UserwiseServiceChargeMaster/Select');
};

export const Insert = async (request) => {
    return PostAPI(request, 'UserwiseServiceChargeMaster/Insert');
};
export const Delete = async (request) => {
    return PostAPI(request, 'UserwiseServiceChargeMaster/Delete');
};
