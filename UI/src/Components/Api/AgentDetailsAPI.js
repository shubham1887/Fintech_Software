import { PostAPI } from 'Components/Helper/Helpers';

export const Upload = async (request) => {
    return PostAPI(request, 'AgentDetails/Upload');
};
export const SelectByPinCode = async (request) => {
    return PostAPI(request, 'AgentDetails/SelectByPinCode');
};

export const FileUpload = async (request) => {
    return PostAPI(request, 'Recon/Upload');
};