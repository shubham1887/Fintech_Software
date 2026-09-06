import { PostAPI } from 'Components/Helper/Helpers';
import { OutsidePostAPI } from 'Components/Helper/OutsideHelper';

export const Initiate = async (request) => {
    return PostAPI(request, 'SDKTransaction/Initiate');
};
export const SelectBBPSServices = async (request) => {
    return OutsidePostAPI(request, 'SDK/v1/SelectBBPSServices');
};
export const SelectOperatorByServiceID = async (request) => {
    return OutsidePostAPI(request, 'SDK/v1/SelectOperatorByServiceID');
};
export const BillFetch = async (request) => {
    return OutsidePostAPI(request, 'SDK/v1/BillFetch');
};
export const Recharge = async (request) => {
    return OutsidePostAPI(request, 'SDK/v1/Recharge');
};
export const SelectByOperatorID = async (request) => {
    return OutsidePostAPI(request, 'SDK/v1/SelectByOperatorID');
};
export const SelectForLast10ByAgentCode = async (request) => {
    return OutsidePostAPI(request, 'SDK/v1/SelectForLast10ByAgentCode');
};
