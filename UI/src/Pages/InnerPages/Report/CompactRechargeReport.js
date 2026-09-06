import React, { useState, useEffect, useRef } from 'react';

import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { TransactionPrint } from 'Components/Print/RechargePrint';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectForRechargeCompactFilter as TRANSACTION_FILTER } from 'Components/Api/TransactionReportAPI';
import BBPS_HORIZONTAL_LOGO from 'Assets/Images/BBPS/BBPS-Horizontal-06.png';
import { SelectByRechargeServices as SERVICE_SELECT_RECHARGE } from 'Components/Api/ServiceApi';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { ResendCallback as RESEND_CALLBACK } from 'Components/Api/APIUserCallbackApi';
import { SelectForReceipt as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';

import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { StatusEdit } from 'Pages/InnerPages/Report/ManualStatusEdit';
import {
    Container,
    Grid,
    MenuItem,
    CircularProgress,
    Box
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_SELECT_VALID,
    ADMIN_CODE,
    RETAILER_CODE,
    API_USER_CODE,
    DTH_RECHARGE_CODE,
    DIST_CODE,
    SUPERDIST_CODE
} from 'Components/Helper/Messages';
import { CustomModal } from 'Components/Modal/CustomModal';
import { SelectByIDLog } from 'Pages/InnerPages/Report/LogByTransactionMasterID';
import { AddComplaint } from 'Pages/InnerPages/Complaints/Add';
import { DthReversal } from 'Pages/InnerPages/Complaints/AddDthReversal';
import HOCLoader from 'Components/Loader/HOCLoader';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { HeaderSummary } from 'Components/SummaryCards/HeaderSummary';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { StatusItems, ComplaintItems } from 'Components/ReportHelper';
import { FETCH_LIST } from 'Config';
import { PrimaryButton } from 'Components/Inputes/CustomButton';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);
export const CompactRechargeReport = () => {
    const formRef = useRef();
    const classes = commonStyle();
    var IsMobikwikBBPS = localStorage.getItem('IsMobikwikBBPS') === 'true';
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [Services, setServices] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);

    const [userList, setUserList] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [isReversal, setIsReversal] = useState(false);

    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [isDistRole, setIsDistRole] = useState(false);

    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [isAPIRole, setIsAPIRole] = useState(false);
    const [IsFetchAllList, setisFetchAllList] = useState(false);
    const [isSuccessFailOpen, setIsSuccessFailOpen] = useState(false);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [transactionMasterID, setTransactionMasterID] = useState(0);
    const [consumerNumber, setConsumerNumber] = useState(0);
    const [Amount, setAmount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [providerList, setProviderList] = useState([]);
    const [headerSummary, setHeaderSummary] = useState({});
    const [openFilter, setOpenFilter] = useState(true);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);

    const [SearchText, setSearchText] = useState('');

    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState(false);
    const [printData, setPrintData] = useState(null);
    const [isAlertOpen, setIsAlertOpen] = useState(false);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ServiceID: 0,
        OperatorID: 0,
        Status: 'ALL',
        Complaint: 'ALL',
        ProviderID: 0,
        IsExcelDownload: false,
        IsRechargeReport: false,

        Amount: 0,
        Criteria: 0,
        CriteriaText: '',
        User: {
            UserID: 0,
            FirmName: 'Select User'
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
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);

        if (RETAILER_CODE === parseInt(currentUsertypeid))
            setIsRetailerRole(true);
        if (API_USER_CODE === parseInt(currentUsertypeid)) setIsAPIRole(true);
        if (DIST_CODE === parseInt(currentUsertypeid)) setIsDistRole(true);
        if (SUPERDIST_CODE === parseInt(currentUsertypeid)) setIsDistRole(true);
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
            Complaint: formRef.current.values.Complaint,
            UserID: formRef.current.values.User.UserID,
            ServiceID: formRef.current.values.ServiceID,
            OperatorID: formRef.current.values.OperatorID,
            ProviderID: formRef.current.values.ProviderID,
            IsExcelDownload: formRef.current.values.IsExcelDownload,
            IsRechargeReport: formRef.current.values.IsExcelDownload,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText,
            Amount: formRef.current.values.Amount
        };

        if (SkipCount <= 0) {
            setHeaderSummary({});
        }
        setIsError(false);
        setLoading(true);

        var requestJSON = JSON.stringify(requestObj);

        var response = await TRANSACTION_FILTER(requestJSON);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        setSeverity('success');
        var data = JSON.parse(response.data);

        if (data.IsExcelDownload) {
            downloadFile(data.ReportPath, 'Recharge Report');
            return;
        }
        if (SkipCount <= 0) {
            setHeaderSummary(data.HeaderSummary);
            setTotalCount(data.Count);
        }

        setTransactions(data.list);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);
        createColumns();

        await callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
        document.removeChild(a);
    }

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

    const selectServices = async () => {
        const response = await SERVICE_SELECT_RECHARGE();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const OnOperatorChange = async (name, value, setFieldValue) => {
        var ServiceID = value; //event.target.value;

        setFieldValue(name, value);

        selectOperator(ServiceID);
    };

    const selectOperator = async (ServiceID) => {
        if (
            ServiceID === null ||
            ServiceID === undefined ||
            ServiceID === 0 ||
            ServiceID === '0'
        )
            return;

        var requestObj = {
            ServiceID: ServiceID
        };
        var request = JSON.stringify(requestObj);

        const response = await OPERATOR_SELECT_BY_SERVICEID(request);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) setIsError(true);

        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);
            setOperator(data);
        }
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

    const isValidToRefreshList = (list) => {
        if (IsFetchAllList === false) {
            return true;
        }

        if (list === undefined || list === null) {
            return true;
        }

        if (list.length <= 1) {
            return true;
        }

        return false;
    };

    const SelectUser = async (text) => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setUserList(objWithSelectAll);
            return;
        }

        var jsonObj = {
            FirmName: text
        };

        if (isValidToRefreshList(userList) === false) {
            return;
        }

        setLoading(true);
        var request = JSON.stringify(jsonObj);
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
    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
    };

    useEffect(() => {
        createColumns();
        fetchUserList();
        CheckUserRole();
        selectServices();
        selectProvider();
        refreshForm();
    }, []);

    const Print = (value) => {
        SelectPrintData(value);
    };
    const ResendCallback = async (TransactionMasterID) => {
        let jsonObj = {
            TransactionMasterID: TransactionMasterID
        };
        var request = JSON.stringify(jsonObj);

        const response = await RESEND_CALLBACK(request);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
    };

    const logClick = async (values) => {
        setIsModalOpen(true);
        setTransactionMasterID(values[0]);
        setConsumerNumber(values[7]);
        setAmount(values[8]);
    };

    const createColumns = () => {
        var columns = [];
        var statusIndex = 1;
        var enableIndex = 2;

        columns.push({
            label: 'Status',
            name: 'Status',
            hide: true
        });

        columns.push({
            label: 'ServiceID',
            name: 'ServiceID',
            hide: true
        });

        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            srno: true
        });
        columns.push({
            label: 'UserID',
            name: 'UserID',
            hide: true
        });
        columns.push({
            label: 'Name',
            name: 'FirmName',
            hide: true
        });
        columns.push({
            label: 'Operator',
            name: 'OperatorName',
            hide: true
        });
        columns.push({
            label: 'Number',
            name: 'ConsumerNumber',
            hide: true
        });
        columns.push({
            label: 'Amt',
            name: 'Amount',
            hide: true
        });

        columns.push({
            label: 'Date',
            name: 'Date'
        });
        columns.push({
            label: 'Name',
            name: 'Name'
        });

        columns.push({
            label: 'Image',
            name: 'Logo',
            Image: true
        });
        columns.push({
            label: 'BBPS',
            name: 'BBPSLogo',
            BBPSImage: true
        });
        columns.push({
            label: 'Operator',
            name: 'Operator'
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });
        columns.push({
            label: 'TxnID',
            name: 'TxnID'
        });
        columns.push({
            label: 'Amount',
            name: 'AmountInfo'
        });

        if (isRetailerRole === true || isAPIRole === true) {
            columns.push({
                label: 'Comm',
                name: 'RetailerCommission',
                textwrapwidth: 60,
                numeric: true
            });
        }
        if (isAdminRole === true || isDistRole === true) {
            columns.push({
                label: 'Comm.Info',
                name: 'CommissionInfo'
            });
        }
        columns.push({
            label: 'Balance',
            name: 'BalanceInfo'
        });
        if (isAdminRole === true) {
            columns.push({
                label: 'Provider',
                name: 'ProviderInfo'
            });
            columns.push({
                label: 'ROffer',
                name: 'ROfferCommission'
            });
        }

        if (isAdminRole === true) {
            columns.push({
                label: 'API Bal',
                name: 'APIBalInfo'
            });
            columns.push({
                label: 'UpdateBy',
                name: 'ResponseType'
            });
        }
        if (isAPIRole === true) {
            columns.push({
                label: 'APIReq',
                name: 'APIUserRequestID'
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
            }
        ];

        options.push({
            Title: 'Complaint',
            disabledOn: ['FAILURE', 'PROCESS'],
            statusIndex: statusIndex,
            Callback: (values) => {
                RaiseComplaint(values[0]);
            }
        });
        options.push({
            Title: 'Wrong 2 Right',
            enableOn: [DTH_RECHARGE_CODE],
            statusIndex: enableIndex,
            Callback: (values) => {
                ReversalComplaint(values[0]);
            }
        });
        if (isAdminRole === true) {
            options.push({
                Title: 'Success/Fail',
                disabledOn: ['SUCCESS', 'FAILURE'],
                statusIndex: statusIndex,
                Callback: (values) => {
                    Edit(values[0]);
                }
            });
        }
        if (isAdminRole === true || isAPIRole === true) {
            options.push({
                Title: 'Log',
                Callback: (values) => {
                    logClick(values);
                }
            });
            options.push({
                Title: 'Hit Callback',
                Callback: (values) => {
                    ResendCallback(values[0]);
                }
            });
        }

        columns.splice(0, 0, {
            label: ' ',
            name: 'TransactionMasterID',
            MoreMenu: true,
            options: options
        });

        setMainColumns(columns);
    };
    const Edit = async (ID) => {
        setTransactionMasterID(ID);
        setIsSuccessFailOpen(true);
    };

    const RaiseComplaint = (ID) => {
        setTransactionMasterID(ID);
        setIsOpen(true);
    };
    const ReversalComplaint = (ID) => {
        setTransactionMasterID(ID);
        setIsReversal(true);
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
    const callSearch = async (text) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            text.length <= 0 ||
            text === userDefault.FirmName
        )
            return;

        SelectUser(text);
    };

    const BBPSHorizontalLogoImage = () => {
        return (
            <img
                src={BBPS_HORIZONTAL_LOGO}
                style={{
                    float: 'right',
                    marginRight: '25px',
                    marginBottom: '25px'
                }}
                alt="BBPS"
            />
        );
    };
    return (
        <>
            {IsMobikwikBBPS && (
                <Grid
                    item
                    xs={12}
                    sm={12}
                    md={12}
                    lg={12}
                    xl={12}
                    style={{ marginBottom: '30px', marginTop: '20px' }}
                >
                    <BBPSHorizontalLogoImage />
                </Grid>
            )}
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
                    {({ setFieldValue, handleSubmit }) => {
                        return (
                            <Form>
                                <Grid item xs={12}>
                                    <CustomAccordion
                                        title={'Recharge Report'}
                                        open={openFilter}
                                        setOpen={(value) =>
                                            setOpenFilter(value)
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
                                                <MaterialSelect
                                                    label="Service"
                                                    name="ServiceID"
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        OnOperatorChange(
                                                            name,
                                                            value,
                                                            setFieldValue
                                                        );
                                                    }}
                                                    title="Service"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        All Service
                                                    </MenuItem>
                                                    {Services.map((Item) => (
                                                        <MenuItem
                                                            key={Item.ServiceID}
                                                            value={
                                                                Item.ServiceID
                                                            }
                                                        >
                                                            {Item.ServiceName}
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
                                                    label="Operator"
                                                    name="OperatorID"
                                                    title="Operator"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        All Operator
                                                    </MenuItem>
                                                    {Operator.map((Item) => (
                                                        <MenuItem
                                                            key={
                                                                Item.OperatorID
                                                            }
                                                            value={
                                                                Item.OperatorID
                                                            }
                                                        >
                                                            {Item.OperatorName}
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
                                                    label="Status"
                                                    name="Status"
                                                    title="Status"
                                                >
                                                    {StatusItems}
                                                </MaterialSelect>
                                            </Grid>

                                            {isAdminRole ? (
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
                                            {isAdminRole ? (
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    <MaterialSelect
                                                        label="Provider"
                                                        name="ProviderID"
                                                    >
                                                        <MenuItem
                                                            key="0"
                                                            value="0"
                                                        >
                                                            All
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
                                                                    value={
                                                                        ProviderID
                                                                    }
                                                                >
                                                                    {
                                                                        ProviderName
                                                                    }
                                                                </MenuItem>
                                                            )
                                                        )}
                                                    </MaterialSelect>
                                                </Grid>
                                            ) : null}
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialNumberField
                                                    label="Amount"
                                                    name="Amount"
                                                    type="text"
                                                    length={7}
                                                    placeholder="Amount"
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
                                                    label="Complaint"
                                                    name="Complaint"
                                                    title="Complaint"
                                                >
                                                    {ComplaintItems}
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
                                                    >
                                                        TransactionID
                                                    </MenuItem>
                                                    <MenuItem
                                                        key={2}
                                                        value={'ConsumerNumber'}
                                                    >
                                                        ConsumerNumber
                                                    </MenuItem>
                                                    <MenuItem
                                                        key={3}
                                                        value={
                                                            'APIUserRequestID'
                                                        }
                                                    >
                                                        APIUserRequestID
                                                    </MenuItem>
                                                </MaterialSelect>
                                            </Grid>

                                            {!IsDisplayCriteria ? null : (
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
                                                md={1}
                                                lg={1}
                                            >
                                                <Box
                                                    style={{
                                                        textAlign: 'center'
                                                    }}
                                                >
                                                    <Icon
                                                        className={
                                                            classes.CustomImageButtonStyle
                                                        }
                                                        disabled={loading}
                                                        classes={{
                                                            root:
                                                                classes.iconRoot
                                                        }}
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
                                                            className={
                                                                classes.imageIcon
                                                            }
                                                            src={excel}
                                                        />
                                                    </Icon>
                                                </Box>
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
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'IsExcelDownload',
                                                            false
                                                        );
                                                        handleSubmit(e);
                                                    }}
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
                                </Grid>

                                <Grid className={classes.footerGrid}>
                                    <Box className={classes.flexReverse}>
                                        <div
                                            className={classes.SubmitWrapper}
                                        ></div>
                                    </Box>
                                </Grid>
                            </Form>
                        );
                    }}
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
                    getSearchCallback={({ SearchText }) => {
                        callSelectData(
                            currentPage,
                            TakeCount,
                            SearchText,
                            '',
                            ''
                        );
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

                {isModalOpen ? (
                    <CustomModal
                        isOpen={isModalOpen}
                        title="API Log"
                        width="75%"
                        onDismissCallback={(value) => {
                            setIsModalOpen(value);
                        }}
                    >
                        <SelectByIDLog
                            TransactionMasterID={transactionMasterID}
                            ConsumerNumber={consumerNumber}
                            Amount={Amount}
                            setLoading={(value) => setLoading(value)}
                        />
                    </CustomModal>
                ) : null}
                {isSuccessFailOpen ? (
                    <CustomModal
                        isOpen={isSuccessFailOpen}
                        title="Manual Status update"
                        width="75%"
                        onDismissCallback={(value) => {
                            setIsSuccessFailOpen(value);
                            refreshForm();
                        }}
                    >
                        <StatusEdit
                            TransactiomMasterID={transactionMasterID}
                            onDismissCallback={(value) => {
                                setIsSuccessFailOpen(value);
                            }}
                        />
                    </CustomModal>
                ) : null}
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
                            IsEdit={false}
                            TransactionMasterID={transactionMasterID}
                            ComplaintMasterID={0}
                            onDismissCallback={(value) => {
                                setIsOpen(value);
                            }}
                        />
                    </CustomModal>
                ) : null}
                {isReversal ? (
                    <CustomModal
                        isOpen={isReversal}
                        title="Complaint"
                        width="75%"
                        onDismissCallback={(value) => {
                            setIsReversal(value);
                        }}
                    >
                        <DthReversal
                            IsEdit={false}
                            TransactionMasterID={transactionMasterID}
                            ComplaintMasterID={0}
                            onDismissCallback={(value) => {
                                setIsReversal(value);
                            }}
                        />
                    </CustomModal>
                ) : null}

                {isAlertOpen ? (
                    <TransactionPrint
                        isOpen={isAlertOpen}
                        title="Transaction Print"
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
        </>
    );
};
