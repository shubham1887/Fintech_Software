import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import { LOGOUT } from 'ConstComponents';
import { DEMAT } from 'Components/Helper/Messages';

import { Container, Grid, Paper } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { SubscriptionPayment } from 'Pages/InnerPages/ServiceSubscriptionCharge/SubscriptionPayment';
import { SelectUserWiseSubscriptionCharge as SELECT_SUBSCRIPTION_CHARGE } from 'Components/Api/ServiceSubscriptionChargeAPI';
import { useLocation } from 'react-router-dom';


const CustomContainer = HOCLoader(Container);
export const SubscriptionPendingPayment = () => {
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const location = useLocation();

    const ServiceID = location.state.requestObj;

    const [FormObj, setFormObj] = useState({
        ServiceAndUserwiseSubscriptionID: '',
        StartDate: '',
        EndDate: '',
        ServiceName: '',
        SubscriptionType: '',
        Charge: 0,
        OriginalCharge: 0,
        PlanName: ''
    });

    useEffect(() => {
        setLoadingOpen(false);
        selectSubscriptionCharge();
    }, []);

    const selectSubscriptionCharge = async () => {
        var requestObj = { ServiceID: ServiceID };
        setLoadingOpen(true);
        var request = JSON.stringify(requestObj);
        const response = await SELECT_SUBSCRIPTION_CHARGE(request);

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }
        let data = JSON.parse(response.data);
        setConfirmAlertRequest(data);
    };

    const UploadForm = (formProps) => {
        return (
            <Form>
                <Grid container spacing={12}>
                    <Grid item xs={12} sm={12} md={12} lg={12}></Grid>
                </Grid>

                <Grid item xs={12} sm={12} md={12} lg={12}>
                    <SubscriptionPayment
                        ServiceAndUserwiseSubscriptionID={
                            confirmAlertRequest.ServiceAndUserwiseSubscriptionID
                        }
                        StartDate={confirmAlertRequest.StartDate}
                        EndDate={confirmAlertRequest.EndDate}
                        ServiceName={confirmAlertRequest.ServiceName}
                        SubscriptionType={confirmAlertRequest.SubscriptionType}
                        Charge={confirmAlertRequest.Charge}
                        OriginalCharge={confirmAlertRequest.OriginalCharge}
                        Discount={confirmAlertRequest.Discount}
                        PlanName={confirmAlertRequest.PlanName}
                        requestObj={ServiceID}
                    />
                </Grid>
            </Form>
        );
    };

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Formik
                enableReinitialize={true}
                initialValues={FormObj}
                resetForm={FormObj}
            >
                {UploadForm}
            </Formik>
        </CustomContainer>
    );
};
