import React, { useState } from 'react';
import { Formik, Form } from 'formik';

import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { LOGOUT } from 'ConstComponents';
import { InitialChangePassword as INITIAL_PASSWORD_UPDATE } from 'Components/Api/UsersApi';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import CustomeAlert from 'Components/Alert/Alert';
import { Container, Grid, Button } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';

export const InitialChangePassword = ({
    userName,
    setChangepasswordOpen,
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

        var response = await INITIAL_PASSWORD_UPDATE(requestObj);
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
            setChangepasswordOpen(false);
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
                            <Grid item xs={12}>
                                <Grid container spacing={1}>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            label="Old Password"
                                            name="OldPassword"
                                            type="password"
                                            placeholder="Old Password"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            label="New Password"
                                            name="NewPassword"
                                            type="password"
                                            placeholder="New Password"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            label="Confirm Password"
                                            name="ConfirmPassword"
                                            type="password"
                                            placeholder="Confirm Password"
                                        />
                                    </Grid>
                                </Grid>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Update"
                                            color="primary"
                                            variant="contained"
                                            className="loginbutton"
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
