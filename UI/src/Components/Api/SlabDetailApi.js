import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'SlabDetail/Insert');
};
export const BulkAdd = async (request) => {
    return PostAPI(request, 'SlabDetail/BulkAdd');
};

export const Select = async (request) => {
    return PostAPI(request, 'SlabDetail/Select');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'SlabDetail/SelectBySlabDetailID');
};
export const Update = async (request) => {
    return PostAPI(request, 'SlabDetail/Update');
};
export const Delete = async (request) => {
    return PostAPI(request, 'SlabDetail/Delete');
};

export const SelectBySlabMasterID = async (request) => {
    return PostAPI(request, 'SlabDetail/SelectBySlabMasterID');
};
export const SelectSlabDetails = async (request) => {
    return PostAPI(request, 'SlabDetail/SelectByUserIDWithService');
};
// export const SelectServiceWithPagination = async (request) => {
//     return PostAPI(request, 'SlabDetail/SelectServiceWithPagination');
// };

export const BulkUpdate = async (request) => {
    return PostAPI(request, 'SlabDetail/BulkUpdate');
};
export const SelectByServiceID = async (request) => {
    return PostAPI(request, 'SlabDetail/SelectByServiceID');
};
export const Activate = async (request) => {
    return PostAPI(request, 'SlabDetail/Activate');
};
export const ServicewiseBulkUpdate = async (request) => {
    return PostAPI(request, 'SlabDetail/ServicewiseBulkUpdate');
};
export const SelectForAdjustCommission = async (request) => {
    return PostAPI(request, 'SlabDetail/SelectForAdjustCommission');
};
export const UpdateCommission = async (request) => {
    return PostAPI(request, 'SlabDetail/IncreaseDecreaseCommission');
};
export const UpdateAmountByID = async (request) => {
    return PostAPI(request, 'SlabDetail/UpdateAmountByID');
};
export const SelectSlabByUserIDWithService = async (request) => {
    return PostAPI(request, 'SlabDetail/SelectSlabByUserIDWithService');
};
export const SelectUPICommissionByUserID = async (request) => {
    return PostAPI(request, 'SlabDetail/SelectUPICommissionByUserID');
};
export const SelectDMTChargeAndCommission = async (request) => {
    return PostAPI(request, 'SlabDetail/SelectDMTChargeAndCommission');
};
export const SelectSlabCommissionByOperatorID = async (request) => {
    return PostAPI(request, 'SlabDetail/SelectSlabCommissionByOperatorID');
};
