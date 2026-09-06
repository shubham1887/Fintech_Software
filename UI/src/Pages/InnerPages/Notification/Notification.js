import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { Insert as NOTIFICATION_ADD } from 'Components/Api/NotificationApi';
import { Select as NOTIFICATION_SELECT } from 'Components/Api/NotificationApi';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';

import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, Grid, Paper, Button } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import Box from '@material-ui/core/Box';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const NotificationAdd = () => {
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    useEffect(() => {
        setLoadingOpen(true);
        select();
    }, []);

    var formObj = {
        NotificationID: 0,
        NotificationText: ''
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            return;
        }
        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);
        setIsError(false);
        var response = await NOTIFICATION_ADD(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        setResponseMessage(response.data);
        setIsError(true);
        setSubmitting(false);

        setSeverity('success');
        if (!response.status) {
            setSeverity('error');
            return;
        }
        select();
    };

    const formValidationSchema = Yup.object({
        NotificationText: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'notification'
        )
    });

    const select = async () => {
        const response = await NOTIFICATION_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }
        var data = JSON.parse(response.data);
        formObj.NotificationID = data.NotificationID;
        formObj.NotificationText = data.NotificationText;
        setLoadingOpen(false);
    };

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Formik
                initialValues={formObj}
                validationSchema={formValidationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
                render={(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Notification
                            </h2>

                            <Grid item xs={6}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={1}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <MaterialTextField
                                                label="Notification Message"
                                                name="NotificationText"
                                                type="text"
                                                placeholder="Notification Message"
                                            />
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                            <Grid item xs={6}>
                                <Grid className={classes.footerGrid}>
                                    <Box className={classes.flexReverse}>
                                        <div className={classes.SubmitWrapper}>
                                            <Button
                                                type="submit"
                                                title="Add"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                Update
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
                                    </Box>
                                </Grid>
                            </Grid>
                        </Form>
                    );
                }}
            />
        </CustomContainer>
    );
};
