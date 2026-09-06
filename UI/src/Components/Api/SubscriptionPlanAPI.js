import { PostAPI } from 'Components/Helper/Helpers';

export const PlanInsert = async (request) => {
    return PostAPI(request, 'SubscriptionPlan/Create');
};

export const UpdateSubsciption = async (request) => {
    return PostAPI(request, 'SubscriptionPlan/Update');
};

export const DeleteSubsciption = async (request) => {
    return PostAPI(request, 'SubscriptionPlan/Delete');
};

export const SelectPlan = async (request) => {
    return PostAPI(request, 'SubscriptionPlan/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'SubscriptionPlan/SelectByID');
};

export const SelectByDetailID = async (request) => {
    return PostAPI(request, 'SubscriptionPlan/SelectByDetailID');
};

export const SelectActiveService = async (request) => {
    return PostAPI(request, 'SubscriptionPlan/IsServiceAndEKYCActive');
};

export const SelectSubscriptionPlan = async (request) => {
    return PostAPI(request, 'SubscriptionPlan/SelectPlanList');
};

export const AssignPlan = async (request) => {
    return PostAPI(request, 'SubscriptionPlan/Subscribe');
};

export const SelectByUserID = async (request) => {
    return PostAPI(request, 'SubscriptionPlan/IsSubscribed');
};
