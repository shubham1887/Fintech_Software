import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import {
    PaysprinteKYCInitiate as EKYC_INITIATE,
    PaysprinteKYCValidate as EKYC_VALIDATE
} from 'Components/Api/EKYCAPI';
import { TransactionResponse } from 'Components/Alert/TransactionResponse';
import { SelecteKYCUserDetailsByUserID as SELECT_BY_USERID } from 'Components/Api/UsersApi';
import { Container, Grid, Paper } from '@material-ui/core';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import Divider from '@material-ui/core/Divider';
import { PLEASE_PROVIDE_VALID, E_KYC } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { OTPModal } from 'Components/Modal/OTPModal';
const CustomContainer = HOCLoader(Container);

const PaysprinteKYC = () => {
    const classes = commonStyle();
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [status, setStatus] = useState('SUCCESS');
    const [ResponseAlert, setResponseAlert] = useState({});
    const [responseMessage, setResponseMessage] = useState('');
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [eKYCMasterID, seteKYCMasterID] = useState(0);
    const [InWords, setInWords] = useState('');
    const [openOTP, setOpenOTP] = useState(false);
    const [isError, setIsError] = useState(false);
    const [eKYCReport, seteKYCReport] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);

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
        Pan: '',
        Aadhaar: ''
    });
    const reset = () => {
        setFormObj({
            Pan: '',
            Aadhaar: ''
        });
    };
    const FormValidationSchema = Yup.object({
        Aadhaar: Yup.string()
            .length(12, 'Must be 12 digit')
            .required(PLEASE_PROVIDE_VALID + ' aadhaar'),
        Pan: Yup.string()
            .length(10, 'Must be 10 alpha numberic')
            .required(PLEASE_PROVIDE_VALID + ' pan')
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
        createColumns();
        let data = JSON.parse(response.data);
        seteKYCReport(data);
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: 'Name',
            name: 'PersonName'
        });
        columns.push({
            label: 'CityName',
            name: 'CityName'
        });
        columns.push({
            label: 'Address',
            name: 'Address'
        });
        columns.push({
            label: 'Pincode',
            name: 'Pincode'
        });

        columns.push({
            label: 'Image',
            name: 'ProfileImagePath',
            Image: true,
            deactived: true
        });

        setMainColumns(columns);
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
                        <h2 className={classes.PaperHead_h2}>e-KYC</h2>
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
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            label="Pan Number"
                                            name="Pan"
                                            length="10"
                                            placeholder="Pan Number"
                                            title="Pan Number"
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
                <Grid
                    item
                    xs={12}
                    sm={12}
                    md={6}
                    lg={8}
                    className="lastrecharges"
                >
                    <h2 className={classes.PaperHead_h2}>eKYC Details</h2>
                    <ReactTable
                        data={eKYCReport}
                        columns={mainColumns}
                        headerToolbar={false}
                        footerToolbar={false}
                    />
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

export default PaysprinteKYC;
