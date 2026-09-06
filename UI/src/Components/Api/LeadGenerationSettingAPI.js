import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'LeadGenerationSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'LeadGenerationSetting/Select');
};

export const IsOnOff = async (request) => {
    return PostAPI(request, 'LeadGenerationSetting/IsOnOff');
};

export const ActiveLeadGeneration = async (request) => {
    return PostAPI(request, 'LeadGenerationSetting/SelectActiveLeadGeneration');
};

export const MultiActiveLeadGeneration = async (request) => {
    return PostAPI(request, 'LeadGenerationSetting/SelectActive');
};
