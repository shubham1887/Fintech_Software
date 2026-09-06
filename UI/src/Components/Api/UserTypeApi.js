import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'UserType/Select');
};
export const SelectWithoutAPIUserType = async (request) => {
    return PostAPI(request, 'UserType/SelectWithoutAPIUserType');
};
export const IsOutsideActivate = async (request) => {
    return PostAPI(request, 'UserType/IsOutsideActivate');
};
export const SelectOusideRoleSetting = async (request) => {
    return PostAPI(request, 'UserType/SelectOusideRoleSetting');
};
export const SelectUserByUserTypeID = async (request) => {
    return PostAPI(request, 'UserType/SelectUserByUserTypeID');
};
export const UpdateAliasName = async (request) => {
    return PostAPI(request, 'UserType/UpdateAliasName');
};
export const SelectAliasName = async (request) => {
    return PostAPI(request, 'UserType/SelectAliasName');
};
export const SelectRoleWithoutSupport = async (request) => {
    return PostAPI(request, 'UserType/SelectRoleWithoutSupport');
};
export const SelectOnlyDistAndSuperDist = async (request) => {
    return PostAPI(request, 'UserType/SelectOnlyDistAndSuperDist');
};

export const SelectForSaleTarget = async (request) => {
    return PostAPI(request, 'UserType/SelectForSaleTarget');
};
export const SelectOnlyRetailerAndAPI = async (request) => {
    return PostAPI(request, 'UserType/SelectOnlyRetailerAndAPI');
};

export const SelectOnlyAdminAndDistAndSuperDist = async (request) => {
    return PostAPI(request, 'UserType/SelectOnlyAdminDistAndSuperDist');
};
