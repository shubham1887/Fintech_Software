import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import CurrencyRupeeIcon from '@mui/icons-material/CurrencyRupee';
import { LOGOUT } from 'ConstComponents';
import { Container, Grid, Button, Paper } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { useHistory } from 'react-router';
import { ServiceSubscriptionPayment } from 'Components/Api/ServiceSubscriptionChargeAPI';
import { Typography } from '@material-ui/core';
const CustomContainer = HOCLoader(Container);
export const SubscriptionPayment = ({
    ServiceAndUserwiseSubscriptionID,
    StartDate,
    EndDate,
    PaymentEndDate,
    ServiceName,
    SubscriptionType,
    Charge,
    OriginalCharge,
    Discount,
    PlanName,
    IsPaymentOverDue,
    requestObj
}) => {
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const history = useHistory();
    const [FormObj, setFormObj] = useState({
        ServiceAndUserwiseSubscriptionID: ServiceAndUserwiseSubscriptionID,
        StartDate: StartDate,
        EndDate: EndDate,
        PaymentEndDate: PaymentEndDate,
        ServiceName: ServiceName,
        SubscriptionType: SubscriptionType,
        Charge: Charge,
        OriginalCharge: OriginalCharge,
        IsPaymentOverDue: IsPaymentOverDue,
        Discount: Discount,
        requestObj: requestObj
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        var request = {
            ServiceAndUserwiseSubscriptionID: ServiceAndUserwiseSubscriptionID
        };
        var requestObj = JSON.stringify(request);

        setIsError(false);
        setConfirmAlertRequest(requestObj);
        setIsAlertOpen(true);
        setSubmitting(false);
    };

    const PayNow = async (requestObj) => {
        setLoadingOpen(true);
        setIsError(false);
        setConfirmAlertRequest(requestObj);

        const response = await ServiceSubscriptionPayment(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            if (
                response.data.toLowerCase() ===
                'Insufficient balance'.toLowerCase()
            )
                history.push('/PaymentGatewayTransaction');

            return;
        }
        var data = JSON.parse(response.data);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data);

        //if (requestObj == 4) {
        history.push('/RetailerDashboard');
        // }
        // else {
        //     history.push('/Demat');
        // }
        return;
    };

    const UploadForm = (formProps) => {
        return (
            <Form>
                <h2 className={classes.PaperHead_h2}>
                    Service Subscription Payment
                </h2>
                <Paper className={classes.PaperStyle}>
                    <Grid container spacing={2} xs={12}>
                        <Grid item xs={12} sm={12} md={4} lg={4}>
                            {' '}
                        </Grid>
                        <Grid item xs={12} sm={12} md={6} lg={6}>
                            <Grid container spacing={2} xs={12}>
                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={4}
                                    lg={4}
                                    style={{
                                        fontSize: '16px',
                                        fontWeight: '600'
                                    }}
                                >
                                    Service :
                                </Grid>
                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={6}
                                    lg={6}
                                    style={{ fontSize: '16px' }}
                                >
                                    {ServiceName}
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={4}
                                    lg={4}
                                    style={{
                                        fontSize: '16px',
                                        fontWeight: '600'
                                    }}
                                >
                                    Renew :
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={6}
                                    lg={6}
                                    style={{ fontSize: '16px' }}
                                >
                                    {SubscriptionType}
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={4}
                                    lg={4}
                                    style={{
                                        fontSize: '16px',
                                        fontWeight: '600'
                                    }}
                                >
                                    Plan :
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={6}
                                    lg={6}
                                    style={{ fontSize: '16px' }}
                                >
                                    {PlanName}
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={4}
                                    lg={4}
                                    style={{
                                        fontSize: '16px',
                                        fontWeight: '600'
                                    }}
                                >
                                    Start Date :
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={6}
                                    lg={6}
                                    style={{ fontSize: '16px' }}
                                >
                                    {StartDate}
                                </Grid>
                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={4}
                                    lg={4}
                                    style={{
                                        fontSize: '16px',
                                        fontWeight: '600'
                                    }}
                                >
                                    End Date :
                                </Grid>
                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={6}
                                    lg={6}
                                    style={{ fontSize: '16px' }}
                                >
                                    {EndDate}
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={4}
                                    lg={4}
                                    style={{
                                        fontSize: '16px',
                                        fontWeight: '600'
                                    }}
                                >
                                    Charge :
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={3}
                                    lg={6}
                                    style={{ fontSize: '18px' }}
                                >
                                    <Typography
                                        component="div"
                                        sx={{
                                            fontSize: '20px',
                                            fontWeight: '400'
                                        }}
                                    >
                                        {' '}
                                        <span style={{ marginRight: '20px' }}>
                                            <CurrencyRupeeIcon />
                                            {OriginalCharge}
                                        </span>
                                    </Typography>
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={4}
                                    lg={4}
                                    style={{
                                        fontSize: '16px',
                                        fontWeight: '600'
                                    }}
                                >
                                    Paying Charge :
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={3}
                                    lg={6}
                                    style={{ fontSize: '18px' }}
                                >
                                    <Typography
                                        component="div"
                                        sx={{
                                            fontSize: '20px',
                                            fontWeight: '400'
                                        }}
                                    >
                                        {' '}
                                        <span style={{ marginRight: '20px' }}>
                                            <CurrencyRupeeIcon />
                                            {Charge}
                                        </span>
                                    </Typography>
                                </Grid>
                            </Grid>
                        </Grid>
                    </Grid>
                    <Grid container spacing={2} xs={12}>
                        <Grid item xs={12} sm={12} md={6} lg={6}>
                            {' '}
                        </Grid>
                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <Button
                                    style={{
                                        margin: '10px',
                                        alignItems: 'right'
                                    }}
                                    type="submit"
                                    variant="contained"
                                    color="primary"
                                    size="small"
                                    onClick={(e) => {
                                        formProps.handleSubmit(e);
                                    }}
                                >
                                    Pay Now
                                </Button>
                            </Grid>
                        </Grid>
                    </Grid>
                </Paper>
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
                onSubmit={handleSubmit}
                resetForm={FormObj}
            >
                {UploadForm}
            </Formik>

            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Payment Confirmation"
                    message="Are you sure to pay subscription charge ?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        PayNow(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
