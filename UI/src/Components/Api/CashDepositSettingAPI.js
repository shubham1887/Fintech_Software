import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'CashDepositSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'CashDepositSetting/Select');
};

export const IsOnOff = async (request) => {
    return PostAPI(request, 'CashDepositSetting/IsOnOff');
};

export const ActiveCashDeposit = async (request) => {
    return PostAPI(request, 'CashDepositSetting/ActiveCashDeposit');
};
