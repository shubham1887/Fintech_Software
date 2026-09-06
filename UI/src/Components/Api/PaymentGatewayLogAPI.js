import { PostAPI } from 'Components/Helper/Helpers';

export const SelectByFromDateTodate = async (request) => {
    return PostAPI(request, 'PaymentGatewayLog/SelectByFromDateTodate');
};

export const SelectByOrderID = async (request) => {
    return PostAPI(request, 'PaymentGatewayLog/SelectByOrderID');
};
