import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'Operator/SelectByServiceID');
};
export const SelectAEPSOperator = async (request) => {
    return PostAPI(request, 'Operator/SelectAEPSOperator');
};
export const Add = async (request) => {
    return PostAPI(request, 'Operator/Insert');
};

export const SelectAll = async (request) => {
    return PostAPI(request, 'Operator/SelectOperator');
};

export const SelectIndex = async (request) => {
    return PostAPI(request, 'Operator/SelectOperatorForExcel');
};

export const Activate = async (request) => {
    return PostAPI(request, 'Operator/ActiveDeactive');
};

export const SelectOperator = async (request) => {
    return PostAPI(request, 'Operator/SelectByOperatorID');
};

export const OperatorUpdate = async (request) => {
    return PostAPI(request, 'Operator/Update');
};
export const SelectMobile = async (request) => {
    return PostAPI(request, 'Operator/SelectByServiceID');
};

export const SelectAllOperator = async (request) => {
    return PostAPI(request, 'Operator/SelectOperator');
};

export const SelectBBPSOperator = async (request) => {
    return PostAPI(request, 'Operator/SelectBBPSOperator');
};

export const SelectOperatorForDropDown = async (request) => {
    return PostAPI(request, 'Operator/SelectOperatorForDropDown');
};

export const SelectOperatorPrepaidAndDTH = async (request) => {
    return PostAPI(request, 'Operator/SelectOperatorPrepaidAndDTH');
};
