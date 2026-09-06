import { PostAPI } from 'Components/Helper/Helpers';

export const SelectRequestByFilter = async (request) => {
    return PostAPI(request, 'Fund/SelectRequestByFilter');
};

export const RequestInsert = async (request) => {
    return PostAPI(request, 'Fund/RequestInsert');
};

export const SelectRequestByID = async (request) => {
    return PostAPI(request, 'Fund/SelectRequestByID');
};

export const FundAction = async (request) => {
    return PostAPI(request, 'Fund/AcceptReject');
};
export const SelectWalletTransferReport = async (request) => {
    return PostAPI(request, 'Fund/SelectWalletTransferReport');
};
export const Transfer = async (request) => {
    return PostAPI(request, 'Fund/Transfer');
};
export const Reverse = async (request) => {
    return PostAPI(request, 'Fund/Reverse');
};
export const GenerateFundReverseOTP = async (request) => {
    return PostAPI(request, 'Fund/GenerateFundReverseOTP');
};
export const ReverseWithOTP = async (request) => {
    return PostAPI(request, 'Fund/ReverseWithOTP');
};
export const TransferReport = async (request) => {
    return PostAPI(request, 'Fund/TransferReport');
};

export const SelectLast10ByReceiverID = async (request) => {
    return PostAPI(request, 'Fund/SelectLast10ByReceiverID');
};

export const SelectLast10ForReverseTransaction = async (request) => {
    return PostAPI(request, 'Fund/SelectLast10ForReverseTransaction');
};

export const ReverseRequestInsert = async (request) => {
    return PostAPI(request, 'Fund/ReverseRequest');
};

export const ReverseReport = async (request) => {
    return PostAPI(request, 'Fund/ReverseReport');
};
// export const selectDashboardfund = async (request) => {
//     return PostAPI(request, 'Fund/SelectTrnasferForDashboard');
// };
export const SelectEcollection = async (request) => {
    return PostAPI(request, 'Fund/SelectEcollection');
};

export const SelectLast10Request = async (request) => {
    return PostAPI(request, 'Fund/SelectLast10Request');
};
export const SelectHeaderWithRequestByFilter = async (request) => {
    return PostAPI(request, 'Fund/SelectHeaderWithRequestByFilter');
};
export const Delete = async (request) => {
    return PostAPI(request, 'Fund/Delete');
};
