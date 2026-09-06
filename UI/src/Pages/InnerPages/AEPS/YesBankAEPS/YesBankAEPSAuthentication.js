import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { Container, Grid, Paper } from '@material-ui/core';
import { DailyAuthentication as DAILY_AUTHENTICATION } from 'Components/Api/YesBankAEPS';
import { FormControlLabel, Checkbox } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID, AEPS_CODE } from 'Components/Helper/Messages';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import HOCLoader from 'Components/Loader/HOCLoader';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import { FailTransactionResponse } from 'Components/Alert/FailTransactionResponse';
import { GetFingerPrint } from 'Components/FingerPrintProcessing';
import usePosition from 'Components/Posistion';
import { useLocation } from 'react-router-dom';
import { SubscriptionConfirmation } from 'Components/Alert/SubscriptionConfirmation';
import { SelectActiveService as SELECT_ACTIVE } from 'Components/Api/ServiceSubscriptionChargeAPI';

const CustomContainer = HOCLoader(Container);

export const YesBankAEPSAuthentication = () => {
    const history = useHistory();
    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }
    const formRef = useRef();
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [Message, setMessage] = useState('');
    const [isAlertErrorOpen, setIsAlertErrorOpen] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [status, setStatus] = useState('PROCESS');
    const aadharNumber = localStorage.getItem('AadharNo');
    const location = useLocation();
    const { IconName } = location.state || {};

    const { latitude, longitude, error } = usePosition();
    const [formObj, setFormObj] = useState({
        adhaarnumber: aadharNumber,
        FingerPrint: '',
        Type: '',
        Latitude: latitude,
        Longitude: longitude,
        agreeToTerms: false
    });
    const reset = () => {
        setFormObj({
            adhaarnumber: '',
            FingerPrint: '',
            Type: '',
            Latitude: latitude,
            Longitude: longitude,
            agreeToTerms: false
        });
    };

    const validationSchema = Yup.object({
        adhaarnumber: Yup.string()
            .length(12, 'Must be 12 digit')
            .required(PLEASE_PROVIDE_VALID + 'aadhaar number'),
        agreeToTerms: Yup.boolean().oneOf(
            [true],
            'You must accept the terms and conditions'
        )
    });

    const SubmitHandle = async (values, { resetForm }) => {
        setIsError(false);
        setResponseMessage('');
        setLoadingOpen(true);

        const { status, xmldata } = await GetFingerPrint('');

        if (status === false) {
            alert(xmldata);
            return;
        }
        let fingerPrintXML = xmldata;
        var request = {
            FingerPrint: fingerPrintXML,
            AadhaarNumber: values.adhaarnumber,
            Type: 'DAILY_LOGIN',
            Latitude: latitude,
            Longitude: longitude
        };

        var requestObj = JSON.stringify(request);
        var response = await DAILY_AUTHENTICATION(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');

        if (!response.status) {
            var data = JSON.parse(response.data);
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data.Message);

            setStatus('FAILURE');
            setMessage(data.Message);
            setIsAlertErrorOpen(true);
            return;
        }
        history.push('/YBAEPS', {
            IconName: IconName
        });
    };

    useEffect(() => {
        fetchConsumerNumberFontSize();
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
            <Formik
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={SubmitHandle}
                resetForm={formObj}
            >
                {({ setFieldValue }) => {
                    return (
                        <Grid
                            item
                            container
                            margin="normal"
                            xs={12}
                            md={12}
                            sm={12}
                            lg={12}
                            spacing={2}
                        >
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <Form>
                                    <Grid item xs={12} sm={12}>
                                        <Paper className={classes.PaperStyle}>
                                            <Grid container spacing={1}>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <MaterialNumberField
                                                        font_size={
                                                            consumerNumberFontSize
                                                        }
                                                        label="Aadhaar Number/VID*"
                                                        name="adhaarnumber"
                                                        placeholder="Aadhaar No/VID*"
                                                        title="Aadhaar No"
                                                        length="16"
                                                        typed_length={1}
                                                    />
                                                </Grid>

                                                <FormControlLabel
                                                    control={
                                                        <Field
                                                            style={{
                                                                marginTop:
                                                                    '-21px'
                                                            }}
                                                            as={Checkbox}
                                                            name="agreeToTerms"
                                                            color="primary"
                                                        />
                                                    }
                                                    label={
                                                        <span>
                                                            By clicking on
                                                            Proceed, you provide
                                                            your Aadhaar consent
                                                            to Hitachi Money
                                                            Spot Plus to fetch
                                                            your details from
                                                            UIDAI website
                                                        </span>
                                                    }
                                                />
                                                <ErrorMessage
                                                    style={{
                                                        color: 'red'
                                                    }}
                                                    name="agreeToTerms"
                                                    component="div"
                                                    className="field-error-red"
                                                />
                                            </Grid>
                                        </Paper>
                                        <Grid className={classes.footerGrid}>
                                            <div
                                                className={
                                                    classes.SubmitWrapper
                                                }
                                            >
                                                <div className="container">
                                                    <div className="row">
                                                        <div className="col-md-6">
                                                            <SecondaryButton
                                                                disabled={
                                                                    loadingOpen
                                                                }
                                                                title=" Reset"
                                                                onClick={() => {
                                                                    reset();
                                                                }}
                                                            />
                                                        </div>
                                                        <div className="col-md-6">
                                                            <PrimaryButton
                                                                title="Proceed"
                                                                disabled={
                                                                    loadingOpen
                                                                }
                                                            />
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </Grid>
                                    </Grid>
                                </Form>
                            </Grid>
                        </Grid>
                    );
                }}
            </Formik>

            {isAlertErrorOpen && (
                <FailTransactionResponse
                    isOpen={isAlertErrorOpen}
                    title={status}
                    Message={Message}
                    OkButtonClick={(request) => {
                        setIsAlertErrorOpen(false);
                    }}
                />
            )}
        </CustomContainer>
    );
};
