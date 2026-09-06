import { PostAPI } from 'Components/Helper/Helpers';

export const SelectByID = async (request) => {
    return PostAPI(request, 'LastChangeUserDetails/SelectByID');
};
