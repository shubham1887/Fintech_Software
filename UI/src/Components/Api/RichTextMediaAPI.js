import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'RichTextMedia/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'RichTextMedia/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'RichTextMedia/SelectByID');
};

export const SelectByKey = async (request) => {
    return PostAPI(request, 'RichTextMedia/SelectDigitalCertificateByKey');
};
