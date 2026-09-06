import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { LOGOUT } from 'ConstComponents';
import {
    UpdatePassword as USER_PASSWORD_UPDATE,
    SelectResponseMessage as Select
} from 'Components/Api/UsersApi';
import { UpdatePin as USER_PIN_UPDATE } from 'Components/Api/UsersApi';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { Container, Grid, Paper, Button, Typography } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const UpdatePassword = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [PasswordExpiryMessage, setPasswordExpiryMessage] = useState('');

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);

        var response = await USER_PASSWORD_UPDATE(requestObj);
        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setLoading(false);
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }

        setResponseMessage(response.data);
        setSeverity('success');
        setIsError(true);

        setSubmitting(false);
        resetForm(FormObj);

        setTimeout(() => {
            history.push('/Login');
        }, MESSAGE_CLOSE_AFTER);
    };

    const submithandleMPin = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);

        var response = await USER_PIN_UPDATE(requestObj);
        setLoading(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setLoading(false);
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }

        setResponseMessage(response.data);
        setSeverity('success');
        setIsError(true);

        setSubmitting(false);
        resetForm(FormObj);

        setTimeout(() => {}, MESSAGE_CLOSE_AFTER);
    };

    useEffect(() => {
        selectPasswordExpiry();
    }, []);

    const selectPasswordExpiry = async () => {
        const response = await Select();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setPasswordExpiryMessage(data);
    };

    var FormObj = {
        OldPassword: '',
        NewPassword: '',
        ConfirmPassword: ''
    };
    var FormMPinObj = {
        NewPin: '',
        ConfirmPin: '',
        CurrentPassword: ''
    };
    const FormValidationSchema = Yup.object({
        OldPassword: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'old password'
        ),

        NewPassword: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'New password')
            .min(6, 'Minimum password length must be 6 character')
            .max(25, 'Maximum password length must be 25 character'),

        ConfirmPassword: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'Confirm Password')
            .oneOf([Yup.ref('NewPassword'), null], 'Passwords must match')
    });
    const FormValidationSchemaMPin = Yup.object({
        NewPin: Yup.string()
            .length(4, 'Must be 4 digit')

            .required(PLEASE_PROVIDE_VALID + 'new pin'),
        ConfirmPin: Yup.string()
            .length(4, 'Must be 4 digit')
            .required(PLEASE_PROVIDE_VALID + 'confirm pin'),
        CurrentPassword: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'current password')
            .min(6, 'Minimum password length must be 6 character')
    });

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
                initialValues={FormMPinObj}
                validationSchema={FormValidationSchemaMPin}
                onSubmit={submithandleMPin}
                resetForm={FormMPinObj}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>Change Pin</h2>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="Login Password"
                                                name="CurrentPassword"
                                                type="password"
                                                placeholder="Login Password"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialNumberField
                                                label="New Pin"
                                                name="NewPin"
                                                type="password"
                                                length={4}
                                                placeholder="New Pin"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialNumberField
                                                label="Confirm Pin"
                                                name="ConfirmPin"
                                                type="password"
                                                length={4}
                                                placeholder="Confirm Pin"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Update Pin"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loading}
                                            >
                                                Update
                                            </PrimaryButton>
                                            {loading && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <SecondaryButton
                                                type="reset"
                                                variant="contained"
                                                color="secondary"
                                                title="Reset"
                                                className={
                                                    classes.CustomDangerButtonStyle
                                                }
                                            >
                                                Reset
                                            </SecondaryButton>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>

            <Formik
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Change password
                            </h2>
                            <Grid item xs={12} sm={12} md={6} lg={6}>
                                <Typography>
                                    <span style={{ color: 'green' }}>
                                        <b>{PasswordExpiryMessage}</b>
                                    </span>
                                </Typography>
                            </Grid>

                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="Old Password"
                                                name="OldPassword"
                                                type="password"
                                                placeholder="Old Password"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="New Password"
                                                type="password"
                                                placeholder="New Password"
                                                name="NewPassword"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="Confirm Password"
                                                name="ConfirmPassword"
                                                type="password"
                                                placeholder="Confirm Password"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Update Password"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loading}
                                            >
                                                Update
                                            </PrimaryButton>
                                            {loading && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <SecondaryButton
                                                type="reset"
                                                variant="contained"
                                                color="secondary"
                                                title="Reset"
                                                className={
                                                    classes.CustomDangerButtonStyle
                                                }
                                            >
                                                Reset
                                            </SecondaryButton>
                                        </Grid>
                                        {/* <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <div style={{ color: 'green' }}>
                                                {responseMessage}
                                            </div>
                                        </Grid> */}
                                    </Grid>
                                </Paper>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div
                                        className={classes.SubmitWrapper}
                                    ></div>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
