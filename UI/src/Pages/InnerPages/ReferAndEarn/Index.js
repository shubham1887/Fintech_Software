import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';

import CopyToClipBoardInputBigText from 'Components/CopyToClipBoardInputBigText';
import CopyToClipBoardInput from 'Components/CopyToClipBoardInput';

import { SelectURL as SELECTURL } from 'Components/Api/UserReferralAPI';
import { Container, Grid, Box, Typography } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import HOCLoader from 'Components/Loader/HOCLoader';

import Paper from '@material-ui/core/Paper';

import { API_URL } from 'Config';

const CustomContainer = HOCLoader(Container);

export const ReferAndEarn = () => {
    const formRef = useRef();
    const classes = commonStyle();

    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [ReferralBenefitMinAmount, setReferralBenefitMinAmount] = useState(0);
    const [ReferralAmount, setReferralAmount] = useState(0);
    const [ReferralCode, setReferralCode] = useState('');
    const [ReferralURL, setReferralURL] = useState('');

    const [firmName] = useState(localStorage.getItem('FirmName'));

    const [baseURL, setBaseURL] = useState('');
    const getBaseUrl = async () => {
        setBaseURL(await API_URL());
    };
    useEffect(() => {
        getBaseUrl();
    }, []);

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);

        setSubmitting(false);
    };

    const selectData = async () => {
        const response = await SELECTURL();
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(response.data);

        setReferralBenefitMinAmount(data.ReferralBenefitMinAmount);
        setReferralAmount(data.ReferralAmount);
        setReferralURL(data.ReferralURL);
        setReferralCode(data.ReferralCode);
    };

    useEffect(() => {
        selectData();
    }, []);

    return (
        <CustomContainer
            isLoading={loading}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Formik innerRef={formRef} onSubmit={handleSubmit}>
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <h2 className={classes.PaperHead_h2}>
                                Refer & Earn
                            </h2>

                            <Grid item xs={12}>
                                <Paper>
                                    <Grid
                                        className={classes.footerGrid}
                                        container
                                        style={{ margin: 20 }}
                                    >
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <Typography>
                                                <span
                                                    style={{
                                                        color: 'green',
                                                        fontSize: '30px'
                                                    }}
                                                >
                                                    Hey <b> {firmName} </b>,
                                                    it’s time to pass it on.
                                                </span>
                                            </Typography>

                                            <Typography>
                                                <span
                                                    style={{
                                                        color: 'green',
                                                        fontSize: '30px',
                                                        marginLeft: '20px',
                                                        marginBottom: '20px'
                                                    }}
                                                >
                                                    Refer a friend and you will
                                                    receive Rs.
                                                    <b> {ReferralAmount} </b>
                                                    after their successful
                                                    prepaid transaction above
                                                    Rs.
                                                    <b>
                                                        {
                                                            ReferralBenefitMinAmount
                                                        }
                                                    </b>
                                                    .
                                                </span>
                                            </Typography>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <Typography>
                                                <span
                                                    style={{
                                                        fontSize: '30px',
                                                        fontFamily: 'Italic',
                                                        marginTop:
                                                            '10px !important'
                                                    }}
                                                >
                                                    Referral Code:
                                                </span>
                                                <span
                                                    style={{
                                                        fontSize: '20px',
                                                        fontFamily: 'bold'
                                                    }}
                                                >
                                                    <CopyToClipBoardInput
                                                        text={ReferralCode}
                                                    />
                                                </span>
                                            </Typography>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <CopyToClipBoardInputBigText
                                                text={ReferralURL}
                                            />
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>

                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <Box>
                                    <img
                                        style={{ width: '100%' }}
                                        src={baseURL + 'app-docs/refer.jpg'}
                                        alt={''}
                                    />
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
