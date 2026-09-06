import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';

import { SelectCallbackLog as SELECT_APIUSER_CALLBACK_LOG } from 'Components/Api/LogAPI';
import { Container, Grid, Button, Paper } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';

import ReactTable from 'Components/ReactTable/ReactTable';

import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const APIUserCallbackLog = () => {
    const classes = commonStyle();

    const [CallbackLog, setCallbackLog] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [formObj] = useState({
        ToDate: new Date(),
        FromDate: new Date()
    });
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            FromDate: values.FromDate,
            ToDate: values.ToDate
        };

        setCallbackLog([]);

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await SELECT_APIUSER_CALLBACK_LOG(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        setSubmitting(false);
        var data = JSON.parse(response.data);
        setCallbackLog(data);
    };

    const validationSchema = Yup.object({
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'from date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'from date')
            .typeError(PLEASE_SELECT_VALID + 'from date'),

        ToDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'to date')
            .typeError(PLEASE_SELECT_VALID + 'to date')
    });

    useEffect(() => {
        createColumns();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        columns.push({
            label: 'ConsumerNumber',
            name: 'ConsumerNumber'
        });
        columns.push({
            label: 'Request',
            name: 'Request',
            textwrapwidth: 350,
            textwrap: true
        });
        columns.push({
            label: 'Response',
            name: 'Response',
            textwrapwidth: 350,
            textwrap: true
        });

        setMainColumns(columns);
    };

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
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
                render={({ values, setFieldValue, handleChange }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Callback log
                            </h2>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialDatePickerField
                                                name="FromDate"
                                                label="From Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialDatePickerField
                                                name="ToDate"
                                                label="To Date"
                                                maxDate={new Date()}
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
                                            title="submit"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Search
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
            />

            <ReactTable data={CallbackLog} columns={mainColumns} />
        </CustomContainer>
    );
};
