import React, { useEffect, useState } from 'react';
import { LOGOUT } from 'ConstComponents';
import { useHistory } from 'react-router';
import AEPSIcon from 'Components/DashboardIcon/AEPSIcon';
import { MultiActiveAEPS as AEPS_MULTI_ACTIVE } from 'Components/Api/AEPSAPI';
import { Container, Grid, Paper, IconButton } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PLEASE_PROVIDE_VALID, AEPS_CODE } from 'Components/Helper/Messages';
import { IsPaymentDueForService } from 'Components/Api/ServiceSubscriptionChargeAPI';
import { SubscriptionRenew } from 'Pages/InnerPages/ServiceSubscriptionCharge/SubscriptionRenew';


const CustomContainer = HOCLoader(Container);

export const AEPSIndex = () => {
    const history = useHistory();
    const classes = commonStyle();
    const [severity, setSeverity] = useState('success');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [Actives, setActive] = useState([]);
    const [renewMessage, setRenewMessage] = useState('');
    const [isExpairAlertOpen, setIsExpairAlertOpen] = useState(false);
    const [isRenewAlertOpen, setIsRenewAlertOpen] = useState(false);
    const [requestObj, setRequestObj] = useState(AEPS_CODE);

    const selectActiveAEPS = async () => {
        const response = await AEPS_MULTI_ACTIVE();

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }
        let data = JSON.parse(response.data);
        if (data.length <= 1) {
            handleClick(data[0].Name);
        }

        setActive(data);
    };
    const AEPSSubscription = async () => {
        var requestObj = { ServiceID: AEPS_CODE };
        setLoadingOpen(true);
        var request = JSON.stringify(requestObj);

        setIsError(false);

        var response = await IsPaymentDueForService(request);
        setLoadingOpen(false);

        setIsError(true);

        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        let data = JSON.parse(response.data);
        if (data.Message === 'No Record Found') {
            setIsExpairAlertOpen(false);
            setIsRenewAlertOpen(false);
            selectActiveAEPS();
            return;
        }

        setRenewMessage(data.Message);
        if (data.IsPaymentOverDue === true) {
            setSeverity('error');
            setIsExpairAlertOpen(true);
            return;
        }
        if (data.Days <= 10) {
            setIsRenewAlertOpen(true);
            selectActiveAEPS();
            return;
        }
        selectActiveAEPS();
    };


    const handleClick = (name) => {
        if (name.toLowerCase() === 'InstantPayWebSDK'.toLowerCase()) {
            history.push('/Transaction/ThirdPartyService');
            return;
        } else if (name.toLowerCase() === 'Paytm'.toLowerCase()) {
            history.push('/PAEPS');
            return;
        } else if (name.toLowerCase() === 'Mahagram'.toLowerCase()) {
            history.push('/MAEPS');
            return;
        } else if (name.toLowerCase() === 'Paysprint'.toLowerCase()) {
            history.push('/PSOnbording');
            return;
        } else if (name.toLowerCase() === 'PaysprintPaytm'.toLowerCase()) {
            history.push('/PSPOnbording');
            return;
        } else if (name.toLowerCase() === 'InstantPay'.toLowerCase()) {
            history.push('/IPMOnbording');
            return;
        } else if (name.toLowerCase() === 'Alle'.toLowerCase()) {
            history.push('/AlleAEPS');
            return;
        } else if (name.toLowerCase() === 'Eko'.toLowerCase()) {
            history.push('/EAEPS');
            return;
        } else if (name.toLowerCase() === 'Fingpay'.toLowerCase()) {
            history.push('/FPAEPS');
            return;
        } else if (name.toLowerCase() === 'ApiClub'.toLowerCase()) {
            history.push('/ACAEPS');
            return;
        } else if (name.toLowerCase() === 'EzyPay'.toLowerCase()) {
            history.push('/EzyPayAEPS');
            return;
        } else if (name.toLowerCase() === 'RapiPay'.toLowerCase()) {
            history.push('/RPAEPS');
            return;
        } else if (name.toLowerCase() === 'CredoPay'.toLowerCase()) {
            history.push('/CPAEPS');
            return;
        }
    };

    useEffect(() => {
        AEPSSubscription();
    }, []);

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
            <Paper className={classes.PaperStyle}>
                <Grid container>
                    {Actives.map((item) => (
                        <Grid
                            item
                            xs={12}
                            sm={12}
                            md={6}
                            lg={2}
                            xl={2}
                            className="text-center"
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={() => {
                                    handleClick(item.Name);
                                }}
                            >
                                <AEPSIcon name={item.DisplayName} />
                            </IconButton>
                        </Grid>
                    ))}
                </Grid>
            </Paper>
            {isExpairAlertOpen ? (
                <SubscriptionRenew
                    isOpen={isExpairAlertOpen}
                    title="Expir Subscription"
                    error={renewMessage}
                    submitCallback={(value) => {
                        setIsExpairAlertOpen(value);
                    }}
                    resendCallback={(value) => { }}
                    onDismissCallback={(value) => {
                        setIsExpairAlertOpen(value);
                    }}
                    requestObj={requestObj}
                />
            ) : null}

            {isRenewAlertOpen ? (
                <SubscriptionRenew
                    isOpen={isRenewAlertOpen}
                    title="Renew Subscription"
                    error={renewMessage}
                    submitCallback={(value) => {
                        setIsRenewAlertOpen(value);
                    }}
                    resendCallback={(value) => { }}
                    onDismissCallback={(value) => {
                        setIsRenewAlertOpen(value);
                    }}
                    requestObj={requestObj}
                />
            ) : null}
        </CustomContainer>
    );
};
