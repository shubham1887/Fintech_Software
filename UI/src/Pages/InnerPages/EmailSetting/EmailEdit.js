import React, { useEffect, useState } from 'react';
import { useHistory } from 'react-router';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { LOGOUT } from 'ConstComponents';
import { SelectByID as EMAIL_SELECT_BY_ID } from 'Components/Api/EmailSettingApi';
import { Update as EMAIL_UPDATE } from 'Components/Api/EmailSettingApi';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

import { Container, Grid, Paper, Button } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';

import Box from '@material-ui/core/Box';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import HOCLoader from 'Components/Loader/HOCLoader';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';

const CustomContainer = HOCLoader(Container);

export const EmailEdit = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);

    const [severity, setSeverity] = useState('success');
    const [formObject, setFormObject] = useState({
        EmailSettingMasterID: '',
        Email: '',
        Password: '',
        HostName: '',
        Port: 0
    });

    useEffect(() => {
        selectEmails();
    }, []);

    const selectEmails = async () => {
        let URL = window.location.pathname;
        var res = URL.split('/');

        let primaryID = res[res.length - 1];

        let JSONObj = {
            EmailSettingMasterID: primaryID
        };
        setLoading(true);
        var request = JSON.stringify(JSONObj);

        const response = await EMAIL_SELECT_BY_ID(request);
        setLoading(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);

        setFormObject({
            EmailSettingMasterID: data.EmailSettingMasterID,
            Email: data.Email,
            Password: data.Password,
            HostName: data.HostName,
            Port: data.Port
        });
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);
        setSubmitting(false);

        var response = await EMAIL_UPDATE(requestObj);
        setLoading(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }

        setIsError(true);
        setSubmitting(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }

        resetForm(formObject);

        setTimeout(() => {
            history.push('/EmailSetting/Index');
        }, MESSAGE_CLOSE_AFTER);
    };

    const FormValidationSchema = Yup.object({
        Email: Yup.string().required(PLEASE_PROVIDE_VALID + 'email ID'),
        Password: Yup.string().required(PLEASE_PROVIDE_VALID + 'password'),
        HostName: Yup.string().required(PLEASE_PROVIDE_VALID + 'host name'),
        Port: Yup.number()
            .moreThan(-1, PLEASE_PROVIDE_VALID + 'port number')
            .required(PLEASE_PROVIDE_VALID + 'port number')
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
                enableReinitialize={true}
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Email Setting
                            </h2>
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
                                            <MaterialTextField
                                                label="Email ID"
                                                name="Email"
                                                type="text"
                                                placeholder="Email ID"
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
                                                label="Password"
                                                name="Password"
                                                type="password"
                                                placeholder="Password"
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
                                                label="Host Name"
                                                name="HostName"
                                                type="text"
                                                placeholder="Host Name"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialNumberField
                                                label="Port"
                                                name="Port"
                                                type="text"
                                                length={3}
                                                placeholder="Port"
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

                                    <Button
                                        type="reset"
                                        variant="contained"
                                        color="secondary"
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
