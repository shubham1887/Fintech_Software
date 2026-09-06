import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import {
    Select2FAQRImage as USER_SELECT_CODE,
    DeactivateWithOTP as DEACTIVATE_2FA_WITH_OTP,
    Send2FAOTP as SEND_2FA_OTP
} from 'Components/Api/UsersApi';

import { VerifyTwoFA as VERIFY_SETUP_TWOFA } from 'Components/Api/UsersApi';
import { SelectProfile as USER_SELECT_BY_ID } from 'Components/Api/UsersApi';

import { LOGOUT } from 'ConstComponents';
import { Container, Grid, Paper, Link, Box, Button } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import { OTPModal } from 'Components/Modal/OTPModal';
const CustomContainer = HOCLoader(Container);
export const TwoFA = () => {
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [imageQRCode, setimageQRCode] = useState('');
    const [SetUpCode, setSetUpCode] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [is2FVerification, setIs2FVerification] = useState(false);
    const [openOTP, setOpenOTP] = useState(false);
    const [modalErrorMessage, setModalErrorMessage] = useState('');

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }

        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);
        setIsError(false);

        var response = await VERIFY_SETUP_TWOFA(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        setResponseMessage(response.data);
        setIsError(true);
        setSubmitting(false);

        setSeverity('success');
        if (!response.status) {
            setSeverity('error');
            return;
        }
    };

    useEffect(() => {
        SelectByID();
    }, []);

    const SelectByID = async () => {
        setLoadingOpen(true);
        const { isAuthorized, status, data } = await USER_SELECT_BY_ID();
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) return;

        var d = JSON.parse(data);
        setFormObject({
            IsTwoFA: d.IsTwoFA
        });
    };

    const [formObj, setFormObject] = useState({
        IsTwoFA: false,
        TwoFAPasscode: ''
    });

    const Select2FAQRImage = async () => {
        setLoading(true);
        const { isAuthorized, status, data } = await USER_SELECT_CODE();

        setLoading(false);
        if (!isAuthorized) LOGOUT();

        if (!status) return;

        var response = JSON.parse(data);

        setIs2FVerification(true);
        setSetUpCode(response.SetUpCode);
        setimageQRCode(response.BarCodeImageURL);
    };

    const activate2FA = async (IsTwoFA) => {
        if (!IsTwoFA) {
            setOpenOTP(true);
            return;
        }

        if (IsTwoFA) {
            Select2FAQRImage();
        }
    };

    const OnChange = (name, value, setFieldValue) => {
        if (!value) Send2FAOTP(value);

        if (value) activate2FA(value);
    };

    const Send2FAOTP = async (IsTwoFA) => {
        setLoadingOpen(true);
        setOpenOTP(false);
        setModalErrorMessage('');
        setIsError(false);
        var response = await SEND_2FA_OTP();

        if (!response.isAuthorized) {
            LOGOUT();
        }

        setResponseMessage(response.data);
        setIsError(true);

        if (response.data === 'True') {
            setOpenOTP(true);
        }
    };

    const pass2FAOTP = async (otpObj) => {
        setIs2FVerification(false);
        setLoadingOpen(true);
        setOpenOTP(false);
        setModalErrorMessage('');
        const { OTP } = otpObj;
        let RequestObj = {
            OTP: OTP
        };
        var response = await DEACTIVATE_2FA_WITH_OTP(
            JSON.stringify(RequestObj)
        );
        setLoadingOpen(false);
        setIsError(true);
        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            setOpenOTP(true);
            setModalErrorMessage(response.data);
            return;
        }
        setOpenOTP(false);

        setResponseMessage(response.data);
        SelectByID();
    };

    const CustomOtpModal = openOTP ? (
        <OTPModal
            isOpen={openOTP}
            title="OTP"
            error={modalErrorMessage}
            submitCallback={(value) => {
                pass2FAOTP(value);
            }}
            resendCallback={(value) => {}}
            onDismissCallback={(value) => {
                setOpenOTP(value);
            }}
        />
    ) : null;

    const UploadForm = (formProps) => {
        return (
            <Form>
                <Grid item xs={12}>
                    <Grid container spacing={2}>
                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <h2 className={classes.PaperHead_h2}>
                                Two-Factor Authentication (2FA)
                            </h2>
                        </Grid>

                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <Box className={classes.flexReverse}>
                                <FormikMaterialSwitch
                                    label="Two-Factor Authentication (2FA)"
                                    name="IsTwoFA"
                                    onCustomCallback={(name, value) => {
                                        OnChange(
                                            name,
                                            value,
                                            formProps.setFieldValue
                                        );
                                    }}
                                />
                            </Box>
                        </Grid>
                    </Grid>
                    {is2FVerification === true ? (
                        <Paper className={classes.PaperStyle}>
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <div>
                                        <p>
                                            <h2>
                                                Enable Two-Factor Authentication
                                                (2FA) using below steps
                                            </h2>

                                            <br />

                                            <ol>
                                                <li>
                                                    Download
                                                    <Link
                                                        variant="body2"
                                                        rel="noopener"
                                                        target="_blank"
                                                        href={
                                                            'https://play.google.com/store/apps/details?id=com.google.android.apps.authenticator2'
                                                        }
                                                    >
                                                        Google Authenticator
                                                    </Link>
                                                    App
                                                </li>

                                                <li>
                                                    Scan the QR Code using above
                                                    downloaded App
                                                </li>
                                                <li>
                                                    Enter the 6 Digit Code,
                                                    which is displayed in app.
                                                </li>
                                                <li> Activate 2FA</li>
                                            </ol>
                                        </p>
                                    </div>
                                </Grid>
                                <Grid item xs={12} sm={12} md={8} lg={8}>
                                    <label
                                        style={{
                                            color: 'primary'
                                        }}
                                    >
                                        ManualEntryKey :
                                    </label>

                                    <label
                                        style={{
                                            color: 'primary'
                                        }}
                                    >
                                        {SetUpCode}
                                    </label>
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={12}
                                    lg={12}
                                    container
                                    justify="right"
                                    style={{
                                        marginTop: '10px',
                                        marginBottom: '10px'
                                    }}
                                >
                                    <img
                                        src={imageQRCode}
                                        height="300px"
                                        width="300px"
                                        alt="QR code"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <MaterialTextField
                                        label="2 FA Security Code "
                                        name="TwoFAPasscode"
                                        type="text"
                                        title="2 FA Security Code"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <Button
                                        type="submit"
                                        title="Submit"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={(e) => {
                                            formProps.setFieldValue(
                                                'Update',
                                                true
                                            );

                                            formProps.setFieldValue(
                                                'Upload',
                                                false
                                            );
                                        }}
                                        disabled={loadingOpen}
                                    >
                                        Enable Authentication
                                    </Button>
                                </Grid>
                            </Grid>
                        </Paper>
                    ) : null}
                </Grid>
            </Form>
        );
    };

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
            <Formik
                initialValues={formObj}
                enableReinitialize={true}
                onSubmit={handleSubmit}
            >
                {UploadForm}
            </Formik>

            {CustomOtpModal}
        </CustomContainer>
    );
};
