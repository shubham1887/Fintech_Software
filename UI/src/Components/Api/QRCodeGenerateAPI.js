import { PostAPI } from 'Components/Helper/Helpers';
export const GenerateQRCode = async (request) => {
    return PostAPI(request, 'QRCodeGenerate/GenerateQRCode');
};
