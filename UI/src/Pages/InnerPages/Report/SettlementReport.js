import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectForFilter as TRANSACTION_DETAIL_FILTER } from 'Components/Api/TransactionDetailApi';
import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
import { Container, Grid, MenuItem, Paper } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import excel from 'Assets/Images/excel.svg';
import Icon from '@material-ui/core/Icon';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_SELECT_VALID,
    ADMIN_CODE,
    RETAILER_CODE,
    API_USER_CODE,
    SUPPORT_CODE
} from 'Components/Helper/Messages';
import { FETCH_LIST } from 'Config';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { ExportTypeItems } from 'Components/ReportHelper';
import HOCLoader from 'Components/Loader/HOCLoader';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const SettlementReport = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [openFilter, setopenFilter] = useState(true);
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [IsFetchAllList, setisFetchAllList] = useState(false);
    const [userList, setUserList] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [SortDirection, setSortDirection] = useState('');
    const [SearchText, setSearchText] = useState('');
    const [currentPage, setCurrentPage] = useState(0);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [wallets, setWallets] = useState([]);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState(false);
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select Own'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ReportBy: 'Inline',
        ExportType: 'Select Download',
        IsPDFDownload: false,
        IsExcelDownload: false,
        WalletID: 0,
        Criteria: 0,
        CriteriaText: '',
        Type: 'All Type',
        User: {
            UserID: 0,
            FirmName: 'Select Own'
        }
    });

    const fetchUserList = async () => {
        let listValue = await FETCH_LIST();

        if (listValue === undefined || listValue === null || listValue === '') {
            return;
        }

        if (listValue !== undefined && listValue === 'All') {
            setisFetchAllList(true);
            SelectUser('');
            return;
        }
    };

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) {
            setIsAdminRole(true);
        }
        if (SUPPORT_CODE === parseInt(currentUsertypeid)) {
            setIsAdminRole(true);
        }
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
        }
        if (API_USER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
        }

        createColumns();
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        //setTransactions([]);
        createColumns();

        await callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
    };

    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        setSortDirection
    ) => {
        let inputObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            SearchText: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            WalletID: formRef.current.values.WalletID,
            ReportBy: formRef.current.values.ReportBy,
            Type: formRef.current.values.Type,
            UserID: formRef.current.values.User.UserID,
            ExportType: formRef.current.values.ExportType,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText,
            IsPDFDownload: formRef.current.values.IsPDFDownload,
            IsExcelDownload: formRef.current.values.IsExcelDownload
        };

        if (formRef.current.values.IsExcelDownload === false) setTransactions();
        //setTransactions([]);

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await TRANSACTION_DETAIL_FILTER(requestObj);
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
        if (data.IsExcelDownload || data.IsPDFDownload) {
            downloadFile(data.ReportPath, 'Account Statement Report');
            return;
        }
        setTransactions([]);
        setTransactions(data.list);
        setTotalCount(data.Count);
    };

    const callSearch = async (text) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            text.length <= 0 ||
            text === userDefault.FirmName
        )
            return;

        var jsonObj = {
            FirmName: text
        };
        SelectUser(jsonObj);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
        document.removeChild(a);
    }
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

    const selectWallets = async () => {
        var request = {
            ID: 0
        };
        var json = JSON.stringify(request);
        const response = await WALLET_SELECT(json);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setWallets(data);
    };

    const SelectUser = async (values) => {
        setLoading(true);
        var request = JSON.stringify(values);
        setLoading(false);
        const userResponse = await USER_SELECT_FOR_DROPDOWN(request);

        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(userResponse.data);

        let objWithSelectAll = [userDefault, ...data];

        setUserList(objWithSelectAll);
    };
    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };
    const TextDisplay = (setFieldValue) => {
        setFieldValue('CriteriaText', '');
        setIsDisplayCriteria(true);
    };
    const OnChangeCriteria = (value) => {
        if (value === undefined || value <= 0) {
            setIsDisplayCriteria(false);
            setCriteriaTextLabel('');
            return;
        }
        setCriteriaTextLabel(value);
        setIsDisplayCriteria(true);
    };
    useEffect(() => {
        fetchUserList();
        CheckUserRole();
        selectWallets();
        refreshForm();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'TransactionDetailID',
            srno: true
        });

        columns.push({
            label: 'Date & Time',
            name: 'DOC'
        });

        columns.push({
            label: 'Entity Identifier',
            name: 'AliasName'
        });

        columns.push({
            label: 'Entity Name',
            name: 'PersonName'
        });

        columns.push({
            label: 'Product Name',
            name: 'DOCType'
        });

        columns.push({
            label: 'Service Name',
            name: 'OperatorName'
        });

        columns.push({
            label: 'Type',
            name: 'AmountCreditDebit',
            customStatus: true
        });

        columns.push({
            label: 'Txn ID',
            name: 'ReferenceNo'
        });

        columns.push({
            label: 'Reference ID',
            name: 'ReferenceID'
        });

        columns.push({
            label: 'Transaction Amount (₹)',
            name: 'Amount',
            numeric: true,
        });

        columns.push({
            label: 'Charge (₹)',
            name: 'Charge',
            numeric: true,
        });

        columns.push({
            label: 'GST',
            name: 'GST',
            numeric: true,
        });

        columns.push({
            label: 'Commission',
            name: 'DiscountAmount',
            numeric: true
        });

        columns.push({
            label: 'TDS',
            name: 'TDS',
            numeric: true,
        });

        columns.push({
            label: 'Net Commission',
            name: 'NetCommission',
            numeric: true,
        });

        columns.push({
            label: 'Opening Balance(₹)',
            name: 'OpeningBalance',
            numeric: true
        });

        columns.push({
            label: 'Closing Balance(₹)',
            name: 'ClosingBalance',
            numeric: true
        });

        columns.push({
            label: 'Status',
            name: 'TransactionDescription',
            render: (data, type, row) => {
                let color = '';
                if (data.TransactionDescription === 'SUCCESS') {
                    color = 'green';
                } else if (data === 'Failure') {
                    color = 'red';
                } else if (data === 'Process') {
                    color = 'orange';
                }

                return (
                    <span style={{ color: color }}>
                        {data}
                    </span>
                );
            }
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
                innerRef={formRef}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={ModelObj}
                render={({
                    values,
                    setFieldValue,
                    handleChange,
                    handleSubmit
                }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'Settlement Report'}
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
                                                    label="Wallet"
                                                    name="WalletID"
                                                    title="Select Wallet"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        All Wallet
                                                    </MenuItem>
                                                    {wallets.map((item) => (
                                                        <MenuItem
                                                            key={item.WalletID}
                                                            value={
                                                                item.WalletID
                                                            }
                                                        >
                                                            {item.Name}
                                                        </MenuItem>
                                                    ))}
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
                                                    <MenuItem
                                                        key="All Type"
                                                        value="All Type"
                                                    >
                                                        All Type
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="FUND RECEIVE"
                                                        value="FUND RECEIVE"
                                                    >
                                                        FUND RECEIVE
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="FUND TRANSFER"
                                                        value="FUND TRANSFER"
                                                    >
                                                        FUND TRANSFER
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="RECHARGE"
                                                        value="RECHARGE"
                                                    >
                                                        RECHARGE
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="DMT"
                                                        value="DMT"
                                                    >
                                                        DMT
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="Payout"
                                                        value="Payout"
                                                    >
                                                        PAYOUT
                                                    </MenuItem>
                                                </MaterialSelect>
                                            </Grid>

                                            {!isRetailerRole ? (
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    <Field
                                                        component={
                                                            AsyncAutoComplete
                                                        }
                                                        defaultValue={
                                                            userDefault
                                                        }
                                                        FieldName="User"
                                                        name="User"
                                                        SubFieldName="UserID"
                                                        options={userList}
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.FirmName}
                                                        textFieldProps={{
                                                            label: 'Select User'
                                                        }}
                                                        onSearchCallback={(
                                                            text
                                                        ) => {
                                                            callSearch(text);
                                                        }}
                                                        onCustomCallback={(
                                                            name,
                                                            value
                                                        ) => {
                                                            setUserDefault({
                                                                UserID:
                                                                    value.UserID,
                                                                FirmName:
                                                                    value.FirmName
                                                            });
                                                        }}
                                                    />
                                                </Grid>
                                            ) : null}

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialSelect
                                                    label="Selection Criteria"
                                                    name="Criteria"
                                                    title="criteria"
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        OnChangeCriteria(value);
                                                    }}
                                                >
                                                    <MenuItem key={0} value={0}>
                                                        Selection Criteria
                                                    </MenuItem>
                                                    <MenuItem
                                                        key={1}
                                                        value={'TransactionID'}
                                                        onClick={() => {
                                                            TextDisplay(
                                                                setFieldValue
                                                            );
                                                        }}
                                                    >
                                                        TransactionID
                                                    </MenuItem>
                                                    <MenuItem
                                                        key={2}
                                                        value={'ConsumerNumber'}
                                                        onClick={() => {
                                                            TextDisplay(
                                                                setFieldValue
                                                            );
                                                        }}
                                                    >
                                                        ConsumerNumber
                                                    </MenuItem>

                                                    <MenuItem
                                                        key={3}
                                                        value={'ReferenceID'}
                                                        onClick={() => {
                                                            TextDisplay(
                                                                setFieldValue
                                                            );
                                                        }}
                                                    >
                                                        ReferenceID
                                                    </MenuItem>
                                                </MaterialSelect>
                                            </Grid>
                                            {IsDisplayCriteria && (
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    <MaterialTextField
                                                        label={
                                                            CriteriaTextLabel
                                                        }
                                                        name="CriteriaText"
                                                        type="text"
                                                        placeholder={
                                                            'Enter ' +
                                                            CriteriaTextLabel
                                                        }
                                                    />
                                                </Grid>
                                            )}
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialSelect
                                                    label="ExportType"
                                                    name="ExportType"
                                                    title="ExportType"
                                                >
                                                    {ExportTypeItems}
                                                </MaterialSelect>
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={1}
                                                lg={1}
                                            >
                                                <PrimaryButton
                                                    type="submit"
                                                    title="Search"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    disabled={loading}
                                                // onClick={(e) => {
                                                //     setFieldValue(
                                                //         'IsExcelDownload',
                                                //         false
                                                //     );
                                                //     handleSubmit(e);
                                                // }}
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
                getSearchCallback={({ SearchText }) => {
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
