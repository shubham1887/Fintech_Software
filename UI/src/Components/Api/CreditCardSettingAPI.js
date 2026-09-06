import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'CreditCardSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'CreditCardSetting/Select');
};

export const IsOnOff = async (request) => {
    return PostAPI(request, 'CreditCardSetting/IsOnOff');
};

export const ActiveCreditCard = async (request) => {
    return PostAPI(request, 'CreditCardSetting/ActiveCreditCard');
};
