import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form } from 'formik';
import { LOGOUT } from 'ConstComponents';
import { AssignPlan as ASSIGN_PLAN } from 'Components/Api/SubscriptionPlanAPI';
import CircularProgress from '@material-ui/core/CircularProgress';
import {
    Container,
    Grid,
    Paper,
    Tabs,
    Tab,
    Typography,
    List,
    ListItem,
    ListItemIcon,
    ListItemText
} from '@material-ui/core';

import CheckCircleIcon from '@material-ui/icons/CheckCircle';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectSubscriptionPlan as SELECT_PLAN } from 'Components/Api/SubscriptionPlanAPI';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { SubscriptionResponse } from 'Components/Alert/SubscriptionResponse';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { useLocation } from 'react-router-dom';
import { useHistory } from 'react-router';
const CustomContainer = HOCLoader(Container);

export const Subscription = ({ onDismissCallback }) => {
    const formRef = useRef();
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const history = useHistory();
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [ImageURL, setImageUrl] = useState([]);
    const [purchaseList, setPurchaseList] = useState([]);
    const [serviceList, setServiceList] = useState([]);
    const [amount, setAmount] = useState(0);
    const [selectedTab, setSelectedTab] = useState(0);
    const [plans, setPlans] = useState([]);
    const [isConfirmationOpen, setIsConfirmationOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [confirmRequest, setConfirmRequest] = useState({});
    const location = useLocation();
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const serviceId = location?.state?.ServiceID;

    const [formObj, setFormObj] = useState({
        ServiceSubscriptionCreatePlanMasterID: '0',
        PlanAmount: '',
        PlanName: '',
        ServiceID: 0
    });

    useEffect(() => {
        select();
    }, [serviceId]);

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;
        var jsonReq = {
            PlanName: values.PlanName,
            PlanAmount: values.PlanAmount,
            ServiceSubscriptionCreatePlanID:
                values.ServiceSubscriptionCreatePlanMasterID
        };
        setIsError(false);
        var requestObj = JSON.stringify(jsonReq);
        setLoadingOpen(true);
        try {
            var response = await ASSIGN_PLAN(requestObj);
            setLoadingOpen(false);
            if (!response.isAuthorized) LOGOUT();
            let data;
            try {
                data = JSON.parse(response.data);
            } catch (error) {
                // If parsing fails, treat the response as a plain string message
                data = {
                    Message: response.data
                };
            }

            if (!response.status) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage(data.Message);
                return;
            }
            // setIsError(true);
            setSeverity('success');
            setResponseMessage(data.Message);
            setSubmitting(false);
            resetForm(formObj);
            setConfirmRequest(data);
            setIsResponseAlertOpen(true);
            if (typeof onDismissCallback === 'function') {
                onDismissCallback(false);
            }
        } catch (error) {
            setLoadingOpen(false);
            setIsError(true);
            setSeverity('error');
            setResponseMessage('An unexpected error occurred');
        }
    };

    const select = async () => {
        var jsonReq = {
            ServiceID: serviceId
        };
        var requestObj = JSON.stringify(jsonReq);
        setLoadingOpen(true);
        const response = await SELECT_PLAN(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;
        var data = JSON.parse(response.data);
        setPlans(data);
        if (Array.isArray(data)) {
            // Extract image URLs or set a placeholder if undefined
            const imageUrls = data.map(
                (plan) => plan.ImageURL || 'path/to/placeholder/image.jpg'
            );
            setImageUrl(imageUrls);

            const purchaseArray = data.map(
                (plan) => plan.IsPlanPurchase || false
            );
            setPurchaseList(purchaseArray);

            const serviceArray = data.map((plan) => plan.Services);
            setServiceList(serviceArray);
        }

        if (data.length > 0) {
            setFormObj({
                ServiceSubscriptionCreatePlanMasterID:
                    data[0].ServiceSubscriptionCreatePlanMasterID,
                PlanAmount: data[0].Amount,
                PlanName: data[0].PlanName
            });
        }
    };

    const handleTabChange = (event, newValue, setFieldValue) => {
        setSelectedTab(newValue);

        // Update form fields based on selected plan
        const selectedPlan = plans[newValue];
        if (selectedPlan) {
            setFieldValue(
                'ServiceSubscriptionCreatePlanMasterID',
                selectedPlan.ServiceSubscriptionCreatePlanMasterID
            );
            setFieldValue('PlanAmount', selectedPlan.Amount);
            setFieldValue('PlanName', selectedPlan.PlanName);
            setAmount(selectedPlan.Amount);
        }
    };

    useEffect(() => {
        // Set initial values when component mounts or when plans change
        if (plans.length > 0) {
            updateSelectedPlan(0);
        }
    }, [plans]);

    const updateSelectedPlan = (tabIndex) => {
        const selectedPlan = plans[tabIndex];
        if (selectedPlan) {
            setAmount(selectedPlan.Amount);
        }
    };

    const handleConfirm = () => {
        setIsConfirmationOpen(false);
        if (formRef.current) {
            formRef.current.submitForm();
        }
    };
    const handlePayClick = (e) => {
        e.preventDefault();
        setIsConfirmationOpen(true);
    };

    const isAnyPlanPurchased = () => {
        return purchaseList.some((item) => item === true);
    };

    const planPurchased = () => {
        return (
            <>
                <Typography variant="h6" component="h3" gutterBottom>
                    You are subscribed below services:
                </Typography>
                <List>
                    {serviceList[selectedTab]?.map((service) => (
                        <ListItem
                            key={service.ServiceID}
                            className={classes.listItem}
                        >
                            <ListItemIcon>
                                <CheckCircleIcon color="primary" />
                            </ListItemIcon>
                            <ListItemText primary={service.ServiceName} />
                        </ListItem>
                    ))}
                </List>
            </>
        );
    };

    const planNotPurchased = () => {
        return (
            <>
                <img
                    src={ImageURL[selectedTab]}
                    alt={`Plan Image ${selectedTab + 1}`}
                    style={{
                        width: '280px',
                        height: '100%',
                        borderRadius: '8px',
                        marginBottom: '15px'
                    }}
                />
                <PrimaryButton
                    type="button"
                    title={isAnyPlanPurchased() ? 'Upgrade' : 'Subscribe'}
                    className={classes.CustomPrimaryButtonStyle}
                    disabled={loadingOpen}
                    style={{
                        padding: '10px 20px',
                        fontSize: '16px',
                        marginTop: '10px'
                    }}
                    onClick={handlePayClick}
                />
            </>
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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={formObj}
                // validationSchema={formValidationSchema}
                onSubmit={handleSubmit}
                resetForm={formObj}
                render={({ values, setFieldValue }) => {
                    return (
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <Form>
                                    <Grid item xs={12}>
                                        <Paper
                                            className={classes.PaperStyle}
                                            elevation={3}
                                            style={{
                                                padding: '20px',
                                                textAlign: 'center',
                                                display: 'flex',
                                                flexDirection: 'column',
                                                alignItems: 'center',
                                                justifyContent: 'center',
                                                width: '100%',
                                                maxWidth: '600px',
                                                margin: '0 auto'
                                            }}
                                        >
                                            <h2
                                                className={classes.PaperHead_h2}
                                            >
                                                Plan Subscription
                                            </h2>

                                            <Tabs
                                                value={selectedTab}
                                                onChange={(event, newValue) =>
                                                    handleTabChange(
                                                        event,
                                                        newValue,
                                                        setFieldValue
                                                    )
                                                }
                                                indicatorColor="primary"
                                                textColor="primary"
                                                centered
                                                style={{
                                                    marginBottom: '20px'
                                                }}
                                            >
                                                {plans.map((plan, index) => (
                                                    <Tab
                                                        label={plan.PlanName}
                                                        key={index}
                                                        style={{
                                                            padding:
                                                                '10px 20px',
                                                            fontSize: '16px'
                                                        }}
                                                    ></Tab>
                                                ))}
                                            </Tabs>
                                            <div
                                                style={{
                                                    padding: '20px',
                                                    textAlign: 'center',
                                                    boxShadow:
                                                        '0 4px 8px rgba(0,0,0,0.1)',
                                                    marginBottom: '20px'
                                                }}
                                            >
                                                {purchaseList[selectedTab] ===
                                                false
                                                    ? planNotPurchased()
                                                    : planPurchased()}
                                            </div>

                                            {loadingOpen && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </Paper>
                                    </Grid>
                                </Form>
                            </Grid>
                        </Grid>
                    );
                }}
            />

            {isConfirmationOpen ? (
                <ConfirmAlert
                    isOpen={isConfirmationOpen}
                    title="Confirmation"
                    request={confirmAlertRequest}
                    message={`Are you sure to purchase this plan a charge of ₹ ${amount} (inclusive of taxes) will be debited`}
                    positiveButtonClick={handleConfirm}
                    negativeButtonClick={() => setIsConfirmationOpen(false)}
                />
            ) : null}

            {isResponseAlertOpen ? (
                <SubscriptionResponse
                    isOpen={isResponseAlertOpen}
                    request={confirmRequest}
                    positiveButtonClick={() => {
                        setIsResponseAlertOpen(false);
                    }}
                    negativeButtonClick={() => {
                        setIsResponseAlertOpen(false);
                        history.push('/RetailerDashboard');
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
