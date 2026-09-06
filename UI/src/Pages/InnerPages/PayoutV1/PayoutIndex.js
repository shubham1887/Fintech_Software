import React, { useEffect, useState } from 'react';

import { LOGOUT } from 'ConstComponents';

import { useHistory } from 'react-router';

import { SelectActivePayout as PAYOUT_ACTIVE } from 'Components/Api/PayoutSettingAPI';

import { Container, Grid, Paper } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

const PayoutIndexV1 = () => {
    const history = useHistory();

    const classes = commonStyle();
    const [severity, setSeverity] = useState('success');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);

    const selectActivePayout = async () => {
        const response = await PAYOUT_ACTIVE();

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }
        let data = JSON.parse(response.data);

        let name = data.Name.toLowerCase();

        if (name === 'Paysprint'.toLowerCase()) {
            history.push('/PSPayout');
            return;
        } else if (name === 'IMoneyPay'.toLowerCase()) {
            history.push('/IMPPayout');
            return;
        } else if (name === 'PrimePay'.toLowerCase()) {
            history.push('/PPPayout');
            return;
        } else if (name === 'EZYPay'.toLowerCase()) {
            history.push('/E_Payout');
            return;
        } else {
            let param = {
                ActivePayoutName: name
            };
            history.push('/PayoutTransactionV1', param);
            return;
        }
    };

    useEffect(() => {
        selectActivePayout();
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
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={12} md={12} lg={12}>
                        Please wait
                    </Grid>
                </Grid>
            </Paper>
        </CustomContainer>
    );
};
export default PayoutIndexV1;
