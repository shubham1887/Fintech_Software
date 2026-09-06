import React, { useState } from 'react';
import { Formik, Form } from 'formik';

import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { LOGOUT } from 'ConstComponents';
import { ChangePasswordAfterExpiry as PASSWORD_UPDATE } from 'Components/Api/UsersApi';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import CustomeAlert from 'Components/Alert/Alert';
import { Container, Grid, Button, Typography } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';

export const ChangePasswordAfterExpiry = ({
    userName,
    setChangePasswordAfterExpiryOpen,
    onDismissCallback
}) => {
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');

    var FormObj = {
        OldPassword: '',
        NewPassword: '',
        ConfirmPassword: '',
        MobileNumber: userName
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);

        var response = await PASSWORD_UPDATE(requestObj);
        if (!response.isAuthorized) {
            LOGOUT();
        }

        setIsError(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setLoading(false);
            setSeverity('error');
            return;
        }
        resetForm(FormObj);

        setTimeout(() => {
            onDismissCallback(false);
            setChangePasswordAfterExpiryOpen(false);
        }, MESSAGE_CLOSE_AFTER);
    };

    const FormValidationSchema = Yup.object({
        OldPassword: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'old password'
        ),
        NewPassword: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'new password'
        ),
        ConfirmPassword: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'confirm password'
        )
    });

    return (
        <Container>
            {isError ? (
                <div>
                    <CustomeAlert
                        alertMessage={responseMessage}
                        isOpen={isError}
                        severity={severity}
                        onCloseCallback={(AlertState) => {
                            setIsError(AlertState);
                        }}
                    />
                </div>
            ) : null}

            <Formik
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <Grid item xs={12} sm={12} md={6} lg={6}>
                                <Typography>
                                    <span style={{ color: 'red' }}>
                                        <b>
                                            {
                                                'Your password is expired, Please reset password.'
                                            }
                                        </b>
                                    </span>
                                </Typography>
                            </Grid>

                            <Grid item xs={12}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <MaterialTextField
                                            label="MobileNumber"
                                            name="MobileNumber"
                                            type="text"
                                            readOnly={true}
                                            title="MobileNumber"
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <MaterialTextField
                                            label="Old Password"
                                            name="OldPassword"
                                            type="password"
                                            placeholder="Old Password"
                                        />
                                    </Grid>
                                </Grid>
                            </Grid>
                            <Grid item xs={12}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <MaterialTextField
                                            label="New Password"
                                            name="NewPassword"
                                            type="password"
                                            placeholder="New Password"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <MaterialTextField
                                            label="Confirm Password"
                                            name="ConfirmPassword"
                                            type="password"
                                            placeholder="Confirm Password"
                                        />
                                    </Grid>
                                </Grid>
                            </Grid>

                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <Typography>
                                    <span style={{ color: 'green' }}>
                                        <b>
                                            {
                                                'Note: Password must contains 1 capital alphabet, 1 small alphabet,1 digit and 1 special character (minimum length 8 char.)'
                                            }
                                        </b>
                                    </span>
                                </Typography>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Update"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Update
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
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </Container>
    );
};
