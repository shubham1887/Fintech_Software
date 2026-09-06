import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import { LOGOUT } from 'ConstComponents';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import {
    YesBankEkycInitiate as EKYC_INITIATE,
    YesBankEkycValidate as EKYC_VALIDATE
} from 'Components/Api/EKYCAPI';
import { TransactionResponse } from 'Components/Alert/TransactionResponse';
import { SelectProfile as SELECT_BY_USERID } from 'Components/Api/UsersApi';
import { Box, Container, Grid, Paper, Typography } from '@material-ui/core';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { E_KYC } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { OTPModal } from 'Components/Modal/OTPModal';
import { useHistory } from 'react-router';
const CustomContainer = HOCLoader(Container);

const Yesbank_eKYC = () => {
    const formRef = useRef();
    const history = useHistory();

    const classes = commonStyle();
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [status, setStatus] = useState('SUCCESS');
    const [ResponseAlert, setResponseAlert] = useState({});
    const [responseMessage, setResponseMessage] = useState('');
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [eKYCMasterID, seteKYCMasterID] = useState(0);
    const [openOTP, setOpenOTP] = useState(false);
    const [isError, setIsError] = useState(false);
    const [IsKYCApproved, setIsKYCApproved] = useState(false);

    const isEditable = 'no';
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        setIsError(false);
        setLoading(true);
        const response = await EKYC_INITIATE();
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            let data = JSON.parse(response.data);
            setSeverity('error');
            setResponseMessage(data.Message);
            setIsError(true);
            setOpenOTP(false);
            return;
        }
        let data = JSON.parse(response.data);
        setSeverity('success');
        seteKYCMasterID(data.eKYCMasterID);

        setIsError(true);
        setOpenOTP(true);
        return;
    };
    const [FormObj, setFormObj] = useState({
        MobileNumber: '',
        EmailID: '',
        PanNumber: '',


    });

    const CustomOtpModal = openOTP && (
        <OTPModal
            isOpen={openOTP}
            title="Confirm with OTP"

            submitCallback={(value) => {
                VerifyOTP(value);
            }}
            resendCallback={(value) => { }}
            onDismissCallback={(value) => { }}
        />
    );
    const VerifyOTP = async (otpObj) => {
        setLoading(true);
        setOpenOTP(false);

        const { OTP } = otpObj;
        let RequestObj = {
            OTP: OTP,
            eKYCMasterID: eKYCMasterID,
            PanNumber: formRef.current.values.PanNumber
        };
        var response = await EKYC_VALIDATE(JSON.stringify(RequestObj));
        setLoading(false);
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            setOpenOTP(true);
            return;
        }
        let data = JSON.parse(response.data);
        setSeverity('success');
        setIsError(true);
        setOpenOTP(false);
        setResponseAlert(data);
        setIsResponseAlertOpen(true);
        history.push('/Yesbank_eKYC_BiomatricVerifcation');
    };
    useEffect(() => {
        SelectReport();
    }, []);

    const SelectReport = async () => {
        setLoading(true);
        const response = await SELECT_BY_USERID();
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        let data = JSON.parse(response.data);
        setFormObj(data);
        setIsKYCApproved(data.IsKYCApproved);

    };
    const TransferForm = ({ setFieldValue }) => {
        return (
            <Grid
                item
                container
                style={{ justifyContent: 'center' }}
            >
                {!IsKYCApproved && ( 
                <Grid item xs={12} sm={12} md={6} lg={4} >
                    <Form>
                        <h2 className={classes.PaperHead_h2}>Agent KYC Update</h2>
                        <Grid item xs={12}>
                            <Paper className={classes.PaperStyle}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialNumberField
                                            label="Agent ID"
                                            name="AgentID"
                                            length="12"
                                            placeholder="AgentID"
                                            title="AgentID"
                                                value="AG001001"
                                            readOnly={isEditable}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            label="Mobile Number"
                                            name="MobileNumber"
                                            length="10"
                                            placeholder="Mobile Number"
                                            title="Mobile Number"
                                            readOnly={isEditable}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            label="Email ID"
                                            name="EmailID"
                                            length="10"
                                            placeholder="Email ID"
                                            title="Email ID"
                                            readOnly={isEditable}
                                        />
                                    </Grid>
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={12}
                                    lg={12}
                                    spacing={2}
                                    container
                                    className={classes.footerGrid}
                                >
                                    <Grid item xs={12} sm={12} md={6} lg={6}>

                                    </Grid>

                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <PrimaryButton
                                            type="submit"
                                            title="Send OTP"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Generate OTP
                                        </PrimaryButton>
                                    </Grid>
                                </Grid>
                            </Paper>
                        </Grid>
                    </Form>
                </Grid>
                )}
                {IsKYCApproved && (
                    <Box className={classes.successBox}>
                        <Typography variant="body1">
                            <i className={`material-icons ${classes.successIcon}`}>check_circle</i>
                            Ekyc Already Done
                        </Typography>
                    </Box>
                )} 
            </Grid>
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
                innerRef={formRef}
                enableReinitialize
                initialValues={FormObj}

                onSubmit={handleSubmit}
                resetForm={FormObj}
            >
                {TransferForm}
            </Formik>
            {CustomOtpModal}

            {isResponseAlertOpen && (
                <TransactionResponse
                    isOpen={isResponseAlertOpen}
                    title={status}
                    ServiceID={E_KYC}
                    request={ResponseAlert}
                    OkButtonClick={(request) => {
                        setIsResponseAlertOpen(false);
                    }}
                />
            )}
        </CustomContainer>
    );
};

export default Yesbank_eKYC;
