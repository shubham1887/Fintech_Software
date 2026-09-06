import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';

import { Container, Grid, Box, Button, Paper } from '@material-ui/core';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { LOGOUT } from 'ConstComponents';
import { SelectURL as SELECT_URL } from 'Components/Api/ThirdPartyServiceProviderAPI';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const ThirdPartyService = (props) => {
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [CustomContainerProps, setCustomContainerProps] = useState({
        loadingOpen: false,
        isError: false,
        severity: 'success',
        responseMessage: ''
    });
    const formRef = useRef();

    const initialValues = {
        URL: '',
        DomainURL: ''
    };

    const [FormObj, setFormObj] = useState(initialValues);

    const selectURL = async () => {
        setCustomContainerProps({
            isError: false,
            loading: true
        });

        const response = await SELECT_URL();

        if (!response.isAuthorized) LOGOUT();

        setCustomContainerProps({
            isError: false,
            loading: false
        });

        if (!response.status) {
            setCustomContainerProps({
                isError: true,
                severity: 'error',
                responseMessage: response.data
            });
            return;
        }

        var data = JSON.parse(response.data);

        setFormObj({
            URL: data.URL,
            DomainURL: data.DomainURL
        });
    };

    useEffect(() => {
        selectURL();
    }, []);

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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={FormObj}
                resetForm={FormObj}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Third Party Service
                            </h2>

                            <Paper className={classes.PaperStyle}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialTextField
                                            label="URL"
                                            name="URL"
                                            type="text"
                                            placeholder="URL"
                                        />
                                    </Grid>
                                </Grid>
                            </Paper>
                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Redirect to AEPS/BBPS"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Redirect to AEPS/BBPS
                                        </Button>
                                    </div>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
