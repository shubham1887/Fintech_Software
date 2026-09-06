import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'LeadGenerationMaster/Insert');
};

export const InsertLead = async (request) => {
    return PostAPI(request, 'OnlyAPILeadGeneration/LeadTransaction');
};

export const Select = async (request) => {
    return PostAPI(request, 'LeadGenerationMaster/Select');
};

export const Update = async (request) => {
    return PostAPI(request, 'LeadGenerationMaster/Update');
};

export const SelectLoanType = async (request) => {
    return PostAPI(request, 'LeadGenerationMaster/SelectLoanType');
};

export const SelectConsumerTypes = async (request) => {
    return PostAPI(request, 'LeadGenerationMaster/SelectConsumerTypes');
};