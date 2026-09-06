import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'Wallet/Select');
};
export const Insert = async (request) => {
    return PostAPI(
        request,
        'UserBalanceCapping/InsertDefaultForAllUserWalletWise'
    );
};
export const SelectBalanceCapping = async (request) => {
    return PostAPI(request, 'UserBalanceCapping/Select');
};
export const Update = async (request) => {
    return PostAPI(request, 'UserBalanceCapping/Update');
};
export const Delete = async (request) => {
    return PostAPI(request, 'UserBalanceCapping/Delete');
};
