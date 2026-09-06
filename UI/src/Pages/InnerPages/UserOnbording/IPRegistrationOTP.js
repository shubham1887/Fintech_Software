import React, { useState, useEffect, useRef } from 'react';
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
import { UserOnbordingRegistrtionOTP as REGISTRTIONOTP } from 'Components/Api/UserOnbordingAPI';
import { SelectOutletID as SELECT_OUTLETID } from 'Components/Api/UserOnbordingAPI';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const IPRegistrationOTP = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [loadingOpen, setLoadingOpen] = useState(false);
    const formRef = useRef();

    const initialValues = {
        MobileNumber: ''
    };

    const [FormObj, setFormObj] = useState(initialValues);

    const FormValidationSchema = Yup.object({
        MobileNumber: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'Mobile Number')
            .length(10, PLEASE_PROVIDE_VALID + 'Mobile Number')
    });

    const selectData = async () => {
        const response = await SELECT_OUTLETID();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);

        //history.push('/QRCodeGenerate');
    };

    useEffect(() => {
        setLoadingOpen(true);
        selectData();
    }, []);
    const submitHandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await REGISTRTIONOTP(requestObj);
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
        setTimeout(() => {
            history.push('/IPRegistration/' + values.MobileNumber);
        }, MESSAGE_CLOSE_AFTER);
    };

    return (
        <CustomContainer
            isLoading={loading}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
        >
            <Formik
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submitHandle}
                resetForm={FormObj}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Generate OTP
                            </h2>

                            <Paper className={classes.PaperStyle}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialTextField
                                            label="Mobile No."
                                            name="MobileNumber"
                                            type="text"
                                            length={20}
                                            placeholder="Mobile Number"
                                        />
                                    </Grid>
                                </Grid>
                            </Paper>
                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Generate OTP"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Generate OTP
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
            </Formik>
        </CustomContainer>
    );
};
