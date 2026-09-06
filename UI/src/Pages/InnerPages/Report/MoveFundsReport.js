
import React, { useState, useEffect, useRef } from 'react';
import * as Yup from 'yup';
import { Formik, Form, Field } from 'formik';
import {
    Container,
    Grid,
    Button,
    Paper,
    MenuItem,
    CircularProgress,
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import {
    SelectWalletTransferReport as TRANSFER_REPORT,
} from 'Components/Api/FundApi';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { HeaderFundSummary } from 'Components/SummaryCards/HeaderFundSummary';
import { PLEASE_SELECT_VALID, SUPPORT_CODE } from 'Components/Helper/Messages';
import {
    ADMIN,
    RETAILER,
    API_USER,
    ACCOUNTANT,
    SUPER_DISTRIBUTOR,
    DISTRIBUTOR
} from 'Components/Helper/UsertypeCode';
import { useHistory } from 'react-router';
import Accordion from '@material-ui/core/Accordion';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import HOCLoader from 'Components/Loader/HOCLoader';
import { SelectByReceiverID as SELECT_PAYMENT_MODE_BYID } from 'Components/Api/PaymentModesAPI';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { FETCH_LIST } from 'Config';
import { ExportTypeItems } from 'Components/ReportHelper';
import { MAIN_WalletID } from 'Components/Helper/Messages';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const MoveFundsReport = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const history = useHistory();
    const [requests, setRequests] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [headerSummary, setHeaderSummary] = useState({});
    const [loading, setLoading] = useState(false);
    const [SearchText] = useState('');
    const [userList, setUserList] = useState([]);

    const [currentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );

    const [IsFetchAllList, setisFetchAllList] = useState(false);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select Entity'
    });
    var ModelObj = {
        ToDate: new Date(),
        FromDate: new Date(),
        Status: 'ALL',
        Type: 'RECEIVED',
        TakeCount: TakeCount,
        SkipCount: SkipCount,
        ExportType: 'Select Download',
        User: {
            UserID: 0,
            FirmName: 'Select Entity'
        },
        WalletID: MAIN_WalletID,
        ReportFor: '0',
        PayMode: 'ALL'
    };

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
        if (SUPPORT_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
        if (RETAILER === parseInt(currentUserTypeID)) setIsRetailerRole(true);
        if (API_USER === parseInt(currentUserTypeID)) setIsRetailerRole(true);
        if (ADMIN === parseInt(currentUserTypeID)) setIsAdminRole(true);
        if (ACCOUNTANT === parseInt(currentUserTypeID)) setIsAdminRole(true);
    };


    const callSelectData = async (
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
            UserID: formRef.current.values.User.UserID,
            Type: formRef.current.values.Type,
            WalletID: formRef.current.values.WalletID,
            PayMode: formRef.current.values.PayMode,
            ReportFor: formRef.current.values.ReportFor,
            ExportType: formRef.current.values.ExportType
        };
        setRequests([]);
        var request = JSON.stringify(requestObj);

        if (requestObj.SkipCount <= 0) {
            setHeaderSummary({});
        }
        //  setRequests([]);
        setIsError(false);
        setLoading(true);
        const response = await TRANSFER_REPORT(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);

        if (data.IsDownload) {
            downloadFile(data.ReportPath, 'Fund Transfer Report');
            return;
        }
        if (requestObj.SkipCount <= 0) {
            setHeaderSummary(data.HeaderViewModel);
            setTotalCount(data.Count);
        }

        setRequests(data.data);
    };
    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }
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
    const SelectUser = async (values) => {
        if (IsFetchAllList === true) {
            values = '';
        }

        if (RETAILER === parseInt(currentUserTypeID)) {
            let objWithSelectAll = [userDefault];
            setUserList(objWithSelectAll);
            return;
        }

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

    useEffect(() => {
        CheckUserRole();
        fetchUserList();

        createColumns();
        refreshForm();
    }, [isRetailerRole, isAdminRole]);

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
            label: 'Date & Time',
            name: 'DOC'
        });

        columns.push({
            label: 'Entity Name',
            name: 'Sender'
        });
        columns.push({
            label: 'Wallet',
            name: 'Wallet'
        });

        columns.push({
            label: 'Original Amt (₹)',
            name: 'OriginalAmount',
            numeric: true
        });

        columns.push({
            label: 'Transfer Amt (₹)',
            name: 'Amount',
            numeric: true
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        columns.push({
            label: 'S.Remark',
            name: 'SenderRemark'
        });
        columns.push({
            label: 'R.Remark',
            name: 'Remark'
        });
        if (!isRetailerRole) {
            columns.push({
                label: 'S.OpBal',
                name: 'SenderOpeningBalance',
                numeric: true
            });
            columns.push({
                label: 'S.ClBal',
                name: 'SenderClosingBalance',
                numeric: true
            });
        }

        columns.push({
            label: 'R.OpBal',
            name: 'ReceiverOpeningBalance',
            numeric: true
        });
        columns.push({
            label: 'R.ClBal',
            name: 'ReceiverClosingBalance',
            numeric: true
        });

        columns.push({
            label: 'Ref.ID',
            name: 'FundTransactionID'
        });
        setMainColumns(columns);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        values.UserID = values.User.UserID;
        callSelectData(TakeCount, SkipCount, '', '', '');
        setSubmitting(false);
    };
    const [openFilter, setopenFilter] = useState(true);
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
                        <Form
                            style={{
                                marginBottom: '15px'
                            }}
                        >
                            <Grid container spacing={2}>
                                <Grid item xs={6} sm={6} md={6} lg={6}></Grid>
                                {isAdminRole ? null : (
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
                                                        '/WalletTransfer'
                                                    );
                                                }}
                                            >
                                                Transfer to e-Balance
                                            </Button>
                                        </h2>
                                    </Grid>
                                )}
                            </Grid>

                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'Transfer to e-Balance Report'}
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

                                            {!isRetailerRole && (
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
                                                            label: 'Select Entity'
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
            <HeaderFundSummary headerSummary={headerSummary} />
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
