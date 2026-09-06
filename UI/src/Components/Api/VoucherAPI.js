import { PostAPI } from 'Components/Helper/Helpers';
export const SelectForDropdown = async (request) => {
    return PostAPI(request, 'VoucherType/SelectForDropdown');
};
export const Insert = async (request) => {
    return PostAPI(request, 'VoucherMaster/Insert');
};
export const Update = async (request) => {
    return PostAPI(request, 'VoucherMaster/Update');
};
export const Reverse = async (request) => {
    return PostAPI(request, 'VoucherMaster/Reverse');
};
export const SelectReport = async (request) => {
    return PostAPI(request, 'VoucherMaster/SelectReport');
};
export const SelectByIDForVoucherApprove = async (request) => {
    return PostAPI(request, 'VoucherMaster/SelectByIDForVoucherApprove');
};
export const VoucherMasterApproveReport = async (request) => {
    return PostAPI(request, 'VoucherMaster/VoucherMasterApproveReport');
};
export const VoucherDetailsReport = async (request) => {
    return PostAPI(request, 'VoucherDetails/SelectReport');
};
