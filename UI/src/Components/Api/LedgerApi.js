import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'Ledger/Select');
};

export const SelectAdmin = async (request) => {
    return PostAPI(request, 'Ledger/AdminLedger');
};
