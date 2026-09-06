import React, { useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Box,
    Button,
    Paper,
    CircularProgress
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';

import { Testsms as TEST_SMS } from 'Components/Api/CheckAPI';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
const CustomContainer = HOCLoader(Container);

export const CheckSMS = ({ onDismissCallback }) => {
    const classes = commonStyle();

    const [loadingOpen, setLoadingOpen] = useState(false);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [formObject, setFormObject] = useState({
        MobileNumber: '',
        Message: '',
        ResponseMessage: ''
    });
    const FormValidationSchema = Yup.object({
        MobileNumber: Yup.string()
            .length(10, 'Must be 10 digit')
            .required(PLEASE_PROVIDE_VALID + 'number'),
        Message: Yup.string().required(PLEASE_PROVIDE_VALID + 'message')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);
        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await TEST_SMS(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        setSubmitting(true);

        setFormObject({
            ...formObject,
            MobileNumber: values.MobileNumber,
            Message: values.Message,
            ResponseMessage: response.data
        });
    };
    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={formObject.ResponseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Formik
                enableReinitialize={true}
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
            >
                {() => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialNumberField
                                                label="Mobile Number"
                                                name="MobileNumber"
                                                type="text"
                                                length={10}
                                                placeholder="Mobile Number"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialTextField
                                                label="Test Message"
                                                name="Message"
                                                type="text"
                                                placeholder="Test Message"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <MaterialTextField
                                                label="Response"
                                                name="ResponseMessage"
                                                type="text"
                                                placeholder="Response"
                                                length="1000"
                                                rows={5}
                                            />
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Test"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loadingOpen}
                                        >
                                            Test
                                        </Button>
                                        {loadingOpen && (
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
                                        title="Reset"
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
