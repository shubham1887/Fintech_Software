import { PostAPI, useCustomAPI } from '../Helper/Helpers';

export const ForgotPasswordAPI = async (request) => {
    return PostAPI(request, 'Credential/ForgotPassword');
};

export const LoginAPI = async (request) => {
    return PostAPI(request, 'Credential/Login');
};


export const NewDeviceLoginAPI = async (request) => {
    return PostAPI(request, 'Credential/NewDeviceLogin');
};

export const Add = async (request) => {
    return PostAPI(request, 'Users/Insert');
};

export const Update = async (request) => {
    return PostAPI(request, 'Users/Update');
};

export const Delete = async (request) => {
    return PostAPI(request, 'Users/Delete');
};

export const CommissionHoldActivate = async (request) => {
    return PostAPI(request, 'Users/CommissionHoldActivate');
};

export const Activate = async (request) => {
    return PostAPI(request, 'Users/Activate');
};

export const Deactivate = async (request) => {
    return PostAPI(request, 'Users/Deactivate');
};

export const LoginOTPActivate = async (request) => {
    return PostAPI(request, 'Users/LoginOTPActivate');
};

export const SelectUpline = async (request) => {
    return PostAPI(request, 'Users/SelectUpline');
};

export const SelectUserList = async (request) => {
    return PostAPI(request, 'Users/SelectAll');
};

export const SelectAgentList = async (request) => {
    return PostAPI(request, 'Users/SelectAgent');
};

export const SelectUser = async (request) => {
    return PostAPI(request, 'Users/SelectByID');
};

export const SelectParent = async (request) => {
    return PostAPI(request, 'Users/SelectParent');
};

export const SelectDownlinForDropDown = async (request) => {
    return PostAPI(request, 'Users/SelectDownlinForDropDown');
};

export const SelectUserForDropDown = async (request) => {
    return PostAPI(request, 'Users/SelectUserForDropDown');
};
export const SelectUserForDropDownForCashCollection = async (request) => {
    return PostAPI(request, 'Users/SelectUserForDropDownForCashCollection');
};
export const SelectChannelUserForDropDown = async (request) => {
    return PostAPI(request, 'Users/SelectChannelUserForDropDown');
};

export const SelectSupportUserForDropDown = async (request) => {
    return PostAPI(request, 'Users/SelectSupportUserForDropDown');
};
export const SelectDownlinForFundTransfer = async (request) => {
    return PostAPI(request, 'Users/SelectDownlinForFundTransfer');
};

export const SelectProfile = async (request) => {
    return PostAPI(request, 'Users/SelectProfile');
};

export const UpdatePassword = async (request) => {
    return PostAPI(request, 'Users/UpdatePassword');
};
export const SendPassword = async (request) => {
    return PostAPI(request, 'Users/SendPassword');
};

export const SelectParentWithAdmin = async (request) => {
    return PostAPI(request, 'Users/SelectParentWithAdmin');
};
export const SelectParentWithAdminForCallMeRequest = async (request) => {
    return PostAPI(request, 'Users/SelectParentWithAdminForCallMeRequest');
};

export const RefreshJWTToken = async (request) => {
    return PostAPI(request, 'Credential/RefreshJWTToken');
};

export const UpdateFCMToken = async (request) => {
    return PostAPI(request, 'Users/UpdateFCMToken');
};
export const IsWebActivate = async (request) => {
    return PostAPI(request, 'Users/IsWebActivate');
};
export const IsAppActivate = async (request) => {
    return PostAPI(request, 'Users/IsAppActivate');
};
export const selectUserSummary = async (request) => {
    return PostAPI(request, 'Users/SelectUserForAdminDashborad');
};

export const selectTarget = async (request) => {
    return PostAPI(request, 'Users/SelectUserTarget');
};
export const InsertToken = async (request) => {
    return PostAPI(request, 'Users/TokenGeneration');
};
export const SelectToken = async (request) => {
    return PostAPI(request, 'Users/SelectAPIToken');
};

export const ProfileUpload = async (request) => {
    return PostAPI(request, 'Users/ProfileUpload');
};

export const InitialChangePassword = async (request) => {
    return PostAPI(request, 'Credential/InitialChangePassword');
};

export const UpdateIsSecureIP = async (request) => {
    return PostAPI(request, 'Users/UpdateIsSecureIP');
};
export const SelectIsSecureIP = async (request) => {
    return PostAPI(request, 'Users/SelectIsSecureIP');
};

export const SelectInActiveUsers = async (request) => {
    return PostAPI(request, 'Users/SelectInActiveUsers');
};
export const SelectUserSummaryForDistributor = async (request) => {
    return PostAPI(
        request,
        'Users/SelectUserBalanceSummaryForDistributorDashborad'
    );
};
export const UpdatePin = async (request) => {
    return PostAPI(request, 'Users/UpdatePin');
};
export const IsInvoiceActivate = async (request) => {
    return PostAPI(request, 'Users/IsInvoiceActivate');
};
export const SelectDownlineReport = async (request) => {
    return PostAPI(request, 'Users/SelectDownlineReport');
};

export const SelectRetailerAndAPIUserForDropDown = async (request) => {
    return PostAPI(request, 'Users/SelectRetailerAndAPIUserForDropDown');
};
export const ProfileUpdate = async (request) => {
    return PostAPI(request, 'Users/UpdateProfile');
};
export const SelectDatewise = async (request) => {
    return PostAPI(request, 'Users/SelectDatewise');
};

export const logoutUser = async (request) => {
    return PostAPI(request, 'Credential/Logout');
};

export const SelectResponseMessage = async (request) => {
    return PostAPI(request, 'Users/SelectResponseMessage');
};

export const UpdateUserCommission = async (request) => {
    return PostAPI(request, 'Users/UpdateUserCommission');
};

export const SelectUserCommissionByUserID = async (request) => {
    return PostAPI(request, 'Users/SelectUserCommissionByUserID');
};

export const Select2FAQRImage = async (request) => {
    return PostAPI(request, 'Users/Select2FAQRImage');
};

export const TwoFAActivate = async (request) => {
    return PostAPI(request, 'Users/TwoFAActivate');
};

export const Send2FAOTP = async (request) => {
    return PostAPI(request, 'Users/Send2FAOTP');
};

export const DeactivateWithOTP = async (request) => {
    return PostAPI(request, 'Users/Deactivate2FAWithOTP');
};
export const TwoFALoginAPI = async (request) => {
    return PostAPI(request, 'Credential/VerifyTwoFAatLogin');
};

export const VerifyTwoFA = async (request) => {
    return PostAPI(request, 'Users/VerifyTwoFA');
};
export const ChangePasswordAfterExpiry = async (request) => {
    return PostAPI(request, 'Credential/ChangePasswordAfterExpiry');
};

export const SelectDeletedUsers = async (request) => {
    return PostAPI(request, 'Users/SelectDeletedUsers');
};

export const UserRevert = async (request) => {
    return PostAPI(request, 'Users/UserRevert');
};

export const SelectSDAndDistForDropDown = async (request) => {
    return PostAPI(request, 'Users/SelectSDAndDistForDropDown');
};
export const SelectSDAndDistForDropDownByOldUserID = async (request) => {
    return PostAPI(request, 'Users/SelectSDAndDistForDropDownByOldUserID');
};

export const TransferUser = async (request) => {
    return PostAPI(request, 'Users/TransferUsers');
};

export const SelectByType = async (request) => {
    return PostAPI(request, 'Users/SelectByType');
};
export const SelectByRole = async (request) => {
    return PostAPI(request, 'Users/SelectByRole');
};
export const SelectByRoleUserBulk = async (request) => {
    return PostAPI(request, 'Users/SelectByRoleUserBulk');
};
export const FetchDetailsForPincode = async (request) => {
    return PostAPI(request, 'Users/FetchDetailsForPincode');
};

export const BulkUpdate = async (request) => {
    return PostAPI(request, 'Users/BulkUpdate');
};
export const ReKyc = async (request) => {
    return PostAPI(request, 'Users/ReKyc');
};
export const ForgotPasswordSendOTP = async (request) => {
    return PostAPI(request, 'Credential/ForgotPasswordGenerateOTP');
};
export const VerifyForgotPasswordOTP = async (request) => {
    return PostAPI(request, 'Credential/VerifyForgotPasswordOTP');
};
export const SelectUserOnbordingList = async (request) => {
    return PostAPI(request, 'Users/SelectUserOnbordingList');
};
export const SelectOnlyParentUserForDropdown = async (request) => {
    return PostAPI(request, 'Users/SelectOnlyParentUserForDropdown');
};
export const SelecteKYCUserDetailsByUserID = async (request) => {
    return PostAPI(request, 'Users/SelecteKYCUserDetailsByUserID');
};

export const SelectDirectDownline = async (request) => {
    return PostAPI(request, 'Users/SelectDirectDownline');
};
export const Select = async (request) => {
    return PostAPI(request, 'UserType/Select');
};
export const SelectV1 = async (request) => {
    return PostAPI(request, 'UserType/SelectV1');
};

export const SelectDirectDownlineForUserBulkSelect = async (request) => {
    return PostAPI(request, 'Users/SelectDirectDownlineForUserBulk');
};

export const SelectDownlineForLedger = async (request) => {
    return PostAPI(request, 'Users/SelectDownlineForLedger');
};
export const SelectUserMorePayOnBoarding = async (request) => {
    return PostAPI(request, 'Users/SelectUserMorePayOnBoarding');
};

export const SelectUserByUserID = async (request) => {
    return PostAPI(request, 'Users/SelectUserByUserID');
};
export const PanView = async (request) => {
    return PostAPI(request, 'Users/PanView');
};
export const GSTView = async (request) => {
    return PostAPI(request, 'Users/GSTView');
};
export const SelectUserOnbordingStatusCheckList = async (request) => {
    return PostAPI(request, 'Users/SelectUserOnbordingStatusCheckList');
};
