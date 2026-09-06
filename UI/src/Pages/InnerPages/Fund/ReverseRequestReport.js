import React, { useState, useEffect, useRef } from 'react';
import * as Yup from 'yup';
import { Formik, Form } from 'formik';
import { Container, Grid, Button, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import CircularProgress from '@material-ui/core/CircularProgress';
import { SelectRequestByFilter as SELECT_FILTER } from 'Components/Api/FundApi';

import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import Box from '@material-ui/core/Box';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';

import { ADMIN, RETAILER } from 'Components/Helper/UsertypeCode';
import { CustomModal } from 'Components/Modal/CustomModal';

import { ReverseAction } from 'Pages/InnerPages/Fund/ReverseAction';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { useHistory } from 'react-router';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const ReverseRequestReport = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const history = useHistory();
    const [requests, setRequests] = useState([]);
    const [openFilter, setopenFilter] = useState(true);
    const [mainColumns, setMainColumns] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [currentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );

    const [loading, setLoading] = useState(false);

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        Status: 'ALL',
        Type: currentUserTypeID === ADMIN ? 'RECEIVED' : 'RAISED',
        DOCType: 'REVERSE_REQUEST'
    });

    const [FundTransactionID, setFundTransactionID] = useState(0);
    const [isOpen, setIsOpen] = useState(false);

    const selectRequests = async (requestObj) => {
        var request = JSON.stringify(requestObj);
        setLoading(true);
        const response = await SELECT_FILTER(request);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);

        setRequests(data);
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
            label: 'Remark',
            name: 'Remark'
        });

        if (currentUserTypeID === ADMIN) {
            columns.push({
                label: 'Action',
                name: 'FundTransactionID',
                disabledOn: ['SUCCESS', 'FAILURE'],
                statusIndex: 6,
                button: true,
                callback: (value) => {
                    setIsOpen(true);
                    setFundTransactionID(value);
                }
            });
        }

        setMainColumns(columns);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setRequests([]);
        setSubmitting(true);

        selectRequests(values);
        setSubmitting(false);
    };

    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
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
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid container spacing={2}>
                                <Grid item xs={6} sm={6} md={6} lg={6}></Grid>
                                {currentUserTypeID === ADMIN ||
                                currentUserTypeID === RETAILER ? null : (
                                    <Grid
                                        item
                                        xs={6}
                                        sm={6}
                                        md={6}
                                        lg={6}
                                        style={{
                                            display: 'flex',
                                            direction: 'rtl'
                                        }}
                                    >
                                        <h2 className={classes.PaperHead_h2}>
                                            <Button
                                                variant="contained"
                                                color="primary"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={() => {
                                                    history.push(
                                                        '/Fund/ReverseRequest'
                                                    );
                                                }}
                                            >
                                                New Request
                                            </Button>
                                        </h2>
                                    </Grid>
                                )}
                            </Grid>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'Fund Reverse Requests'}
                                        open={openFilter}
                                        setOpen={(value) =>
                                            setopenFilter(value)
                                        }
                                    >
                                        <Grid container spacing={2}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
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
                                                md={2}
                                                lg={2}
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
                                                md={2}
                                                lg={2}
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
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <PrimaryButton
                                                    type="submit"
                                                    title="Search"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    disabled={loading}
                                                >
                                                    Search
                                                </PrimaryButton>
                                                {loading && (
                                                    <CircularProgress
                                                        size={24}
                                                        className={
                                                            classes.buttonProgress
                                                        }
                                                    />
                                                )}
                                            </Grid>
                                        </Grid>
                                    </CustomAccordion>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />

            {isOpen ? (
                <CustomModal
                    isOpen={isOpen}
                    title="Reverse Action"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsOpen(value);
                        refreshForm();
                    }}
                >
                    <ReverseAction
                        FundTransactionID={FundTransactionID}
                        onDismissCallback={(value) => {
                            setIsOpen(value);
                            refreshForm();
                        }}
                    />
                </CustomModal>
            ) : null}

            <ReactTable data={requests} columns={mainColumns} />
        </CustomContainer>
    );
};
