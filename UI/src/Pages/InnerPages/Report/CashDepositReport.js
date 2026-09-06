import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { HeaderSummary } from 'Components/SummaryCards/HeaderSummary';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectCashDepositForFilter as TRANSACTION_FILTER } from 'Components/Api/TransactionReportAPI';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import { GetCashDespositTransactionStatus as TRANSACTION_STATUS } from 'Components/Api/TransactionAPI';
import { SelectForReceipt as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { PayoutPrint } from 'Components/Print/PayoutPrint';
import { Container, Grid, MenuItem, Accordion, Box } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { AddComplaint } from 'Pages/InnerPages/Complaints/Add';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import {
    PLEASE_SELECT_VALID,
    RETAILER_CODE,
    API_USER_CODE,
    ADMIN_CODE,
    DIST_CODE,
    SUPERDIST_CODE
} from 'Components/Helper/Messages';
import { CustomModal } from 'Components/Modal/CustomModal';
import HOCLoader from 'Components/Loader/HOCLoader';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { StatusItems } from 'Components/ReportHelper';

import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { FETCH_LIST } from 'Config';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

const CashDepositReport = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [IsFetchAllList, setisFetchAllList] = useState(false);
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [openFilter, setopenFilter] = useState(true);
    const [currentPage, setCurrentPage] = useState(0);
    const [userList, setUserList] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [isDistRole, setIsDistRole] = useState(false);
    const [providerList, setProviderList] = useState([]);
    const [headerSummary, setHeaderSummary] = useState({});
    const [printData, setPrintData] = useState(null);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState(false);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ServiceID: 0,
        OperatorID: 0,
        Status: 'ALL',
        ProviderID: 0,
        IsExcelDownload: false,
        IsRechargeReport: false,
        Criteria: 0,
        CriteriaText: '',
        User: userDefault
    });
    const selectProvider = async () => {
        if (ADMIN_CODE !== parseInt(currentUsertypeid)) return;

        setLoading(true);
        const response = await PROVIDER_DROP_DOWN();
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
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
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
        }
        if (ADMIN_CODE === parseInt(currentUsertypeid)) {
            setIsAdminRole(true);
        }
        if (API_USER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
        }
        if (DIST_CODE === parseInt(currentUsertypeid)) setIsDistRole(true);
        if (SUPERDIST_CODE === parseInt(currentUsertypeid)) setIsDistRole(true);
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);
        createColumns();
        callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
    };

    const showUserList = () => {
        if (isDistRole === true) return true;
        if (isAdminRole === true) return true;
    };
    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let inputObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            SearchText: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            Status: formRef.current.values.Status,
            UserID: formRef.current.values.User.UserID,
            ProviderID: formRef.current.values.ProviderID,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText,
            IsExcelDownload: formRef.current.values.IsExcelDownload
        };
        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await TRANSACTION_FILTER(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        if (SkipCount <= 0) {
            setHeaderSummary({});
        }
        var data = JSON.parse(response.data);
        if (data.IsExcelDownload) {
            formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'CashDeposit Report');
        }

        if (SkipCount <= 0) {
            setHeaderSummary(data.HeaderSummary);
        }

        setTransactions(data.list);
        setTotalCount(data.Count);
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
        var currentUsertypeid = localStorage.getItem('UserTypeID');

        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
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

    useEffect(() => {
        fetchUserList();
        CheckUserRole();
        selectProvider();
        createColumns();
    }, []);
    const SelectPrintData = async (TransactionMasterID) => {
        var req = {
            ID: TransactionMasterID
        };
        var request = JSON.stringify(req);
        setLoading(true);
        const { isAuthorized, status, data } = await TRANSACTION_RECEIPT(
            request
        );
        setLoading(false);
        if (!isAuthorized) LOGOUT();

        // if (!status) return;

        var d = JSON.parse(data);
        setPrintData(d);

        setIsAlertOpen(true);
    };

    const Print = (value) => {
        SelectPrintData(value);
    };
    const createColumns = () => {
        var columns = [];
        var statusIndex = 1;

        columns.push({
            label: 'Status',
            name: 'Status',
            hide: true
        });
        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Name',
            name: 'FirmName'
        });
        columns.push({
            label: 'Number',
            name: 'ConsumerNumber',
            bold: true
        });

        columns.push({
            label: 'Bene',
            name: 'BeneficaryName'
        });

        columns.push({
            label: 'Op Bal',
            name: 'OpeningBalance',
            textwrapwidth: 70,
            numeric: true
        });

        columns.push({
            label: 'Amt',
            name: 'Amount',
            textwrapwidth: 60,
            numeric: true
        });

        columns.push({
            label: 'Debit',
            name: 'DebitAmount',
            textwrapwidth: 60,
            numeric: true
        });

        columns.push({
            label: 'Cl Bal',
            name: 'ClosingBalance',
            textwrapwidth: 70,
            numeric: true
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        columns.push({
            label: 'Ref No',
            name: 'ReferenceNo'
        });
        columns.push({
            label: 'Remark',
            name: 'Description'
        });

        if (isAdminRole === true) {
            columns.push({
                label: 'Provider',
                name: 'ProviderName'
            });
            columns.push({
                label: 'RequestID',
                name: 'ProviderRequestID'
            });
        }

        var options = [
            {
                Title: 'Print',
                disabledOn: ['FAILURE'],
                statusIndex: statusIndex,
                Callback: (values) => {
                    Print(values[0]);
                }
            },
            {
                Title: 'Check',
                disabledOn: ['FAILURE', 'SUCCESS'],
                statusIndex: statusIndex,
                Callback: (values) => {
                    StatusCheck(values[0]);
                }
            }
        ];
        columns.splice(0, 0, {
            label: ' ',
            name: 'TransactionMasterID',
            MoreMenu: true,
            options: options
        });

        setMainColumns(columns);
    };

    const StatusCheck = async (ID) => {
        var inputObj = {
            ID: ID
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);
        setResponseMessage('');

        var response = await TRANSACTION_STATUS(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);
        setResponseMessage(data.Message);
        if (!data.Status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        setSeverity('success');
        setIsError(true);

        refreshForm();
    };

    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
    };

    const [TransactionMasterID, setTransactionMasterID] = useState(0);
    const [ConsumerNumber, setConsumerNumber] = useState('');
    const [Status, setStatus] = useState('');
    const [Amount, setAmount] = useState(0);

    const RaiseComplaint = (ID, ConsumerNumber, Amount, Status) => {
        setTransactionMasterID(ID);
        setConsumerNumber(ConsumerNumber);
        setStatus(Status);
        setAmount(Amount);
        setIsOpen(true);
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
    const loadingProgress = loading && (
        <CircularProgress size={24} className={classes.buttonProgress} />
    );

    const CashDepositForm = ({ setFieldValue, handleSubmit }) => {
        return (
            <Form style={{ marginBottom: '15px' }}>
                <Grid item xs={12}>
                    <CustomAccordion
                        title={'Cash Deposit Report'}
                        open={openFilter}
                        setOpen={(value) => setopenFilter(value)}
                    >
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="Status"
                                    name="Status"
                                    title="Status"
                                >
                                    {StatusItems}
                                </MaterialSelect>
                            </Grid>

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialDatePickerField
                                    name="FromDate"
                                    label="From Date"
                                    maxDate={new Date()}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialDatePickerField
                                    name="ToDate"
                                    label="To Date"
                                    maxDate={new Date()}
                                />
                            </Grid>

                            {showUserList() ? (
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <Field
                                        component={AsyncAutoComplete}
                                        defaultValue={userDefault}
                                        FieldName="User"
                                        name="User"
                                        SubFieldName="UserID"
                                        options={userList}
                                        getOptionLabel={(option) =>
                                            option.FirmName
                                        }
                                        textFieldProps={{
                                            label: 'Select User'
                                        }}
                                        onSearchCallback={(text) => {
                                            callSearch(text);
                                        }}
                                        onCustomCallback={(name, value) => {
                                            setUserDefault({
                                                UserID: value.UserID,
                                                FirmName: value.FirmName
                                            });
                                        }}
                                    />
                                </Grid>
                            ) : null}

                            {isAdminRole ? (
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <MaterialSelect
                                        label="Provider"
                                        name="ProviderID"
                                    >
                                        <MenuItem key="0" value="0">
                                            All
                                        </MenuItem>
                                        {providerList.map(
                                            (
                                                { ProviderID, ProviderName },
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
                            ) : null}

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="Selection Criteria"
                                    name="Criteria"
                                    title="criteria"
                                    onCustomCallback={(name, value) => {
                                        OnChangeCriteria(value);
                                    }}
                                >
                                    <MenuItem key={0} value={0}>
                                        Selection Criteria
                                    </MenuItem>
                                    <MenuItem key={1} value={'TransactionID'}>
                                        TransactionID
                                    </MenuItem>
                                </MaterialSelect>
                            </Grid>

                            {!IsDisplayCriteria ? null : (
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <MaterialTextField
                                        label={CriteriaTextLabel}
                                        name="CriteriaText"
                                        type="text"
                                        placeholder={
                                            'Enter ' + CriteriaTextLabel
                                        }
                                    />
                                </Grid>
                            )}
                            <Grid item xs={12} sm={12} md={1} lg={1}>
                                <Box style={{ textAlign: 'center' }}>
                                    <Icon
                                        className={
                                            classes.CustomImageButtonStyle
                                        }
                                        disabled={loading}
                                        classes={{ root: classes.iconRoot }}
                                        onClick={(e) => {
                                            setFieldValue(
                                                'IsExcelDownload',
                                                true
                                            );
                                            handleSubmit(e);
                                        }}
                                    >
                                        <img
                                            alt=""
                                            className={classes.imageIcon}
                                            src={excel}
                                        />
                                    </Icon>
                                </Box>
                            </Grid>
                            <Grid item xs={12} sm={12} md={1} lg={1}>
                                <PrimaryButton
                                    type="submit"
                                    title="Search"
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={loading}
                                    onClick={(e) => {
                                        setFieldValue('IsExcelDownload', false);
                                        handleSubmit(e);
                                    }}
                                >
                                    Search
                                </PrimaryButton>
                                {loadingProgress}
                            </Grid>
                        </Grid>
                    </CustomAccordion>
                </Grid>
            </Form>
        );
    };
    const customContainerCallback = (alertState) => {
        setIsError(alertState);
    };
    return (
        <CustomContainer
            isLoading={loading}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={customContainerCallback}
        >
            <Formik
                initialValues={ModelObj}
                innerRef={formRef}
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={ModelObj}
            >
                {CashDepositForm}
            </Formik>
            <HeaderSummary headerSummary={headerSummary} />

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
            />

            {isOpen ? (
                <CustomModal
                    isOpen={isOpen}
                    title="Complaint"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsOpen(value);
                    }}
                >
                    <AddComplaint
                        ComplaintMasterID={0}
                        TransactionMasterID={TransactionMasterID}
                        ConsumerNumber={ConsumerNumber}
                        Status={Status}
                        Amount={Amount}
                        Remark=""
                        AdmnRemark=""
                    />
                </CustomModal>
            ) : null}
            {isAlertOpen ? (
                <PayoutPrint
                    isOpen={isAlertOpen}
                    title="Cash Deposit Print"
                    request={printData}
                    positiveButtonClick={(printData) => {
                        setIsAlertOpen(false);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
export default CashDepositReport;
