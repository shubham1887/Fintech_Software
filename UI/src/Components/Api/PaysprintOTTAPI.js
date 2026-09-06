import { PostAPI } from 'Components/Helper/Helpers';

export const SelectSubscriptionOperator = async (request) => {
    return PostAPI(request, 'PaysprintOTT/SelectSubscriptionOperator');
};

export const SelectSubscriptionPlan = async (request) => {
    return PostAPI(request, 'PaysprintOTT/SelectSubscriptionPlan');
};

export const SelectBundleOperator = async (request) => {
    return PostAPI(request, 'PaysprintOTT/SelectBundleOperator');
};

export const SelectBundlePlan = async (request) => {
    return PostAPI(request, 'PaysprintOTT/SelectBundlePlan');
};
export const SubscriptionPayment = async (request) => {
    return PostAPI(request, 'PaysprintOTT/SubscriptionPayment');
};

export const BundlePayment = async (request) => {
    return PostAPI(request, 'PaysprintOTT/BundlePayment');
};
