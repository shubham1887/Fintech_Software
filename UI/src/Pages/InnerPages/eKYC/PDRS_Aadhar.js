import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import {
    PDRSeKYCInitiate as EKYC_INITIATE,
    PDRSeKYCValidate as EKYC_VALIDATE
} from 'Components/Api/EKYCAPI';
import { SelectByUserID as SELECT_BY_USERID } from 'Components/Api/EKYCAPI';
import { TransactionResponse } from 'Components/Alert/TransactionResponse';
import { AadharDetails } from 'Pages/InnerPages/eKYC/AadharDetails';
import { Container, Grid, Paper } from '@material-ui/core';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID, E_KYC } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { OTPModal } from 'Components/Modal/OTPModal';
const CustomContainer = HOCLoader(Container);

export const PADRSAadhar = () => {
    const classes = commonStyle();
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [status, setStatus] = useState('SUCCESS');
    const [ResponseAlert, setResponseAlert] = useState({});
    const [ResponsDetails, setResponsDetails] = useState();
    const [responseMessage, setResponseMessage] = useState('');
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [eKYCMasterID, seteKYCMasterID] = useState(0);
    const [openOTP, setOpenOTP] = useState(false);
    const [openDetail, setOpenDetail] = useState(false);
    const [isError, setIsError] = useState(false);

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);
        setIsError(false);
        setLoading(true);
        const response = await EKYC_INITIATE(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        let data = JSON.parse(response.data);
        setSeverity('success');
        setResponseMessage(data.Message);
        seteKYCMasterID(data.eKYCMasterID);
        setIsError(true);
        setOpenOTP(true);

        return;
    };
    const [FormObj, setFormObj] = useState({
        Aadhaar: ''
    });
    const reset = () => {
        setFormObj({
            Aadhaar: ''
        });
    };
    const FormValidationSchema = Yup.object({
        Aadhaar: Yup.string()
            .length(12, 'Must be 12 digit')
            .required(PLEASE_PROVIDE_VALID + 'aadhaar')
    });

    const CustomOtpModal = openOTP ? (
        <OTPModal
            isOpen={openOTP}
            title="OTP"
            error={responseMessage}
            submitCallback={(value) => {
                VerifyOTP(value);
            }}
            resendCallback={(value) => {}}
            onDismissCallback={(value) => {}}
        />
    ) : null;
    const VerifyOTP = async (otpObj) => {
        setLoading(true);
        setOpenOTP(false);

        const { OTP } = otpObj;
        let RequestObj = {
            OTP: OTP,
            eKYCMasterID: eKYCMasterID
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
        reset();
        SelectDetails();
    };
    useEffect(() => {
        SelectDetails();
    }, []);
    const SelectDetails = async () => {
        let jsonObj = {
            DocumentType: 'AADHAR'
        };
        var request = JSON.stringify(jsonObj);
        setLoading(true);
        const response = await SELECT_BY_USERID(request);

        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }
        let data = JSON.parse(response.data);

        let item = JSON.parse(data.Response);

        setResponsDetails(item);
        setOpenDetail(true);
    };

    const TransferForm = ({ setFieldValue }) => {
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
                <Grid item xs={12} sm={12} md={6} lg={4}>
                    <Form>
                        <Grid item xs={12}>
                            <Paper className={classes.PaperStyle}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialNumberField
                                            label="Aadhaar"
                                            name="Aadhaar"
                                            length="12"
                                            placeholder="Aadhaar"
                                            title="Aadhaar"
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
                                        <SecondaryButton
                                            variant="contained"
                                            color="secondary"
                                            type="reset"
                                            title="Reset"
                                            onClick={() => {
                                                reset();
                                            }}
                                            className={
                                                classes.CustomDangerButtonStyle
                                            }
                                        >
                                            Reset
                                        </SecondaryButton>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <PrimaryButton
                                            type="submit"
                                            title="Generate OTP"
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
                <Grid item xs={12} sm={12} md={6} lg={8}>
                    {openDetail ? (
                        <AadharDetails request={ResponsDetails} />
                    ) : null}
                </Grid>
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
                enableReinitialize
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={handleSubmit}
                resetForm={FormObj}
            >
                {TransferForm}
            </Formik>
            {CustomOtpModal}

            {isResponseAlertOpen ? (
                <TransactionResponse
                    isOpen={isResponseAlertOpen}
                    title={status}
                    ServiceID={E_KYC}
                    request={ResponseAlert}
                    OkButtonClick={(request) => {
                        setIsResponseAlertOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
