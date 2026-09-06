import React, { useState, useEffect, useRef } from 'react';
import * as Yup from 'yup';
import { Formik, Form } from 'formik';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import CircularProgress from '@material-ui/core/CircularProgress';
import { SelectHeaderWithRequestByFilter as SELECT_FILTER } from 'Components/Api/FundApi';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import {
    PLEASE_SELECT_VALID,
    ADMIN_CODE,
    SUPPORT_CODE
} from 'Components/Helper/Messages';
import { ADMIN, RETAILER, API_USER } from 'Components/Helper/UsertypeCode';
import { HeaderFundRequestSummary } from 'Components/SummaryCards/HeaderFundRequestSummary';

import { Action } from 'Pages/InnerPages/Fund/Action';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import { useHistory } from 'react-router';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';

import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

var type = '';
export const FundRequestIndex = () => {
    const classes = commonStyle();
    const history = useHistory();
    const formRef = useRef();
    const [openFilter, setopenFilter] = useState(true);
    const [requests, setRequests] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [totalCount, setTotalCount] = useState(0);
    const [headerSummary, setHeaderSummary] = useState({});
    const [currentUserTypeID, setCurrentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );
    const [isAdmin, setIsAdminRole] = useState(false);

    type =
        currentUserTypeID !== RETAILER && currentUserTypeID !== API_USER
            ? 'RECEIVED'
            : 'RAISED';
    const [loading, setLoading] = useState(false);

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date().addDays(-15),
        Status: 'PROCESS',
        Type: type,
        DOCType: 'TRANSFER_REQUEST'
    });

    const [FundTransactionID, setFundTransactionID] = useState(0);
    const [isOpen, setIsOpen] = useState(false);
    const selectRequests = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            Status: formRef.current.values.Status,
            Type: formRef.current.values.Type,
            DOCType: formRef.current.values.DOCType,
            IsExcelDownload: false,
            ReportFor: formRef.current.values.ReportFor
        };
        var request = JSON.stringify(requestObj);
        setLoading(true);

        const response = await SELECT_FILTER(request);

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
        if (SkipCount <= 0) {
            setHeaderSummary(data.HeaderSummary);
            setTotalCount(data.Count);
        }

        if (data.HeaderSummary != null) {
            setHeaderSummary(data.HeaderSummary);
        }
        setRequests(data.list);
    };
    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
        if (SUPPORT_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
    };
    useEffect(() => {
        CheckUserRole();
        createColumns();
        refreshForm();
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
        if (currentUserTypeID !== RETAILER && type === 'RECEIVED') {
            columns.push({
                label: 'Action',
                name: 'FundTransactionID',
                disabledOn: ['SUCCESS', 'FAILURE'],
                statusIndex: 8,
                button: true,
                callback: (value) => {
                    setIsOpen(true);
                    setFundTransactionID(value);
                }
            });
        }
        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        if (currentUserTypeID !== RETAILER) {
            columns.push({
                label: 'From',
                name: 'Receiver'
            });
        }

        columns.push({
            label: 'To',
            name: 'Sender'
        });

        columns.push({
            label: 'Wallet',
            name: 'Wallet'
        });

        columns.push({
            label: 'Mode',
            name: 'PayMode'
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
            label: 'Bank',
            name: 'BankName'
        });
        columns.push({
            label: 'UTR No.',
            name: 'UTRNumber'
        });
        columns.push({
            label: 'R.Remark',
            name: 'Remark'
        });

        columns.push({
            label: 'Transaction Date',
            name: 'TransactionDOC'
        });
        columns.push({
            label: 'Accepted/Rejected Date',
            name: 'UpdatedDOC'
        });

        columns.push({
            label: 'S.Remark',
            name: 'AdminRemark'
        });
        columns.push({
            label: 'Receipt',
            name: 'ReceiptPath',
            download: true
        });
        columns.push({
            label: 'TransactionID',
            name: 'FundTransactionID'
        });

        setMainColumns(columns);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setRequests([]);
        setSubmitting(true);
        setMainColumns([]);
        setHeaderSummary({});
        type = values.Type;
        createColumns();
        await selectRequests(TakeCount, SkipCount, '', '', '');
        //   selectRequests(values);
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
                render={() => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                {/* <Grid container spacing={2}>
                                    <Grid
                                        item
                                        xs={6}
                                        sm={6}
                                        md={6}
                                        lg={6}
                                    ></Grid>
                                    {!isAdmin ? (
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
                                            <h2
                                                className={classes.PaperHead_h2}
                                            >
                                                <Button
                                                    variant="contained"
                                                    color="primary"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    onClick={() => {
                                                        history.push(
                                                            '/Fund/Request'
                                                        );
                                                    }}
                                                >
                                                    New Request
                                                </Button>
                                            </h2>
                                        </Grid>
                                    ) : null}
                                </Grid> */}
                                <Paper>
                                    <CustomAccordion
                                        title={'Fund Requests'}
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
                                                <MaterialSelect
                                                    label="Type"
                                                    name="Type"
                                                    title="Type"
                                                >
                                                    {/* <MenuItem
                                                        key="0"
                                                        value="RECEIVED"
                                                    >
                                                        Type
                                                    </MenuItem> */}
                                                    {currentUserTypeID !==
                                                    RETAILER ? (
                                                        <MenuItem
                                                            key="RECEIVED"
                                                            value="RECEIVED"
                                                        >
                                                            RECEIVED
                                                        </MenuItem>
                                                    ) : null}
                                                    {currentUserTypeID !==
                                                        ADMIN &&
                                                    currentUserTypeID !==
                                                        SUPPORT_CODE ? (
                                                        <MenuItem
                                                            key="RAISED"
                                                            value="RAISED"
                                                        >
                                                            RAISED
                                                        </MenuItem>
                                                    ) : null}
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

                                            {!isAdmin ? (
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    <PrimaryButton
                                                        type="submit"
                                                        title="New Request"
                                                        className={
                                                            classes.CustomPrimaryButtonStyle
                                                        }
                                                        disabled={loading}
                                                        onClick={() => {
                                                            history.push(
                                                                '/Fund/Request'
                                                            );
                                                        }}
                                                    >
                                                        New Request
                                                    </PrimaryButton>
                                                </Grid>
                                            ) : null}
                                        </Grid>
                                    </CustomAccordion>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />

            {isOpen ? (
                <CustomModalWithScroll
                    isOpen={isOpen}
                    title="Action"
                    onDismissCallback={(value) => {
                        setIsOpen(value);
                        refreshForm();
                    }}
                >
                    <Action
                        FundTransactionID={FundTransactionID}
                        onDismissCallback={(value) => {
                            setIsOpen(value);
                            refreshForm();
                        }}
                    />
                </CustomModalWithScroll>
            ) : null}
            <HeaderFundRequestSummary headerFundRequest={headerSummary} />
            <ReactTable
                data={requests}
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                    selectRequests(
                        RowsPerPage,
                        RowsPerPage * currentPage,
                        '',
                        '',
                        ''
                    );
                }}
                getSearchCallback={({ SearchText }) => {
                    selectRequests(TakeCount, 0, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    selectRequests(
                        TakeCount,
                        currentPage,
                        SearchText,
                        column,
                        direction
                    );
                }}
            />
        </CustomContainer>
    );
};
