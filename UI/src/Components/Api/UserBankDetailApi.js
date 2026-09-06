import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'UserBankDetails/Select');
};

export const Insert = async (request) => {
    return PostAPI(request, 'UserBankDetails/Insert');
};

export const Update = async (request) => {
    return PostAPI(request, 'UserBankDetails/Update');
};

export const Delete = async (request) => {
    return PostAPI(request, 'UserBankDetails/Delete');
};
export const SelectByParentID = async (request) => {
    return PostAPI(request, 'UserBankDetails/SelectByParentID');
};
export const SelectParentBanks = async (request) => {
    return PostAPI(request, 'UserBankDetails/SelectParentBanks');
};
export const SelectCompanyBanks = async (request) => {
    return PostAPI(request, 'UserBankDetails/SelectCompanyBanks');
};
export const SelectBankForDropDown = async (request) => {
    return PostAPI(request, 'UserBankDetails/SelectBankForDropDown');
};
