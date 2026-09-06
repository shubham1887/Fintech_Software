import { PostAPI } from '../Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'PaymentModes/Select');
};

export const SelectByReceiverID = async (request) => {
    return PostAPI(request, 'PaymentModes/SelectByReceiverID');
};
