import { PostAPI } from 'Components/Helper/Helpers';

export const GenerateURL = async (request) => {
    return PostAPI(request, 'PaysprintLeadgeneration/GenerateURL');
};
