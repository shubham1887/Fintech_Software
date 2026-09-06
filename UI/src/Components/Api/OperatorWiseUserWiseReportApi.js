import { PostAPI } from '../Helper/Helpers';

export const SelectOperatorWisesUserWiseReport = async (request) => {
    return PostAPI(
        request,
        'TransactionWiseReport/SelectOperatorWisesUserWiseReport'
    );
};
export const SelectUserWiseOperatorWiseSellReport = async (request) => {
    return PostAPI(
        request,
        'TransactionWiseReport/SelectUserWiseOperatorWiseSellReport'
    );
};



