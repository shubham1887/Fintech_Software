import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'ServiceAndUserwiseSubscription/Select');
};

export const Insert = async (request) => {
    return PostAPI(request, 'ServiceAndUserwiseSubscription/Insert');
};
export const Delete = async (request) => {
    return PostAPI(request, 'ServiceAndUserwiseSubscription/Delete');
};
export const AutoDebitOnOff = async (request) => {
    return PostAPI(request, 'ServiceAndUserwiseSubscription/AutoDebitOnOff');
};

export const Update = async (request) => {
    return PostAPI(request, 'ServiceAndUserwiseSubscription/Update');
};

export const SelectChargeReport = async (request) => {
    return PostAPI(
        request,
        'ServiceAndUserwiseSubscription/SelectChargeReport'
    );
};

export const SelectPendingChargeReport = async (request) => {
    return PostAPI(
        request,
        'ServiceAndUserwiseSubscription/SelectPendingSubscriptionCharge'
    );
};

export const SelectUserWiseSubscriptionCharge = async (request) => {
    return PostAPI(
        request,
        'ServiceAndUserwiseSubscription/SelectServiceCharge'
    );
};

export const ServiceSubscriptionPayment = async (request) => {
    return PostAPI(
        request,
        'ServiceAndUserwiseSubscription/ServiceSubscriptionPayment'
    );
};

export const SelectPaymentReport = async (request) => {
    return PostAPI(
        request,
        'ServiceAndUserwiseSubscription/SelectSubscriptionPaymentReport'
    );
};

export const IsPaymentDueForService = async (request) => {
    return PostAPI(
        request,
        'ServiceAndUserwiseSubscription/IsPaymentDueForService'
    );
};

export const ApprovalByAdmin = async (request) => {
    return PostAPI(
        request,
        'ServiceAndUserwiseSubscription/PaymentApprovalByAdmin'
    );
};

export const UPDATE_STATUS = async (request) => {
    return PostAPI(
        request,
        'ServiceAndUserwiseSubscription/UpdateSubscriptionStatus'
    );
};