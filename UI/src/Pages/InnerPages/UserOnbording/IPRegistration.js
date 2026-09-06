import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Box,
    Button,
    Paper,
    CircularProgress
} from '@material-ui/core';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { LOGOUT } from 'ConstComponents';
import { UserOnbordingRegistrtion as REGISTRTION } from 'Components/Api/UserOnbordingAPI';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const IPRegistration = () => {
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const initialValues = {
        MobileNumber: '',
        PAN: '',
        PinCode: '',
        OTP: ''
    };

    const [FormObj, setFormObj] = useState(initialValues);

    const FormValidationSchema = Yup.object({
        MobileNumber: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'Mobile Number')
            .length(10, PLEASE_PROVIDE_VALID + 'Mobile Number'),
        PinCode: Yup.string().required(PLEASE_PROVIDE_VALID + 'Pin Code'),
        PAN: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'PAN')
            .length(10, PLEASE_PROVIDE_VALID + 'PAN'),
        OTP: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'OTP')
            .length(6, PLEASE_PROVIDE_VALID + 'OTP')
    });

    const submitHandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await REGISTRTION(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        setSubmitting(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }
    };
    const select = () => {
        setLoading(true);
        let URL = window.location.pathname;
        var res = URL.split('/');

        let primaryID = res[res.length - 1];

        setLoading(false);

        setFormObj({
            ...FormObj,
            MobileNumber: primaryID,
            PinCode: '',
            PAN: '',
            OTP: ''
        });
    };

    useEffect(() => {
        select();
    }, []);

    return (
        <CustomContainer
            isLoading={loading}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
        >
            <Formik
                enableReinitialize={true}
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submitHandle}
                resetForm={FormObj}
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>Register</h2>

                            <Paper className={classes.PaperStyle}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialTextField
                                            label="Mobile No."
                                            name="MobileNumber"
                                            type="text"
                                            length={10}
                                            readOnly={true}
                                            placeholder="Mobile Number"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialTextField
                                            label="Pin Code"
                                            name="PinCode"
                                            type="text"
                                            length={6}
                                            placeholder="Pin Code"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialTextField
                                            label="PAN"
                                            name="PAN"
                                            type="text"
                                            length={10}
                                            placeholder="PAN"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialTextField
                                            label="OTP"
                                            name="OTP"
                                            type="text"
                                            length={6}
                                            placeholder="OTP"
                                        />
                                    </Grid>
                                </Grid>
                            </Paper>
                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Register"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Register
                                        </Button>
                                        {loading && (
                                            <CircularProgress
                                                size={24}
                                                className={
                                                    classes.buttonProgress
                                                }
                                            />
                                        )}
                                    </div>
                                    <Button
                                        type="reset"
                                        variant="contained"
                                        color="secondary"
                                        title="Reset"
                                        disabled={loading}
                                        className={
                                            classes.CustomDangerButtonStyle
                                        }
                                    >
                                        Reset
                                    </Button>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            />
        </CustomContainer>
    );
};
