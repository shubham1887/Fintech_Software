import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { CustomModal } from 'Components/Modal/CustomModal';
import { Container, Grid, MenuItem, Paper } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID, SUPPORT_CODE } from 'Components/Helper/Messages';
import { SelectAll as SELECT } from 'Components/Api/TicketAPI';
import { ActionTicket } from 'Pages/InnerPages/Ticket/Action';
import { TicketView } from 'Pages/InnerPages/Ticket/View';
import { ADMIN } from 'Components/Helper/UsertypeCode';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const TicketReport = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isOpen, setIsOpen] = useState(false);
    const [isViewOpen, setIsViewOpen] = useState(false);
    const [TicketMasterID, setTicketMasterID] = useState(0);
    const [FirmName, setFirmName] = useState(0);
    const [openFilter, setopenFilter] = useState(true);

    const [currentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        Status: 'PROCESS'
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);

        setIsError(false);
        setLoading(true);
        createColumns();
        setTransactions([]);
        var response = await SELECT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        setSeverity('success');
        setSubmitting(false);
        var data = JSON.parse(response.data);

        setTransactions(data);
    };
    const SelectPending = async () => {
        let jsonObj = {
            ToDate: new Date(),
            FromDate: new Date(),
            Status: 'PROCESS'
        };

        var requestObj = JSON.stringify(jsonObj);

        setIsError(false);
        setLoading(true);
        createColumns();
        setTransactions([]);
        var response = await SELECT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);

        setTransactions(data);
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
        SelectPending();
    }, []);

    const createColumns = () => {
        var columns = [];

        var enableIndex = 2;
        if (currentUserTypeID === ADMIN || currentUserTypeID === SUPPORT_CODE) {
            var options = [
                {
                    Title: 'Action',
                    name: 'TicketMasterID',
                    enableOn: ['PROCESS', 'OPEN'],
                    statusIndex: enableIndex,
                    Callback: (values) => {
                        RaiseTicket(values[0], values[3]);
                    }
                }
            ];

            options.push({
                Title: 'View',
                name: 'TicketMasterID',
                enableOn: ['PROCESS', 'OPEN', 'CLOSED'],
                statusIndex: enableIndex,
                Callback: (values) => {
                    ViewTicket(values[0]);
                }
            });

            columns.splice(0, 0, {
                label: ' ',
                name: 'TicketMasterID',
                MoreMenu: true,
                options: options
            });
        }

        columns.push({
            label: '#',
            name: 'TicketMasterID'
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            hide: true
        });

        columns.push({
            label: 'DOC',
            name: 'DOC'
        });
        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });
        columns.push({
            label: 'Category',
            name: 'Category'
        });
        columns.push({
            label: 'Subject',
            name: 'Subject'
        });
        columns.push({
            label: 'Body',
            name: 'Body'
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        columns.push({
            label: 'Resolution',
            name: 'Resolution',
            ReadOnlyTextArea: true
        });

        columns.push({
            label: 'ResolutionDate',
            name: 'ResolutionDate',
            ReadOnlyTextArea: true
        });

        setMainColumns(columns);
    };

    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
    };

    const RaiseTicket = (TicketMasterID, FirmName) => {
        setTicketMasterID(TicketMasterID);
        setFirmName(FirmName);
        setIsOpen(true);
    };

    const ViewTicket = (TicketMasterID) => {
        setTicketMasterID(TicketMasterID);
        setIsViewOpen(true);
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
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'Ticket Report'}
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
                                                        key="OPEN"
                                                        value="OPEN"
                                                    >
                                                        OPEN
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="CLOSED"
                                                        value="CLOSED"
                                                    >
                                                        CLOSED
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="PROCESS"
                                                        value="PROCESS"
                                                    >
                                                        PROCESS
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
                                                    title="submit"
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

            <ReactTable
                data={transactions}
                getCallback={({ currentPage, RowsPerPage }) => {
                    // setTakeCount(RowsPerPage);
                    // setSkipCount(RowsPerPage * currentPage);
                }}
                count={50}
                columns={mainColumns}
            />
            {isOpen ? (
                <CustomModal
                    isOpen={isOpen}
                    title="Ticket"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsOpen(value);
                        refreshForm();
                    }}
                >
                    <ActionTicket
                        TicketMasterID={TicketMasterID}
                        FirmName={FirmName}
                        onDismissCallback={(value) => {
                            setIsOpen(value);
                            refreshForm();
                        }}
                    />
                </CustomModal>
            ) : null}
            {isViewOpen ? (
                <CustomModal
                    isOpen={isViewOpen}
                    title="Ticket"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsViewOpen(value);
                        refreshForm();
                    }}
                >
                    <TicketView
                        TicketMasterID={TicketMasterID}
                        onDismissCallback={(value) => {
                            setIsViewOpen(value);
                            refreshForm();
                        }}
                    />
                </CustomModal>
            ) : null}
        </CustomContainer>
    );
};
