import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';

import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';

import { Container, Grid, Button } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import Box from '@material-ui/core/Box';
import { PLEASE_SELECT_VALID, RETAILER_CODE } from 'Components/Helper/Messages';

import { SelectForRetailerDashborad as SEARCH } from 'Components/Api/TransactionReportAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const RetailerDashboard = () => {
    const [list, setList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [openFilter, setopenFilter] = useState(true);
    const CheckUserRole = () => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            //setIsRetailerRole(true);
        }
    };

    useEffect(() => {
        CheckUserRole();
        setLoadingOpen(false);
        createColumns();
    }, []);

    var [formObj, SetFormObj] = useState({
        Date: new Date()
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;

        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        setIsError(false);

        var response = await SEARCH(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);

        setList(data);
    };

    const validationSchema = Yup.object({
        Date: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'to date')
            .typeError(PLEASE_SELECT_VALID + 'to date')
    });

    const createColumns = () => {
        var columns = [];
        var textwrapwidth = 60;

        columns.push({
            label: 'Total',
            name: 'TotalCount',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'T Amt',
            name: 'TotalAmount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'Success',
            name: 'SuccessCount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'S Amt',
            name: 'SuccessAmount',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Failed',
            name: 'FailedCount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'F Amt',
            name: 'FailedAmount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'Pending',
            name: 'PendingCount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'P Amt',
            name: 'PendingAmount',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Commission',
            name: 'RetailerCommission',
            textwrapwidth,
            numeric: true
        });
        setMainColumns(columns);
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
                enableReinitialize={true}
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
                render={({ values, setFieldValue, handleChange }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Retailer Dashboard
                            </h2>

                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Retailer Dashboard'}
                                    open={openFilter}
                                    setOpen={(value) => setopenFilter(value)}
                                >
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialDatePickerField
                                                name="Date"
                                                label="Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>
                                    </Grid>
                                </CustomAccordion>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Search"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loadingOpen}
                                        >
                                            Search
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
            />

            <h2 className={classes.PaperHead_h2}>Report</h2>
            <ReactTable data={list} columns={mainColumns} />
        </CustomContainer>
    );
};
