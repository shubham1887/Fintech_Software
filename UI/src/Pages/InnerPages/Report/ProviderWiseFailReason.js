import React, { useState, useEffect, useRef } from 'react';

import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectForFilter as TRANSACTION_FILTER } from 'Components/Api/TransactionReportAPI';
import { SelectByRechargeServices as SERVICE_SELECT_RECHARGE } from 'Components/Api/ServiceApi';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';

import { Container, Grid, Button, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID, ADMIN_CODE } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';

import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);
export const ProviderWiseFailReport = () => {
    const formRef = useRef();
    const classes = commonStyle();
    var currentUsertypeid = localStorage.getItem('UserTypeID');

    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAdminRole, setIsAdminRole] = useState(false);

    const [currentPage, setCurrentPage] = useState(0);
    const [providerList, setProviderList] = useState([]);
    const [headerSummary, setHeaderSummary] = useState({});
    const [openFilter, setopenFilter] = useState(true);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [SortDirection, setSortDirection] = useState('');

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ServiceID: 0,
        OperatorID: 0,
        Status: 'ALL',
        ProviderID: 0,
        IsExcelDownload: false,
        IsRechargeReport: false,
        TopRecords: TakeCount,
        Criteria: 0,
        CriteriaText: '',
        User: {
            UserID: 0,
            FirmName: 'Select User'
        }
    });

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
    };

    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        setSortDirection
    ) => {
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            SearchText: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            Status: formRef.current.values.Status,
            UserID: formRef.current.values.User.UserID,
            ServiceID: formRef.current.values.ServiceID,
            OperatorID: formRef.current.values.OperatorID,
            ProviderID: formRef.current.values.ProviderID,
            IsExcelDownload: formRef.current.values.IsExcelDownload,
            IsRechargeReport: formRef.current.values.IsExcelDownload,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText
        };

        setIsError(false);
        setLoading(true);

        var requestJSON = JSON.stringify(requestObj);

        var response = await TRANSACTION_FILTER(requestJSON);

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
        setTransactions(data.list);
        setTotalCount(data.Count);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setHeaderSummary({});
        setTransactions([]);
        createColumns();

        await callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
    };

    const selectProvider = async () => {
        setLoading(true);
        const response = await PROVIDER_DROP_DOWN();
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
    };

    const selectServices = async () => {
        const response = await SERVICE_SELECT_RECHARGE();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        //setServices(data);
    };

    const validationSchema = Yup.object({
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.number()
                    .nullable(PLEASE_SELECT_VALID + 'User')
                    .min(0, PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            }),

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

    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
    };

    useEffect(() => {
        createColumns();
        CheckUserRole();
        selectServices();
        selectProvider();
        refreshForm();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            srno: true
        });
        columns.push({
            label: 'Provider',
            name: 'Provider'
        });

        columns.push({
            label: 'FailMessage',
            name: 'FailMessage'
        });

        columns.push({
            label: 'Amount',
            name: 'Amount',
            textwrapwidth: 70,
            numeric: true
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
                enableReinitialize={true}
                innerRef={formRef}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={ModelObj}
            >
                {() => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Provider wise fail Report'}
                                    open={openFilter}
                                    setOpen={(value) => setopenFilter(value)}
                                >
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                label="Select Provider"
                                                name="ProviderID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Provider
                                                </MenuItem>
                                                {providerList.map(
                                                    (
                                                        {
                                                            ProviderID,
                                                            ProviderName
                                                        },
                                                        index
                                                    ) => (
                                                        <MenuItem
                                                            key={index}
                                                            value={ProviderID}
                                                        >
                                                            {ProviderName}
                                                        </MenuItem>
                                                    )
                                                )}
                                            </MaterialSelect>
                                        </Grid>

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
            </Formik>

            <ReactTable
                data={transactions}
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                    callSelectData(
                        RowsPerPage,
                        RowsPerPage * currentPage,
                        '',
                        '',
                        ''
                    );
                }}
                getSearchCallback={(SearchText) => {
                    callSelectData(currentPage, TakeCount, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    callSelectData(
                        currentPage,
                        TakeCount,
                        SearchText,
                        column,
                        direction
                    );
                }}
            />
        </CustomContainer>
    );
};
