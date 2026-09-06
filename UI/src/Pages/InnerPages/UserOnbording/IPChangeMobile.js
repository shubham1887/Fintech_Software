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
import { ChangeMobileNumber as CHANGE_MOBILE_NUMBER } from 'Components/Api/UserOnbordingAPI';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const IPChangeMobile = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const initialValues = {
        OldMobileNumber: '',
        NewMobileNumber: ''
    };

    const [FormObj, setFormObj] = useState(initialValues);

    const FormValidationSchema = Yup.object({
        OldMobileNumber: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'Old Mobile Number')
            .length(10, PLEASE_PROVIDE_VALID + 'Old Mobile Number'),
        NewMobileNumber: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'New Mobile Number')
            .length(10, PLEASE_PROVIDE_VALID + 'New Mobile Number')
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
        var response = await CHANGE_MOBILE_NUMBER(requestObj);
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

        let param = {
            OldMobileNumber: values.OldMobileNumber,
            NewMobileNumber: values.NewMobileNumber
        };

        setTimeout(() => {
            history.push('/IPChangeMoblieVerify/', param);
        }, MESSAGE_CLOSE_AFTER);
    };

    return (
        <CustomContainer
            isLoading={loading}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
        >
            <label>
                <h3 className={classes.Provider_Format}>
                    Change the mobile number of the Outlet for InstantPay <br />
                </h3>
            </label>
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
                                            label="Old Aadhar Mobile No."
                                            name="OldMobileNumber"
                                            type="text"
                                            length={10}
                                            placeholder="Old Mobile Number"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialTextField
                                            label="New Aadhar Mobile No."
                                            name="NewMobileNumber"
                                            type="text"
                                            length={10}
                                            placeholder="New Mobile Number"
                                        />
                                    </Grid>
                                </Grid>
                            </Paper>
                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Submit"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Submit
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
