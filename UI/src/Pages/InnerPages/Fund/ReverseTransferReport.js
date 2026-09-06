import React, { useState, useEffect, useRef } from 'react';
import * as Yup from 'yup';
import { Formik, Form } from 'formik';
import { Container, Grid, Button, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import CircularProgress from '@material-ui/core/CircularProgress';
import { ReverseReport as REVERSE_REPORT } from 'Components/Api/FundApi';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import Box from '@material-ui/core/Box';
import Accordion from '@material-ui/core/Accordion';

import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import {
    RETAILER,
    API_USER,
    DISTRIBUTOR,
    SUPER_DISTRIBUTOR
} from 'Components/Helper/UsertypeCode';
import HOCLoader from 'Components/Loader/HOCLoader';

import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const ReverseTransferReport = () => {
    const classes = commonStyle();
    const formRef = useRef();

    const [list, setList] = useState([]);

    const [mainColumns, setMainColumns] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [TakeCount, setTakeCount] = useState(100);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(false);
    const [currentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );
    const [openFilter, setopenFilter] = useState(true);

    const handlePanelChange = (panel) => (event, newExpanded) => {
        setopenFilter(newExpanded ? panel : false);
    };

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        Status: 'ALL',
        Type:
            currentUserTypeID === RETAILER || currentUserTypeID === API_USER
                ? 'TRANSFER'
                : 'RECEIVED',
        TakeCount: TakeCount,
        SkipCount: SkipCount
    });

    const selectReport = async (requestObj) => {
        var request = JSON.stringify(requestObj);
        setLoading(true);
        const response = await REVERSE_REPORT(request);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        setList(data.data);
    };

    useEffect(() => {
        createColumns();
    }, []);

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

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'FundTransactionID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        if (
            currentUserTypeID === DISTRIBUTOR ||
            currentUserTypeID === SUPER_DISTRIBUTOR
        )
            columns.push({
                label: 'From',
                name: 'Sender'
            });

        columns.push({
            label: 'To',
            name: 'Receiver'
        });

        columns.push({
            label: 'Wallet',
            name: 'Wallet'
        });

        columns.push({
            label: 'Amount',
            name: 'Amount',
            numeric: true
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        columns.push({
            label: 'Type',
            name: 'Through'
        });

        columns.push({
            label: 'Remark',
            name: 'Remark'
        });

        setMainColumns(columns);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setList([]);
        setSubmitting(true);
        selectReport(values);
        setSubmitting(false);
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
                innerRef={formRef}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={ModelObj}
                render={({ values, setFieldValue, handleChange }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'Reverse Report'}
                                        open={openFilter}
                                        setOpen={(value) =>
                                            setopenFilter(value)
                                        }
                                    >
                                        <Grid container spacing={4}>
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
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialSelect
                                                    label="Status"
                                                    name="Status"
                                                    title="Status"
                                                >
                                                    <MenuItem
                                                        key="ALL"
                                                        value="ALL"
                                                    >
                                                        All Status
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="PROCESS"
                                                        value="PROCESS"
                                                    >
                                                        PROCESS
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="SUCCESS"
                                                        value="SUCCESS"
                                                    >
                                                        SUCCESS
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="FAILURE"
                                                        value="FAILURE"
                                                    >
                                                        FAILURE
                                                    </MenuItem>
                                                </MaterialSelect>
                                            </Grid>
                                        </Grid>
                                    </CustomAccordion>
                                </Paper>
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

            <ReactTable
                data={list}
                columns={mainColumns}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                    // callSelectData(RowsPerPage, RowsPerPage * currentPage);
                }}
                count={totalCount}
                rowsPerPage={100}
            />
        </CustomContainer>
    );
};
