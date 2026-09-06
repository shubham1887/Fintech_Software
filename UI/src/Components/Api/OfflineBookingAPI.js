import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'OfflineBookingDetails/Insert');
};
export const Select = async (request) => {
    return PostAPI(request, 'OfflineBookingDetails/SelectByUserID');
};
export const SelectFilter = async (request) => {
    return PostAPI(request, 'OfflineBookingDetails/SelectForFilter');
};
export const UpdateStatusmanually = async (request) => {
    return PostAPI(request, 'OfflineBookingDetails/UpdateStatusManually');
};
